﻿#region Header
//
//   Project:           WriteableBitmapEx - WriteableBitmap extensions
//   Description:       Collection of extension methods for the WriteableBitmap class.
//
//   Changed by:        $Author$
//   Changed on:        $Date$
//   Changed in:        $Revision$
//   Project:           $URL$
//   Id:                $Id$
//
//
//   Copyright © 2009-2015 Rene Schulte and WriteableBitmapEx Contributors
//
//   This code is open source. Please read the License.txt for details. No worries, we won't sue you! ;)
//
#endregion

#define WPF

using System;
using System.Collections.Generic;

#if NETFX_CORE
using System.Runtime.InteropServices.WindowsRuntime;
using System.Collections.Concurrent;

namespace Windows.UI.Xaml.Media.Imaging
#else
namespace System.Windows.Media.Imaging
#endif
{
    /// <summary>
    /// Read Write Mode for the BitmapContext.
    /// </summary>
    public enum ReadWriteMode
    {
        /// <summary>
        /// On Dispose of a BitmapContext, do not Invalidate
        /// </summary>
        ReadOnly,

        /// <summary>
        /// On Dispose of a BitmapContext, invalidate the bitmap
        /// </summary>
        ReadWrite,
    }

    /// <summary>
    /// A disposable cross-platform wrapper around a WriteableBitmap, allowing a common API for Silverlight + WPF with locking + unlocking if necessary
    /// </summary>
    /// <remarks>Attempting to put as many preprocessor hacks in this file, to keep the rest of the codebase relatively clean</remarks>
    public
#if WPF
 unsafe
#endif
 struct BitmapContext : IDisposable
    {
        private readonly WriteableBitmap writeableBitmap;
        private readonly ReadWriteMode mode;

        private readonly int pixelWidth;
        private readonly int pixelHeight;

#if WPF
        private readonly static IDictionary<WriteableBitmap, int> UpdateCountByBmp = new System.Collections.Concurrent.ConcurrentDictionary<WriteableBitmap, int>();

        private readonly int _length;
        private readonly int* _backBuffer;
#elif NETFX_CORE
        private readonly static IDictionary<WriteableBitmap, int> UpdateCountByBmp = new ConcurrentDictionary<WriteableBitmap, int>();
        private readonly static IDictionary<WriteableBitmap, int[]> PixelCacheByBmp = new ConcurrentDictionary<WriteableBitmap, int[]>();
        private int length;
        private int[] pixels;
#endif

        /// <summary>
        /// The Bitmap
        /// </summary>
        public WriteableBitmap WriteableBitmap => this.writeableBitmap;

        /// <summary>
        /// Width of the bitmap
        /// </summary>
        public int Width => this.writeableBitmap.PixelWidth;

        /// <summary>
        /// Height of the bitmap
        /// </summary>
        public int Height => this.writeableBitmap.PixelHeight;

        /// <summary>
        /// Creates an instance of a BitmapContext, with default mode = ReadWrite
        /// </summary>
        /// <param name="writeableBitmap"></param>
        public BitmapContext(WriteableBitmap writeableBitmap)
            : this(writeableBitmap, ReadWriteMode.ReadWrite)
        {
        }

        /// <summary>
        /// Creates an instance of a BitmapContext, with specified ReadWriteMode
        /// </summary>
        /// <param name="writeableBitmap"></param>
        /// <param name="mode"></param>
        public BitmapContext(WriteableBitmap writeableBitmap, ReadWriteMode mode)
        {
            this.writeableBitmap = writeableBitmap;
            this.mode = mode;

            this.pixelWidth = this.writeableBitmap.PixelWidth;
            this.pixelHeight = this.writeableBitmap.PixelHeight;
#if WPF
            //// Check if it's the Pbgra32 pixel format
            //if (writeableBitmap.Format != PixelFormats.Pbgra32)
            //{
            //   throw new ArgumentException("The input WriteableBitmap needs to have the Pbgra32 pixel format. Use the BitmapFactory.ConvertToPbgra32Format method to automatically convert any input BitmapSource to the right format accepted by this class.", "writeableBitmap");
            //}

            double width = this.writeableBitmap.BackBufferStride / WriteableBitmapExtensions.SizeOfArgb;
            this._length = (int)(width * this.pixelHeight);

            lock (UpdateCountByBmp)
            {
                // Ensure the bitmap is in the dictionary of mapped Instances
                if (!UpdateCountByBmp.ContainsKey(writeableBitmap))
                {
                    // Set UpdateCount to 1 for this bitmap 
                    UpdateCountByBmp.Add(writeableBitmap, 1);

                    // Lock the bitmap
                    writeableBitmap.Lock();
                }
                else
                {
                    // For previously contextualized bitmaps increment the update count
                    IncrementRefCount(writeableBitmap);
                }
            }

            this._backBuffer = (int*)writeableBitmap.BackBuffer;
#elif NETFX_CORE
            // Ensure the bitmap is in the dictionary of mapped Instances
            if (!UpdateCountByBmp.ContainsKey(_writeableBitmap))
            {
                // Set UpdateCount to 1 for this bitmap 
                UpdateCountByBmp.Add(_writeableBitmap, 1);
                length = _writeableBitmap.PixelWidth * _writeableBitmap.PixelHeight;
                pixels = new int[length];
                CopyPixels();
                PixelCacheByBmp.Add(_writeableBitmap, pixels);
            }
            else
            {
                // For previously contextualized bitmaps increment the update count
                IncrementRefCount(_writeableBitmap);
                pixels = PixelCacheByBmp[_writeableBitmap];
                length = pixels.Length;
            }
#endif
        }

#if NETFX_CORE
        private unsafe void CopyPixels()
        {
            var data = _writeableBitmap.PixelBuffer.ToArray();
            fixed (byte* srcPtr = data)
            {
                fixed (int* dstPtr = pixels)
                {
                    for (var i = 0; i < length; i++)
                    {
                        dstPtr[i] = (srcPtr[i * 4 + 3] << 24)
                                  | (srcPtr[i * 4 + 2] << 16)
                                  | (srcPtr[i * 4 + 1] << 8)
                                  | srcPtr[i * 4 + 0];
                    }
                }
            }
        }
#endif

#if NETFX_CORE

        /// <summary>
        /// Gets the Pixels array 
        /// </summary>        
        public int[] Pixels { get { return pixels; } }

        /// <summary>
        /// Gets the length of the Pixels array 
        /// </summary>
        public int Length { get { return length; } }

        /// <summary>
        /// Performs a Copy operation from source BitmapContext to destination BitmapContext
        /// </summary>
        /// <remarks>Equivalent to calling Buffer.BlockCopy in Silverlight, or native memcpy in WPF</remarks>
        public static void BlockCopy(BitmapContext src, int srcOffset, BitmapContext dest, int destOffset, int count)
        {
            Buffer.BlockCopy(src.Pixels, srcOffset, dest.Pixels, destOffset, count);
        }

        /// <summary>
        /// Performs a Copy operation from source Array to destination BitmapContext
        /// </summary>
        /// <remarks>Equivalent to calling Buffer.BlockCopy in Silverlight, or native memcpy in WPF</remarks>
        public static void BlockCopy(Array src, int srcOffset, BitmapContext dest, int destOffset, int count)
        {
            Buffer.BlockCopy(src, srcOffset, dest.Pixels, destOffset, count);
        }

        /// <summary>
        /// Performs a Copy operation from source BitmapContext to destination Array
        /// </summary>
        /// <remarks>Equivalent to calling Buffer.BlockCopy in Silverlight, or native memcpy in WPF</remarks>
        public static void BlockCopy(BitmapContext src, int srcOffset, Array dest, int destOffset, int count)
        {
            Buffer.BlockCopy(src.Pixels, srcOffset, dest, destOffset, count);
        }

        /// <summary>
        /// Clears the BitmapContext, filling the underlying bitmap with zeros
        /// </summary>
        public void Clear()
        {
            var pixels = Pixels;
            Array.Clear(pixels, 0, pixels.Length);
        }

        /// <summary>
        /// Disposes this instance if the underlying platform needs that.
        /// </summary>
        public unsafe void Dispose()
        {
            // Decrement the update count. If it hits zero
            if (DecrementRefCount(_writeableBitmap) == 0)
            {
                // Remove this bitmap from the update map 
                UpdateCountByBmp.Remove(_writeableBitmap);
                PixelCacheByBmp.Remove(_writeableBitmap);

                // Copy data back
                if (_mode == ReadWriteMode.ReadWrite)
                {
                    using (var stream = _writeableBitmap.PixelBuffer.AsStream())
                    {
                        var buffer = new byte[length * 4];
                        fixed (int* srcPtr = pixels)
                        {
                            var b = 0;
                            for (var i = 0; i < length; i++, b += 4)
                            {
                                var p = srcPtr[i];
                                buffer[b + 3] = (byte)((p >> 24) & 0xff);
                                buffer[b + 2] = (byte)((p >> 16) & 0xff);
                                buffer[b + 1] = (byte)((p >> 8) & 0xff);
                                buffer[b + 0] = (byte)(p & 0xff);
                            }
                            stream.Write(buffer, 0, length * 4);
                        }
                    }
                    _writeableBitmap.Invalidate();
                }
            }
        }

#elif WPF
        /// <summary>
        /// The pixels as ARGB integer values, where each channel is 8 bit.
        /// </summary>
        public unsafe int* Pixels
        {
            [System.Runtime.TargetedPatchingOptOut("Candidate for inlining across NGen boundaries for performance reasons")]
            get { return this._backBuffer; }
        }

        /// <summary>
        /// The pixel format
        /// </summary>
        public PixelFormat Format
        {
            [System.Runtime.TargetedPatchingOptOut("Candidate for inlining across NGen boundaries for performance reasons")]
            get { return this.writeableBitmap.Format; }
        }

        /// <summary>
        /// The number of pixels.
        /// </summary>
        public int Length
        {
            [System.Runtime.TargetedPatchingOptOut("Candidate for inlining across NGen boundaries for performance reasons")]
            get
            {
                return this._length;
            }
        }

        /// <summary>
        /// Performs a Copy operation from source Bto destination BitmapContext
        /// </summary>
        /// <remarks>Equivalent to calling Buffer.BlockCopy in Silverlight, or native memcpy in WPF</remarks>
        [System.Runtime.TargetedPatchingOptOut("Candidate for inlining across NGen boundaries for performance reasons")]
        public static unsafe void BlockCopy(BitmapContext src, int srcOffset, BitmapContext dest, int destOffset, int count)
        {
            NativeMethods.CopyUnmanagedMemory((byte*)src.Pixels, srcOffset, (byte*)dest.Pixels, destOffset, count);
        }

        /// <summary>
        /// Performs a Copy operation from source Array to destination BitmapContext
        /// </summary>
        /// <remarks>Equivalent to calling Buffer.BlockCopy in Silverlight, or native memcpy in WPF</remarks>
        [System.Runtime.TargetedPatchingOptOut("Candidate for inlining across NGen boundaries for performance reasons")]
        public static unsafe void BlockCopy(int[] src, int srcOffset, BitmapContext dest, int destOffset, int count)
        {
            fixed (int* srcPtr = src)
            {
                NativeMethods.CopyUnmanagedMemory((byte*)srcPtr, srcOffset, (byte*)dest.Pixels, destOffset, count);
            }
        }

        /// <summary>
        /// Performs a Copy operation from source Array to destination BitmapContext
        /// </summary>
        /// <remarks>Equivalent to calling Buffer.BlockCopy in Silverlight, or native memcpy in WPF</remarks>
        [System.Runtime.TargetedPatchingOptOut("Candidate for inlining across NGen boundaries for performance reasons")]
        public static unsafe void BlockCopy(byte[] src, int srcOffset, BitmapContext dest, int destOffset, int count)
        {
            fixed (byte* srcPtr = src)
            {
                NativeMethods.CopyUnmanagedMemory(srcPtr, srcOffset, (byte*)dest.Pixels, destOffset, count);
            }
        }

        /// <summary>
        /// Performs a Copy operation from source BitmapContext to destination Array
        /// </summary>
        /// <remarks>Equivalent to calling Buffer.BlockCopy in Silverlight, or native memcpy in WPF</remarks>
        [System.Runtime.TargetedPatchingOptOut("Candidate for inlining across NGen boundaries for performance reasons")]
        public static unsafe void BlockCopy(BitmapContext src, int srcOffset, byte[] dest, int destOffset, int count)
        {
            fixed (byte* destPtr = dest)
            {
                NativeMethods.CopyUnmanagedMemory((byte*)src.Pixels, srcOffset, destPtr, destOffset, count);
            }
        }

        /// <summary>
        /// Performs a Copy operation from source BitmapContext to destination Array
        /// </summary>
        /// <remarks>Equivalent to calling Buffer.BlockCopy in Silverlight, or native memcpy in WPF</remarks>
        [System.Runtime.TargetedPatchingOptOut("Candidate for inlining across NGen boundaries for performance reasons")]
        public static unsafe void BlockCopy(BitmapContext src, int srcOffset, int[] dest, int destOffset, int count)
        {
            fixed (int* destPtr = dest)
            {
                NativeMethods.CopyUnmanagedMemory((byte*)src.Pixels, srcOffset, (byte*)destPtr, destOffset, count);
            }
        }

        /// <summary>
        /// Clears the BitmapContext, filling the underlying bitmap with zeros
        /// </summary>
        [System.Runtime.TargetedPatchingOptOut("Candidate for inlining across NGen boundaries for performance reasons")]
        public void Clear()
        {
            NativeMethods.SetUnmanagedMemory(this.writeableBitmap.BackBuffer, 0, this.writeableBitmap.BackBufferStride * this.writeableBitmap.PixelHeight);
        }

        /// <summary>
        /// Disposes the BitmapContext, unlocking it and invalidating if WPF
        /// </summary>
        public void Dispose()
        {
            // Decrement the update count. If it hits zero
            if (DecrementRefCount(this.writeableBitmap) == 0)
            {
                // Remove this bitmap from the update map 
                UpdateCountByBmp.Remove(this.writeableBitmap);

                // Invalidate the bitmap if ReadWrite _mode
                if (this.mode == ReadWriteMode.ReadWrite)
                {
                    this.writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, this.writeableBitmap.PixelWidth, this.writeableBitmap.PixelHeight));
                }

                // Unlock the bitmap
                this.writeableBitmap.Unlock();
            }
        }
#endif

#if WPF || NETFX_CORE
        private static void IncrementRefCount(WriteableBitmap target)
        {
            UpdateCountByBmp[target]++;
        }

        private static int DecrementRefCount(WriteableBitmap target)
        {
            int current = UpdateCountByBmp[target];
            current--;
            UpdateCountByBmp[target] = current;
            return current;
        }
#endif
    }
}
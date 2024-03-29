﻿namespace WPFHexaEditor.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// Static class for valid keyboard key.
    /// </summary>
    public static class KeyValidator
    {
        /// <summary>
        /// Check if is a numeric key as pressed
        /// </summary>
        public static bool IsNumericKey(Key key)
        {
            return key == Key.D0 || key == Key.D1 || key == Key.D2 || key == Key.D3 || key == Key.D4 || key == Key.D5 || key == Key.D6 || key == Key.D7 || key == Key.D8 || key == Key.D9 ||
                key == Key.NumPad0 || key == Key.NumPad1 || key == Key.NumPad2 || key == Key.NumPad3 || key == Key.NumPad4 || key == Key.NumPad5 || key == Key.NumPad6 || key == Key.NumPad7 || key == Key.NumPad8 || key == Key.NumPad9;
        }

        /// <summary>
        /// Get if key is a Hexakey (alpha)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsHexKey(Key key)
        {
            return key == Key.A || key == Key.B || key == Key.C || key == Key.D || key == Key.E || key == Key.F ||
                IsNumericKey(key);
        }

        /// <summary>
        /// Get the digit from key
        /// </summary>
        public static int GetDigitFromKey(Key key)
        {
            switch (key)
            {
                case Key.D0:
                case Key.NumPad0: return 0;
                case Key.D1:
                case Key.NumPad1: return 1;
                case Key.D2:
                case Key.NumPad2: return 2;
                case Key.D3:
                case Key.NumPad3: return 3;
                case Key.D4:
                case Key.NumPad4: return 4;
                case Key.D5:
                case Key.NumPad5: return 5;
                case Key.D6:
                case Key.NumPad6: return 6;
                case Key.D7:
                case Key.NumPad7: return 7;
                case Key.D8:
                case Key.NumPad8: return 8;
                case Key.D9:
                case Key.NumPad9: return 9;
                default: throw new ArgumentOutOfRangeException("Invalid key: " + key.ToString());
            }
        }

        public static bool IsBackspaceKey(Key key)
        {
            return key == Key.Back;
        }

        public static bool IsSubstractKey(Key key)
        {
            return key == Key.Subtract || key == Key.OemMinus;
        }

        public static bool IsDeleteKey(Key key)
        {
            return key == Key.Delete;
        }

        public static bool IsCapsLock(Key key)
        {
            return key == Key.CapsLock;
        }

        public static bool IsIgnoredKey(Key key)
        {
            // ADD SOMES OTHER KEY FOR VALIDATED IN STRINGBYTE CONTROL

            // DELETE KEY FOR ADD OTHER FUNCTIONALITY...
            return key == Key.Tab ||
                   key == Key.Enter ||
                   key == Key.Return ||
                   key == Key.LWin ||
                   key == Key.RWin ||
                   key == Key.CapsLock ||
                   key == Key.LeftAlt ||
                   key == Key.RightAlt ||
                   key == Key.System ||
                   key == Key.LeftCtrl ||
                   key == Key.F1 || key == Key.F2 || key == Key.F3 || key == Key.F4 || key == Key.F5 || key == Key.F6 || key == Key.F7 || key == Key.F8 || key == Key.F9 || key == Key.F10 || key == Key.F11 || key == Key.F12 ||
                   key == Key.Home ||
                   key == Key.Insert ||
                   key == Key.End;
        }

        public static bool IsEscapeKey(Key key)
        {
            return key == Key.Escape;
        }

        public static bool IsUpKey(Key key)
        {
            return key == Key.Up;
        }

        public static bool IsWindowsKey(Key key)
        {
            return key == Key.LWin || key == Key.RWin;
        }

        public static bool IsDownKey(Key key)
        {
            return key == Key.Down;
        }

        public static bool IsRightKey(Key key)
        {
            return key == Key.Right;
        }

        public static bool IsLeftKey(Key key)
        {
            return key == Key.Left;
        }

        public static bool IsPageDownKey(Key key)
        {
            return key == Key.PageDown;
        }

        public static bool IsPageUpKey(Key key)
        {
            return key == Key.PageUp;
        }

        public static bool IsEnterKey(Key key)
        {
            return key == Key.Enter;
        }

        public static bool IsCtrlCKey(Key key)
        {
            return key == Key.C && Keyboard.Modifiers == ModifierKeys.Control;
        }

        public static bool IsCtrlZKey(Key key)
        {
            return key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control;
        }

        public static bool IsCtrlYKey(Key key)
        {
            return key == Key.Y && Keyboard.Modifiers == ModifierKeys.Control;
        }

        public static bool IsCtrlVKey(Key key)
        {
            return key == Key.V && Keyboard.Modifiers == ModifierKeys.Control;
        }
    }
}

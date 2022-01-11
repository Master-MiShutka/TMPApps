namespace DBF
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Acts as a container for concurrent read/write flushing (for example, parsing a
    /// file while concurrently uploading the contents); supports any number of concurrent
    /// writers and readers, but note that each item will only be returned once (and once
    /// fetched, is discarded). It is necessary to Close() the bucket after adding the last
    /// of the data, otherwise any iterators will never finish
    /// </summary>
    internal class ThreadSafeBucket<T> : IEnumerable<T>
    {
        private readonly Queue<T> queue = new Queue<T>();

        public void Add(T value)
        {
            lock (this.queue)
            {
                if (this.closed) // no more data once closed
                {
                    throw new InvalidOperationException("The bucket has been marked as closed");
                }

                this.queue.Enqueue(value);
                if (this.queue.Count == 1)
                { // someone may be waiting for data
                    Monitor.PulseAll(this.queue);
                }
            }
        }

        public void Close()
        {
            lock (this.queue)
            {
                this.closed = true;
                Monitor.PulseAll(this.queue);
            }
        }

        private bool closed;

        public IEnumerator<T> GetEnumerator()
        {
            while (true)
            {
                T value;
                lock (this.queue)
                {
                    if (this.queue.Count == 0)
                    {
                        // no data; should we expect any?
                        if (this.closed)
                        {
                            yield break; // nothing more ever coming
                        }

                        // else wait to be woken, and redo from start
                        Monitor.Wait(this.queue);
                        continue;
                    }

                    value = this.queue.Dequeue();
                }

                // yield it **outside** of the lock
                yield return value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}

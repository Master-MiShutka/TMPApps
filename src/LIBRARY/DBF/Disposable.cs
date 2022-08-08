namespace DBF
{
    using System;

    public abstract class Disposable : IDisposable
    {
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Disposable()
        {
            this.Dispose(false);
        }

        protected abstract void Dispose(bool disposing);
    }
}

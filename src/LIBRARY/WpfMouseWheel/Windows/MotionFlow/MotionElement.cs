namespace WpfMouseWheel.Windows.MotionFlow
{
    using System;
    using System.Windows;

    public interface IMotionElementInput : IInputElement
    {
    }

    public interface IMotionElement : IMotionElementInput
    {
        int Id
        {
            get;
        }

        string Name
        {
            get; set;
        }
    }

    public class MotionElement : ContentElement, IMotionElement
    {
        public static string TransmitMethodSuffix(IMotionInfo info, int nativeDelta)
        {
            return nativeDelta == 0 || Math.Sign(nativeDelta) == info.NativeDirection ? "++" : "--";
        }

        public MotionElement()
        {
            this.Id = ++_refCount;
            this.Name = this.Id.ToString("'M'00");
        }

        public int Id
        {
            get; protected set;
        }

        public string Name
        {
            get; set;
        }

        public override string ToString()
        {
            return this.Name;
        }

        private static int _refCount;
    }

    public class MotionElementLink : MotionElement
    {
        protected IMotionElementInput GetNext(bool setParent = true)
        {
            if (setParent)
                this._next.SetParent(this);
            return this._next;
        }

        protected void SetNext(IMotionElementInput value, bool setParent = true)
        {
            this._next = value;
            if (setParent)
                value.SetParent(this);
        }

        private IMotionElementInput _next;
    }

    public static class MotionElementExtensions
    {
        public static void SetParent(this IMotionElementInput self, DependencyObject parent)
        {
            if (self is ContentElement)
            {
                ContentOperations.SetParent(self as ContentElement, parent);
            }
            else
            {
                throw new ArgumentException("Given object is not a ContentElement");
            }
        }
    }
}

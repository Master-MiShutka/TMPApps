using System;

namespace TMPApplication
{
    /// <summary>
    /// Exception thrown by the BindingExceptionThrower each time a WPF binding error occurs
    /// </summary>
    [Serializable]
    public class BindingException : Exception
    {
        public BindingException(string message)
            : base(message)
        {
            ;            
        }
    }
}
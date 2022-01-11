namespace TemplateEngine.Docx.Errors
{
    using System;

    internal interface IError : IEquatable<IError>
    {
        string Message { get; }
    }
}

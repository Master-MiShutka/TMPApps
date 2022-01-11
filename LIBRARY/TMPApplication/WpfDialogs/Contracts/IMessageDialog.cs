namespace TMPApplication.WpfDialogs.Contracts
{
    public interface IMessageDialog : IDialog
    {
        string Message { get; set; }
    }
}
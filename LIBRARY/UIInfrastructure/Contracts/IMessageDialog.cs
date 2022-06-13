namespace UIInfrastructure.WindowWithDialogs.Contracts
{
    public interface IMessageDialog : IDialog
    {
        string Message { get; set; }
    }
}
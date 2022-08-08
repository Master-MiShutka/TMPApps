namespace UIInfrastructure.Contracts
{
    public interface IMessageDialog : IDialog
    {
        string Message { get; set; }
    }
}
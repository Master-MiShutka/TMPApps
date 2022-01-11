namespace WpfMouseWheel.Maths
{
    public interface IFunction<T>
    {
        T F(T x);
    }

    public interface IDifferentialFunction<T>
    {
        T DF(T dx);
    }
}

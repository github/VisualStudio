public interface ITestOutputHelper
{
    void WriteLine(string message);
    void WriteLine(string format, params object[] args);
}

public class Delegates
{
    public delegate void ServiceCallback(bool success = true, string errorMessage = null);
    public delegate void ServiceCallback<in Result>(bool success = true, string errorMessage = null, Result result = default(Result));
}
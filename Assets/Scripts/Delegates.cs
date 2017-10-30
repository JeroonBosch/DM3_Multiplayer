
public class Delegates
{
    public delegate void ServiceCallback(bool success = false, string errorMessage = null);
    public delegate void ServiceCallback<in Result>(bool success = false, string errorMessage = null, Result result = default(Result));
}
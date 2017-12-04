using System.Collections.Generic;

public class Delegates
{
    public delegate void ServiceCallback(bool success = true, string errorMessage = null);
    public delegate void ServiceCallback<in Result>(bool success = true, string errorMessage = null, Result result = default(Result));
    public delegate void ServiceCallbackCustomInfo<in Result>(bool success = true, string errorMessage = null, Result result = default(Result), Dictionary<string, object> customInfo = null);
}
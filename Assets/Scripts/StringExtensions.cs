using System.Security.Cryptography;
using System.Text;


public static class StringExtensions
{
    /// <summary>
    /// Gets the boolean from string.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static bool ToBoolean(this string value)
    {
        return !string.IsNullOrEmpty(value) && !value.Equals("0");
    }

    /// <summary>
    /// Gets the MD5.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static string GetMd5(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        var md5 = MD5.Create();
        var data = md5.ComputeHash(Encoding.UTF8.GetBytes(value));
        var stringBuilder = new StringBuilder();
        foreach (var d in data)
        {
            stringBuilder.Append(d.ToString("x2"));
        }

        return stringBuilder.ToString();
    }
}


using UnityEngine;

public class SettingsService
{
    private LoginType _lastLoginType = LoginType.NONE;
    private string _mobileId = "";
    private string _hexaClash = "";

    public SettingsService()
    {
        if (PlayerPrefs.HasKey("LastLoginType"))
        {
            _lastLoginType = (LoginType)PlayerPrefs.GetInt("LastLoginType", 0);
        }
        if (PlayerPrefs.HasKey("MobileId"))
        {
            _mobileId = PlayerPrefs.GetString("MobileId", "");
        }
        if (PlayerPrefs.HasKey("HexaClash"))
        {
            _hexaClash = PlayerPrefs.GetString("HexaClash", "");
        }
    }

    public void SaveLocalSettings()
    {
        PlayerPrefs.Save();
    }

    public LoginType lastLoginType
    {
        get
        {
            return _lastLoginType;
        }
        set
        {
            _lastLoginType = value;
            PlayerPrefs.SetInt("LastLoginType", (int)value);
            PlayerPrefs.Save();
        }
    }

    public string mobileId
    {
        get
        {
            return _mobileId;
        }
        set
        {
            _mobileId = value;
            PlayerPrefs.SetString("MobileId", value);
            PlayerPrefs.Save();
        }
    }

    public string hexaClash
    {
        get
        {
            return _hexaClash;
        }
        set
        {
            _hexaClash = value;
            PlayerPrefs.SetString("HexaClash", value);
            PlayerPrefs.Save();
        }
    }
}


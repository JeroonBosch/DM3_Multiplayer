using UnityEngine;

namespace Com.Hypester.DM3
{
    public class PlayerEvent : MonoBehaviour
    {
        public delegate void LoginAction(LoginCanvas.LoginType loginType);

        public static event LoginAction OnPlayerLogin;

        public static void PlayerLogin(LoginCanvas.LoginType loginType)
        {
            if (OnPlayerLogin != null) OnPlayerLogin(loginType);
        }

        public delegate void ProfileImageAction(Texture2D newImage);
        public delegate void ProfileNameAction(string value);
        public delegate void StatAction(int amount);
        public delegate void SkillStatAction(string skillColor, int amount);

        public static event ProfileImageAction OnProfileImageChange;
        public static event ProfileNameAction OnProfileNameChange;

        public static event StatAction OnCoinAmountChange;
        public static event StatAction OnTrophyAmountChange;
        public static event StatAction OnXpAmountChange;
        public static event StatAction OnUnspentSkillPointAmountChange;

        public static event SkillStatAction OnSkillLevelChange;

        public static void ProfileImageChange(Texture2D newImage)
        {
            if (OnProfileImageChange != null) OnProfileImageChange(newImage);
        }
        public static void ProfileNameChange(string value)
        {
            if (OnProfileNameChange != null) OnProfileNameChange(value);
        }
        public static void CoinAmountChange(int amount)
        {
            if (OnCoinAmountChange != null) OnCoinAmountChange(amount);
        }
        public static void TrophyAmountChange(int amount)
        {
            if (OnTrophyAmountChange != null) OnTrophyAmountChange(amount);
        }
        public static void XpAmountChange(int amount)
        {
            if (OnXpAmountChange != null) OnXpAmountChange(amount);
        }
        public static void UnspentSkillPointAmountChange(int amount)
        {
            if (OnUnspentSkillPointAmountChange != null) OnUnspentSkillPointAmountChange(amount);
        }
        public static void SkillLevelChange(string syscode, int amount)
        {
            if (OnSkillLevelChange != null) OnSkillLevelChange(syscode, amount);
        }
    }
}
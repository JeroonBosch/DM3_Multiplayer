using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;

namespace Com.Hypester.DM3
{
    public class PlayerEvent
    {
        public delegate void LoginAction(LoginType loginType, PlayerService.LoginRequestObject loginObject);

        public static event LoginAction OnPlayerLogin;

        public static void PlayerLogin(LoginType loginType, PlayerService.LoginRequestObject loginObject)
        {
            if (OnPlayerLogin != null) OnPlayerLogin(loginType, loginObject);
        }

        public delegate void ProfileSpriteAction(Sprite newSprite);
        public delegate void ProfileTexture2DAction(Texture2D newTexture);
        public delegate void ProfileNameAction(string value);
        public delegate void StatAction(int amount);
        public delegate void SkillStatAction(string skillColor, int amount);

        public static event ProfileSpriteAction OnAvatarBorderChange;
        public static event ProfileTexture2DAction OnProfileImageChange;
        public static event ProfileNameAction OnProfileNameChange;

        public static event StatAction OnCoinAmountChange;
        public static event StatAction OnTrophyAmountChange;
        public static event StatAction OnXpAmountChange;
        public static event StatAction OnUnspentSkillPointAmountChange;

        public static event SkillStatAction OnSkillLevelChange;

        public static void AvatarBorderChange(Sprite newBorder)
        {
            if (OnAvatarBorderChange != null) OnAvatarBorderChange(newBorder);
        }
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

        // NETWORKED PLAYER (PHOTON)
        public delegate void PlayerIdAction(int playerId);
        public delegate void PlayerStatsAction(int playerId, Hashtable stats);

        public static event PlayerIdAction OnPlayerWantsRematch;
        public static event PlayerStatsAction OnPlayerStatsUpdate;

        public static void PlayerWantsRematch(int playerId)
        {
            if (null != OnPlayerWantsRematch) OnPlayerWantsRematch(playerId);
        }
        public static void PlayerStatsUpdate(int playerId, Hashtable stats)
        {
            if (null != OnPlayerStatsUpdate) OnPlayerStatsUpdate(playerId, stats);
        }
    }
}
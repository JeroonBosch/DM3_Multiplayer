using UnityEngine;

namespace Com.Hypester.DM3
{
    public class UIEvent : MonoBehaviour
    {
        // Delegates for the events
        public delegate void PopupSkillUpgradeConfirmationAction(string syscode, int currentLevel, string description, int coinCost, int skillCost);
        public delegate void PopupSkillUpgradeLockedAction(string description);
        public delegate void PopupSkillUpgradeSuccessAction(bool success, string syscode, int newLevel);

        public delegate void PopupCoinPurchaseSuccessAction(bool success, int amount);
        public delegate void PopupInsufficientCurrency(SkillLevelEntry.Currency currency);

        public delegate void PopupAction(IPopup popup);
        public delegate void PopupInfoAction(string msg, PopupType type);

        public delegate void PopupSurrenderAction();

        // CURRENCY
        public static event PopupCoinPurchaseSuccessAction OnCoinPurchaseSuccess;
        public static event PopupInsufficientCurrency OnInsufficientCurrency;

        public static void CoinPurchase(bool success, int amount)
        {
            if (OnCoinPurchaseSuccess != null) OnCoinPurchaseSuccess(success, amount);
        }
        public static void InsufficientCurrency(SkillLevelEntry.Currency currency)
        {
            if (OnInsufficientCurrency != null) OnInsufficientCurrency(currency);
        }

        // SKILL UPGRADES
        public static event PopupSkillUpgradeConfirmationAction OnSkillUpgradeConfirmation;
        public static event PopupSkillUpgradeLockedAction OnSkillLocked;
        public static event PopupSkillUpgradeSuccessAction OnSkillUpgradeSuccess;

        public static void SkillUpgrade(string syscode, int currentLevel, string description, int coinCost, int skillCost)
        {
            if (OnSkillUpgradeConfirmation != null) OnSkillUpgradeConfirmation(syscode, currentLevel, description, coinCost, skillCost);
        }
        public static void SkillLocked(string description)
        {
            if (OnSkillLocked != null) OnSkillLocked(description);
        }
        public static void SkillUpgradeSuccess(bool success, string syscode, int newLevel)
        {
            if (OnSkillUpgradeSuccess != null) OnSkillUpgradeSuccess(success, syscode, newLevel);
        }

        // GENERAL
        public static event PopupAction OnPopupClose;
        public static event PopupInfoAction OnInfo;

        public static void PopupClose(IPopup popup)
        {
            if (OnPopupClose != null) OnPopupClose(popup);
        }
        public static void Info(string msg, PopupType type)
        {
            if (OnInfo != null) { OnInfo(msg, type); }
        }

        // GAME
        public static event PopupSurrenderAction OnSurrender;

        public static void Surrender()
        {
            if (OnSurrender != null) { OnSurrender(); }
        }
    }
}
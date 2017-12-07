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

        public delegate void PopupTextAction();
        public delegate void PopupTextPlayerAction(bool localPlayer);
        public delegate void PopupTextTileAction(Vector2 tilePos);
        public delegate void PopupTextBoosterAction(Vector2 tilePos, int boosterLevel);
        public delegate void PopupTextSkillAction(SkillColor color);

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

        public static event PopupTextPlayerAction OnTurnChange;
        public static event PopupTextBoosterAction OnBoosterTrigger;
        public static event PopupTextAction OnBoosterTriggerDouble;
        public static event PopupTextAction OnBoosterTriggerTriple;
        public static event PopupTextAction OnBoosterTriggerMulti;
        public static event PopupTextAction OnOpponentTrapPlaced;
        public static event PopupTextTileAction OnOpponentTrapTrigger;
        public static event PopupTextTileAction OnLocalTrapTrigger;
        public static event PopupTextPlayerAction OnShieldActivate;
        public static event PopupTextPlayerAction OnShieldHit;
        public static event PopupTextPlayerAction OnHeal;
        public static event PopupTextSkillAction OnSkillNotFull;


        public static void Surrender()
        {
            if (OnSurrender != null) { OnSurrender(); }
        }

        public static void TurnChange(bool localPlayer)
        {
            if (OnTurnChange != null) { OnTurnChange(localPlayer); }
        }
        public static void BoosterTrigger(Vector2 tilePos, int boosterLevel)
        {
            if (OnBoosterTrigger != null) { OnBoosterTrigger(tilePos, boosterLevel); }
        }
        public static void BoosterTriggerDouble()
        {
            if (OnBoosterTriggerDouble != null) { OnBoosterTriggerDouble(); }
        }
        public static void BoosterTriggerTriple()
        {
            if (OnBoosterTriggerTriple != null) { OnBoosterTriggerTriple(); }
        }
        public static void BoosterTriggerMulti()
        {
            if (OnBoosterTriggerMulti != null) { OnBoosterTriggerMulti(); }
        }
        public static void OpponentTrapPlaced()
        {
            if (OnOpponentTrapPlaced != null) { OnOpponentTrapPlaced(); }
        }
        public static void OpponentTrapTrigger(Vector2 tilePos)
        {
            if (OnOpponentTrapTrigger != null) { OnOpponentTrapTrigger(tilePos); }
        }
        public static void LocalTrapTrigger(Vector2 tilePos)
        {
            if (OnLocalTrapTrigger != null) { OnLocalTrapTrigger(tilePos); }
        }
        public static void ShieldActivate(bool localPlayer)
        {
            if (OnShieldActivate != null) { OnShieldActivate(localPlayer); }
        }
        public static void ShieldHit(bool localPlayer)
        {
            if (OnShieldHit != null) { OnShieldHit(localPlayer); }
        }
        public static void Heal(bool localPlayer)
        {
            if (OnHeal != null) { OnHeal(localPlayer); }
        }
        public static void SkillNotFull(SkillColor color)
        {
            if (OnSkillNotFull != null) { OnSkillNotFull(color); }
        }
    }
}
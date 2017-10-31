using UnityEngine;
using System.Collections.Generic;

namespace Com.Hypester.DM3
{
    public class PopupManager : MonoBehaviour
    {
        // TODO: use object pool for popups.

        [Header("General")]
        [SerializeField] GameObject infoPrefab;

        [Header("Currency")]
        [SerializeField] GameObject coinPurchasePrefab;
        [SerializeField] GameObject insufficientCurrencyPrefab;

        [Header("SkillUpgrades")]
        [SerializeField] GameObject upgradeSkillNextLevelPrefab;
        [SerializeField] GameObject upgradeSkillLockedPrefab;
        [SerializeField] GameObject upgradeSkillSuccessPrefab;

        Queue<GameObject> popupQueue = new Queue<GameObject>();
        GameObject activePopup;

        private void OnEnable()
        {
            UIEvent.OnCoinPurchaseSuccess += CoinPurchaseSuccess;
            UIEvent.OnInsufficientCurrency += InsufficientCurrency;

            UIEvent.OnSkillUpgradeConfirmation += SkillUpgradeConfirmation;
            UIEvent.OnSkillLocked += SkillLocked;
            UIEvent.OnSkillUpgradeSuccess += SkillUpgradeSuccess;

            UIEvent.OnPopupClose += PopupClose;
            UIEvent.OnInfo += Info;
        }

        private void OnDisable()
        {
            UIEvent.OnCoinPurchaseSuccess -= CoinPurchaseSuccess;
            UIEvent.OnInsufficientCurrency -= InsufficientCurrency;

            UIEvent.OnSkillUpgradeConfirmation -= SkillUpgradeConfirmation;
            UIEvent.OnSkillLocked -= SkillLocked;
            UIEvent.OnSkillUpgradeSuccess -= SkillUpgradeSuccess;

            UIEvent.OnPopupClose -= PopupClose;
            UIEvent.OnInfo -= Info;
        }

        private void Update()
        {
            if (popupQueue.Count > 0)
            {
                if (activePopup == null)
                {
                    ActivatePopup(popupQueue.Dequeue());
                }
            }
        }

        private void Info(string msg, PopupType type)
        {
            InfoPopup ip = Instantiate(infoPrefab).GetComponent<InfoPopup>();

            ip.msgIcon.sprite = MainController.Data.sprites.thumbsDown;
            ip.msgTitle.text = "Oops...";
            ip.msgBody.text = msg;
            ip.background.sprite = MainController.Data.sprites.GetPopupBackground(type);

            AddPopupToQueue(ip.gameObject);
        }

        void InsufficientCurrency(SkillLevelEntry.Currency currency)
        {
            InsufficientCurrencyPopup icp = Instantiate(insufficientCurrencyPrefab).GetComponent<InsufficientCurrencyPopup>();
            icp.acceptButton.onClick.RemoveAllListeners();

            switch (currency)
            {
                case SkillLevelEntry.Currency.Coins:
                    icp.msgIcon.sprite = MainController.Data.sprites.coinIcon;
                    icp.msgTitle.text = "Not enough coins";
                    icp.msgBody.text = "Win more coins by playing against opponents or visit the coin store";
                    icp.refuseButton.SetActive(true);
                    icp.acceptButtonText.text = "Coin Store";
                    icp.acceptButton.onClick.AddListener(() => icp.CoinStore());
                    break;
                case SkillLevelEntry.Currency.SkillPoints:
                    icp.msgIcon.sprite = MainController.Data.sprites.skillIcon;
                    icp.msgTitle.text = "Not enough skill points";
                    icp.msgBody.text = "Win more skill points by playing against opponents";
                    icp.acceptButtonText.text = "Okay";
                    icp.acceptButton.onClick.AddListener(() => icp.Accept());
                    break;
            }

            AddPopupToQueue(icp.gameObject);
        }

        void CoinPurchaseSuccess(bool success, int amount)
        {
            CoinShopPopupSuccess csps = Instantiate(coinPurchasePrefab).GetComponent<CoinShopPopupSuccess>();

            if (success)
            {
                csps.msgIcon.sprite = MainController.Data.sprites.thumbsUp;
                csps.msgTitle.text = "Purchase successful!";
                csps.msgBody.text = amount.ToString() + " Coins have been deposited to your account";
                csps.buttonText.text = "Nice!";
            } else {
                csps.msgIcon.sprite = MainController.Data.sprites.thumbsDown;
                csps.msgTitle.text = "Purchase failed";
                csps.msgBody.text = "Please try again later"; // TODO: add error message here
                csps.buttonText.text = "Okay";
            }

            AddPopupToQueue(csps.gameObject);
        }

        void SkillUpgradeConfirmation(string syscode, int currentLevel, string description, int coinCost, int skillCost)
        {
            UpgradeSkillPopupNextLevel uspnl = Instantiate(upgradeSkillNextLevelPrefab).GetComponent<UpgradeSkillPopupNextLevel>();

            uspnl.syscode = syscode;
            uspnl.nextLevel = currentLevel + 1;
            uspnl.coinCost = coinCost;
            uspnl.skillCost = skillCost;

            uspnl.currentLevelIcon.sprite = MainController.Data.sprites.GetSkillSprite(syscode, currentLevel);
            uspnl.nextLevelIcon.sprite = MainController.Data.sprites.GetSkillSprite(syscode, currentLevel+1);
            uspnl.msgTitle.text = "Upgrade to Level " + (currentLevel + 1).ToString();
            uspnl.msgBody.text = description;
            uspnl.coinCostText.text = coinCost.ToString();
            uspnl.skillCostText.text = skillCost.ToString();

            AddPopupToQueue(uspnl.gameObject);
        }
        void SkillLocked(string description)
        {
            UpgradeSkillPopupLocked uspl = Instantiate(upgradeSkillLockedPrefab).GetComponent<UpgradeSkillPopupLocked>();

            uspl.msgBody.text = description;

            AddPopupToQueue(uspl.gameObject);
        }
        void SkillUpgradeSuccess(bool success, string syscode, int newLevel)
        {
            UpgradeSkillPopupSuccess csps = Instantiate(upgradeSkillSuccessPrefab).GetComponent<UpgradeSkillPopupSuccess>();

            if (success)
            {
                csps.msgIcon.sprite = MainController.Data.sprites.GetSkillSprite(syscode, newLevel);
                csps.msgTitle.text = "Upgrade successful!";
                csps.msgBody.text = "You have reached " + syscode + " skill level " + newLevel.ToString();
                csps.buttonText.text = "Nice!";
            }
            else
            {
                csps.msgIcon.sprite = MainController.Data.sprites.thumbsDown;
                csps.msgTitle.text = "Upgrade failed";
                csps.msgBody.text = "Please try again later"; // TODO: add error message here
                csps.buttonText.text = "Okay";
            }

            AddPopupToQueue(csps.gameObject);
        }

        void PopupClose(IPopup popup)
        {
            if (popup.GetGameObject() == activePopup)
            {
                Destroy(popup.GetGameObject());
                activePopup = null;
            }
        }
        void AddPopupToQueue(GameObject popup)
        {
            popupQueue.Enqueue(popup);
        }
        void ActivatePopup(GameObject popup)
        {
            activePopup = popup;
            popup.SetActive(true);
        }
    }
}
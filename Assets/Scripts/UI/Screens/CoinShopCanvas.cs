using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Com.Hypester.DM3
{
    public class CoinShopCanvas : BaseMenuCanvas
    {
        public void BuyCoins(Text coinAmountText)
        {
            int amount = -1;
            int.TryParse(coinAmountText.text, out amount);
            if (amount < 0)
            {
                Debug.LogError("Failed to parse Amount Text.text to integer.");
            }
            else {
                Debug.Log("Purchased " + amount.ToString() + " coins.");
                MainController.ServiceEconomy.BuyCoinsTemp(amount, amount, OnBuyCoins);
            }
        }

        void OnBuyCoins(bool isSuccess, string errorMessage, EconomyService.BuyCoinsTempResponseObject responseObj, Dictionary<string, object> customInfo)
        {
            bool hasError = !string.IsNullOrEmpty(errorMessage);
            if (!isSuccess || responseObj == null || hasError)
            {
                Debug.LogError(string.Format("!isSuccess({0}), responseObj == null ({1}), hasError({2})", !isSuccess, responseObj == null, hasError));
                if (hasError)
                {
                    UIEvent.Info(errorMessage, PopupType.Error);
                    Debug.Log(errorMessage);
                    return;
                }
            }
            if (responseObj == null)
            {
                UIEvent.Info("Response serialization failed", PopupType.Error);
                Debug.LogError("Could not create responseObj.");
                return;
            }

            if (customInfo != null && customInfo.ContainsKey("purchase_amount"))
            {
                object purchaseAmount = 0;
                if (customInfo.TryGetValue("purchase_amount", out purchaseAmount))
                {
                    MainController.Instance.playerData.AddCoins((int)purchaseAmount);
                    MainController.Instance.playerData.AddUnspentSkillPoints((int)purchaseAmount);
                    UIEvent.CoinPurchase(true, (int)purchaseAmount);
                }
            }
        }

        public void Redeem()
        {

        }

        public void PreviousScreen()
        {
            GoToScreen(PrevScreen());
        }
    }
}
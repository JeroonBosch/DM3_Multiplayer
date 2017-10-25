using UnityEngine;
using UnityEngine.UI;

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
                MainController.Instance.playerData.AddCoins(amount);
                UIEvent.CoinPurchase(true, amount);
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
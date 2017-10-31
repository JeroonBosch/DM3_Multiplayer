using UnityEngine;

namespace Com.Hypester.DM3
{
    public class ShopCanvas : BaseMenuCanvas
    {
        public void CoinShop()
        {
            GoToScreen(GameObject.Find("CoinShopScreen").GetComponent<CoinShopCanvas>());
        }

        public void PreviousScreen()
        {
            GoToScreen(PrevScreen());
        }
    }
}
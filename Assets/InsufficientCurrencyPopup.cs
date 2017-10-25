using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Com.Hypester.DM3
{
    public class InsufficientCurrencyPopup : MonoBehaviour, IPopup
    {
        public delegate void CurrencyFunction();

        public Image msgIcon;
        public Text msgTitle;
        public Text msgBody;

        public Button acceptButton;
        public Text acceptButtonText;
        public GameObject refuseButton;

        public void Accept()
        {
            UIEvent.PopupClose(this);
        }

        public void CoinStore()
        {
            CoinShopCanvas csc = GameObject.Find("CoinShopScreen").GetComponent<CoinShopCanvas>();
            csc.GoToScreen(csc);
            UIEvent.PopupClose(this);
        }

        public void Refuse()
        {
            UIEvent.PopupClose(this);
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }
    }
}
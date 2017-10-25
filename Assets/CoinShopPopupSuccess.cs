using System;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class CoinShopPopupSuccess : MonoBehaviour, IPopup
    {
        public Image msgIcon;
        public Text msgTitle;
        public Text msgBody;
        public Text buttonText;

        public void Accept()
        {
            UIEvent.PopupClose(this);
        }

        public void Refuse()
        {
            throw new NotImplementedException();
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }
    }
}
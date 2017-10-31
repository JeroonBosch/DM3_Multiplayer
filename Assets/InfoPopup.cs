using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class InfoPopup : MonoBehaviour, IPopup
    {
        public delegate void CurrencyFunction();

        public Image msgIcon;
        public Text msgTitle;
        public Text msgBody;

        public Image background;

        public void Accept()
        {
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
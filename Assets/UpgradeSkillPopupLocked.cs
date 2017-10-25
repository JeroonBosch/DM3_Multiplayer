using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class UpgradeSkillPopupLocked : MonoBehaviour, IPopup
    {
        public Text msgBody;

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
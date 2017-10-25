using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class UpgradeSkillPopupNextLevel : MonoBehaviour, IPopup
    {
        public string syscode;
        public int nextLevel;
        public int coinCost;
        public int skillCost;

        public Image currentLevelIcon;
        public Image nextLevelIcon;
        public Text msgTitle;
        public Text msgBody;
        public Text coinCostText;
        public Text skillCostText;

        public void Accept()
        {
            bool upgradeSuccess = true; // TODO: The purchase must be done through the server.
            if (upgradeSuccess) {
                MainController.Instance.playerData.AddCoins(-coinCost);
                MainController.Instance.playerData.AddUnspentSkillPoints(-skillCost);
                MainController.Instance.playerData.SetSkillLevel(syscode, nextLevel);
            }
            UIEvent.PopupClose(this);
            UIEvent.SkillUpgradeSuccess(upgradeSuccess, syscode, nextLevel);
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
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

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
            bool gotData = true;

            if (MainController.Data.temporary.skills.Count <= 0) { gotData = false; }

            EconomyService.Skill currentSkill = MainController.Data.temporary.skills.Where(s => s.syscode == syscode).FirstOrDefault();
            if (currentSkill == null) { gotData = false; }
            else
            {
                EconomyService.Level level = currentSkill.levels.Where(l => l.level == nextLevel).FirstOrDefault();
                if (level == null) { gotData = false; }
                else
                {
                    string newLevelId = level.id;
                    MainController.ServiceEconomy.LoadShop(OnUpgradeCallback, newLevelId);
                    // TODO: popup a loading panel
                }
            }
        }

        private void OnUpgradeCallback(bool success, string errorMessage, EconomyService.ShopRequestObject result)
        {
            bool unlockSuccessful = success && string.IsNullOrEmpty(errorMessage) && result != null;

            if (result != null && result.skills != null && result.skills.Count > 0)
            {
                MainController.Data.temporary.skills = result.skills;
                MainController.Instance.playerData.SetSkillLevel(syscode, nextLevel);
            }

            UIEvent.PopupClose(this);
            UIEvent.SkillUpgradeSuccess(unlockSuccessful, syscode, nextLevel);
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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class UpgradeSkillCanvas : BaseMenuCanvas, IEntryList
    {
        [SerializeField] GameObject skillLevelEntryPrefab;
        [SerializeField] GameObject skillLevelListPrefab;

        [SerializeField] Text blueText;
        [SerializeField] Text greenText;
        [SerializeField] Text redText;
        [SerializeField] Text yellowText;
        [SerializeField] Image blueIcon;
        [SerializeField] Image greenIcon;
        [SerializeField] Image redIcon;
        [SerializeField] Image yellowIcon;

        List<SkillLevelInfo> blueSkills = new List<SkillLevelInfo>();
        List<SkillLevelInfo> greenSkills = new List<SkillLevelInfo>();
        List<SkillLevelInfo> redSkills = new List<SkillLevelInfo>();
        List<SkillLevelInfo> yellowSkills = new List<SkillLevelInfo>();
        
        Dictionary<string, GameObject> skillLevelLists = new Dictionary<string, GameObject>();

        protected override void OnEnable()
        {
            PlayerEvent.OnXpAmountChange += XpAmountChange;
            PlayerEvent.OnUnspentSkillPointAmountChange += UnspentSkillPointAmountChange;
            PlayerEvent.OnSkillLevelChange += SkillLevelChange;

            base.OnEnable();
        }

        protected override void OnDisable()
        {
            PlayerEvent.OnCoinAmountChange += CoinAmountChange;
            PlayerEvent.OnXpAmountChange -= XpAmountChange;
            PlayerEvent.OnUnspentSkillPointAmountChange -= UnspentSkillPointAmountChange;
            PlayerEvent.OnSkillLevelChange -= SkillLevelChange;

            base.OnDisable();
        }

        void CoinAmountChange(int amount)
        {
            // TODO: Unrestrict skills accordingly
        }
        void XpAmountChange(int amount)
        {
            // TODO: Unrestrict skills accordingly
        }
        void UnspentSkillPointAmountChange(int amount)
        {
            // TODO: Unrestrict skills accordingly
        }
        public void SkillLevelChange(string skillColor, int amount)
        {
            Text chosenText = null;
            Image chosenIcon = null;
            switch (skillColor)
            {
                case "blue":
                    chosenText = blueText;
                    chosenIcon = blueIcon;
                    break;
                case "green":
                    chosenText = greenText;
                    chosenIcon = greenIcon;
                    break;
                case "red":
                    chosenText = redText;
                    chosenIcon = redIcon;
                    break;
                case "yellow":
                    chosenText = yellowText;
                    chosenIcon = yellowIcon;
                    break;
            }
            if (chosenText != null && chosenIcon != null)
            {
                int maxLevel = Constants.MaxSkillLevel;
                chosenText.text = amount.ToString() + "/" + maxLevel.ToString();
                chosenIcon.sprite = MainController.Data.sprites.GetSkillSprite(skillColor, amount);
            }
        }

        public override void Hide()
        {
            base.Hide();

            ClearSkillLevelLists();
        }

        public override void Show()
        {
            base.Show();

            MainController.ServiceEconomy.LoadShop(OnShopLoaded);

            /*
            blueSkills.Add(new SkillLevelInfo("Blue (shield)", "blue", "10% Less blue required to activate", 1, 500, 10, 2, new List<float>() { 1, 1 }));
            blueSkills.Add(new SkillLevelInfo("Blue (shield)", "blue", "Reflects 10% damage", 2, 2500, 25, 5, new List<float>() { 1, 1 }));
            blueSkills.Add(new SkillLevelInfo("Blue (shield)", "blue", "20% Less blue req. + reflects 20% damage", 3, 10000, 75, 8, new List<float>() { 1, 1 }));
            blueSkills.Add(new SkillLevelInfo("Blue (shield)", "blue", "35% Less blue req. + reflects 35% damage", 4, 50000, 150, 12, new List<float>() { 1, 1 }));
            blueSkills.Add(new SkillLevelInfo("Blue (shield)", "blue", "50% Less blue req. + reflects 50% damage", 5, 200000, 250, 15, new List<float>() { 1, 1 }));
            blueSkills.Add(new SkillLevelInfo("Blue (shield)", "blue", "Reflects 100% damage", 6, 500000, 500, 18, new List<float>() { 1, 1 }));

            greenSkills.Add(new SkillLevelInfo("Green (heal)", "green", "10% Less green required to activate", 1, 500, 10, 2, new List<float>() { 1, 1 }));
            greenSkills.Add(new SkillLevelInfo("Green (heal)", "green", "Heal amount +10%", 2, 2500, 25, 5, new List<float>() { 1, 1 }));
            greenSkills.Add(new SkillLevelInfo("Green (heal)", "green", "20% Less green req. heal +20%", 3, 10000, 75, 8, new List<float>() { 1, 1 }));
            greenSkills.Add(new SkillLevelInfo("Green (heal)", "green", "35% Less green req. heal +35%", 4, 50000, 150, 12, new List<float>() { 1, 1 }));
            greenSkills.Add(new SkillLevelInfo("Green (heal)", "green", "50% Less green req. heal +50%", 5, 200000, 250, 15, new List<float>() { 1, 1 }));
            greenSkills.Add(new SkillLevelInfo("Green (heal)", "green", "Heal every time you trigger green", 6, 500000, 500, 18, new List<float>() { 1, 1 }));

            redSkills.Add(new SkillLevelInfo("Red (trap)", "red", "10% Less red required to activate", 1, 500, 10, 2, new List<float>() { 1, 1 }));
            redSkills.Add(new SkillLevelInfo("Red (trap)", "red", "Damage amount +10%", 2, 2500, 25, 5, new List<float>() { 1, 1 }));
            redSkills.Add(new SkillLevelInfo("Red (trap)", "red", "20% Less red req. Damage +20%", 3, 10000, 75, 8, new List<float>() { 1, 1 }));
            redSkills.Add(new SkillLevelInfo("Red (trap)", "red", "35% Less red req. Damage +35%", 4, 50000, 150, 12, new List<float>() { 1, 1 }));
            redSkills.Add(new SkillLevelInfo("Red (trap)", "red", "50% Less red req. Damage +50%", 5, 200000, 250, 15, new List<float>() { 1, 1 }));
            redSkills.Add(new SkillLevelInfo("Red (trap)", "red", "Store multiple mines", 6, 500000, 500, 18, new List<float>() { 1, 1 }));

            yellowSkills.Add(new SkillLevelInfo("Yellow (fireball)", "yellow", "10% Less yellow required to activate", 1, 500, 10, 2, new List<float>() { 1, 1 }));
            yellowSkills.Add(new SkillLevelInfo("Yellow (fireball)", "yellow", "Damage amount +10%", 2, 2500, 25, 5, new List<float>() { 1, 1 }));
            yellowSkills.Add(new SkillLevelInfo("Yellow (fireball)", "yellow", "20% Less yellow req. Damage +20%", 3, 10000, 75, 8, new List<float>() { 1, 1 }));
            yellowSkills.Add(new SkillLevelInfo("Yellow (fireball)", "yellow", "35% Less yellow req. Damage +35%", 4, 50000, 150, 12, new List<float>() { 1, 1 }));
            yellowSkills.Add(new SkillLevelInfo("Yellow (fireball)", "yellow", "50% Less yellow req. Damage +50%", 5, 200000, 250, 15, new List<float>() { 1, 1 }));
            yellowSkills.Add(new SkillLevelInfo("Yellow (fireball)", "yellow", "Store multiple fireballs", 6, 500000, 500, 18, new List<float>() { 1, 1 }));

            string[] skillNames = new string[4] { "blue", "green", "red", "yellow" };

            PlayerData playerData = MainController.Instance.playerData;

            
            blueText.text = playerData.blueSkill.ToString() + "/" + blueSkills.Count.ToString();
            greenText.text = playerData.greenSkill.ToString() + "/" + greenSkills.Count.ToString();
            redText.text = playerData.redSkill.ToString() + "/" + redSkills.Count.ToString();
            yellowText.text = playerData.yellowSkill.ToString() + "/" + yellowSkills.Count.ToString();
            blueIcon.sprite = MainController.Data.sprites.GetSkillSprite("blue", playerData.blueSkill);
            greenIcon.sprite = MainController.Data.sprites.GetSkillSprite("green", playerData.greenSkill);
            redIcon.sprite = MainController.Data.sprites.GetSkillSprite("red", playerData.redSkill);
            yellowIcon.sprite = MainController.Data.sprites.GetSkillSprite("yellow", playerData.yellowSkill);
            

            for (int i = 0; i < skillNames.Length; i++)
            {
                GameObject skillList = Instantiate(skillLevelListPrefab, this.transform, false);
                skillList.name = skillNames[i] + "List";
                Transform contentTransform = skillList.GetComponentInChildren<VerticalLayoutGroup>().transform;

                List<SkillLevelInfo> skillLevelList = new List<SkillLevelInfo>();
                switch (skillNames[i])
                {
                    case "blue":
                        skillLevelList = blueSkills;
                        break;
                    case "green":
                        skillLevelList = greenSkills;
                        break;
                    case "red":
                        skillLevelList = redSkills;
                        break;
                    case "yellow":
                        skillLevelList = yellowSkills;
                        break;
                }
                foreach (SkillLevelInfo sli in skillLevelList)
                {
                    SkillLevelEntry sle = Instantiate(skillLevelEntryPrefab, contentTransform, false).GetComponent<SkillLevelEntry>();

                    sle.syscode = sli.syscode;
                    sle.description = sli.description;
                    sle.level = sli.level;
                    sle.coinCost = sli.coinCost;
                    sle.skillCost = sli.skillCost;
                    sle.requiredXp = sli.xpRequirement;

                    sle.SetLevelText("Level " + sli.level.ToString());
                    sle.SetDescription(sli.description);
                    sle.SetLevelImage(MainController.Data.sprites.GetSkillSprite(sli.syscode, sli.level));
                    if (playerData.GetSkillLevel(sli.syscode) >= sli.level) // Player has unlocked this already.
                    {
                        sle.UnlockSkill();
                        sle.state = SkillLevelEntry.State.Unlocked;
                    }
                    else
                    {
                        sle.SetCoinCost(sli.coinCost);
                        sle.SetSkillCost(sli.skillCost);
                        if (playerData.xp < sli.xpRequirement) // Restricted for player due to xp requirement.
                        {
                            sle.ToggleLockIcon(true);
                            sle.ToggleXpRequirementText(true, sli.xpRequirement);
                            sle.state = SkillLevelEntry.State.RestrictedByXp;
                        }
                        else if (playerData.GetSkillLevel(sli.syscode) < sli.level - 1)
                        {
                            sle.ToggleLockIcon(true);
                            sle.ToggleXpRequirementText(false, 0);
                            sle.state = SkillLevelEntry.State.RestrictedByPrevious;
                        }
                        else
                        {
                            sle.state = SkillLevelEntry.State.Locked;
                        }
                    }
                }
                skillLevelLists.Add(skillNames[i], skillList);
                skillList.SetActive(false);
            }

            SelectSkill("blue");
            */
        }

        private void OnShopLoaded(bool isSuccess, string errorMessage, EconomyService.ShopRequestObject shop)
        {
            // TODO: error popup events
            Debug.Log("Shop loading complete");

            if (!isShown) { return; }

            bool hasError = !string.IsNullOrEmpty(errorMessage);
            if (!isSuccess || shop == null || hasError)
            {
                Debug.LogError(string.Format("!isSuccess({0}), shop == null ({1}), hasError({2})", !isSuccess, shop == null, hasError));
                if (hasError) {
                    UIEvent.Info(errorMessage, PopupType.Error);
                    Debug.Log(errorMessage);
                    return;
                }
            }
            if (shop == null)
            {
                UIEvent.Info("Shop serialization failed", PopupType.Error);
                Debug.LogError("Could not create shop object.");
                return;
            }
            Debug.Log("shop.skills.Count: " + shop.skills.Count);
            PlayerData playerData = MainController.Instance.playerData; // TODO: use the received skill list to get current player level
            for (int i=0; i < shop.skills.Count; i++)
            {
                EconomyService.Skill skill = shop.skills[i];
                if (skill.levels == null || skill.levels.Count <= 0) { Debug.LogError("No level information for " + skill.name); continue; }

                GameObject skillList = Instantiate(skillLevelListPrefab, this.transform, false);
                skillList.name = skill.syscode + "List";
                Transform contentTransform = skillList.GetComponentInChildren<VerticalLayoutGroup>().transform;

                foreach (EconomyService.Level skillLevel in skill.levels)
                {
                    SkillLevelEntry sle = Instantiate(skillLevelEntryPrefab, contentTransform, false).GetComponent<SkillLevelEntry>();

                    sle.syscode = skill.syscode;
                    sle.description = skillLevel.description;
                    sle.level = skillLevel.level;
                    sle.coinCost = int.Parse(skillLevel.coins);
                    sle.skillCost = int.Parse(skillLevel.skills);
                    sle.requiredXp = int.Parse(skillLevel.xp);

                    sle.SetLevelText("Level " + skillLevel.level.ToString());
                    sle.SetDescription(skillLevel.description);
                    sle.SetLevelImage(MainController.Data.sprites.GetSkillSprite(skill.syscode, skillLevel.level-1));
                    if (playerData.GetSkillLevel(skill.syscode) >= skillLevel.level) // Player has unlocked this already.
                    {
                        sle.UnlockSkill();
                        sle.state = SkillLevelEntry.State.Unlocked;
                    }
                    else
                    {
                        sle.SetCoinCost(int.Parse(skillLevel.coins));
                        sle.SetSkillCost(int.Parse(skillLevel.skills));
                        if (playerData.xp < int.Parse(skillLevel.xp)) // Restricted for player due to xp requirement.
                        {
                            sle.ToggleLockIcon(true);
                            sle.ToggleXpRequirementText(true, int.Parse(skillLevel.xp));
                            sle.state = SkillLevelEntry.State.RestrictedByXp;
                        }
                        else if (playerData.GetSkillLevel(skill.syscode) < skillLevel.level - 1)
                        {
                            sle.ToggleLockIcon(true);
                            sle.ToggleXpRequirementText(false, 0);
                            sle.state = SkillLevelEntry.State.RestrictedByPrevious;
                        }
                        else
                        {
                            sle.state = SkillLevelEntry.State.Locked;
                        }
                    }
                }
                skillLevelLists.Add(skill.syscode, skillList);
                skillList.SetActive(false);
            }

            SelectSkill("blue");
        }

        public void SelectSkill(string syscode)
        {
            foreach (KeyValuePair<string, GameObject> kvp in skillLevelLists)
            {
                if (kvp.Key == syscode) { kvp.Value.SetActive(true); continue; }
                kvp.Value.SetActive(false);
            }
        }

        void ClearSkillLevelLists()
        {
            
            int listCount = skillLevelLists.Count;
            foreach (KeyValuePair<string, GameObject> kvp in skillLevelLists)
            {
                Destroy(kvp.Value);
            }
            skillLevelLists.Clear();
            blueSkills.Clear();
            greenSkills.Clear();
            redSkills.Clear();
            yellowSkills.Clear();
        }

        public void PreviousScreen()
        {
            GoToScreen(PrevScreen());
        }

        public void Selected(UIEntry entry, bool yes)
        {
            throw new NotImplementedException();
        }

        public void EntryPrimaryAction(UIEntry entry)
        {

        }
    }

    [System.Serializable]
    public class SkillLevelInfo
    {
        public string name;
        public string syscode;
        public string description;
        public int level;
        public int coinCost;
        public int skillCost;
        public int xpRequirement;
        public List<float> currLevel;

        public SkillLevelInfo(string name, string syscode, string description, int level, int coinCost, int skillCost, int xpRequirement, List<float> currLevel)
        {
            this.name = name;
            this.syscode = syscode;
            this.description = description;
            this.level = level;
            this.coinCost = coinCost;
            this.skillCost = skillCost;
            this.xpRequirement = xpRequirement;
            this.currLevel = currLevel;
        }

        public static SkillLevelInfo CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<SkillLevelInfo>(jsonString);
        }
        // Given JSON input:
        // {"name":"Dr Charles","lives":3,"health":0.8}
        // this example will return a PlayerInfo object with
        // name == "Dr Charles", lives == 3, and health == 0.8f.
    }
}
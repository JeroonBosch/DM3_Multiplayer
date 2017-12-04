using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class SkillLevelEntry : UIEntry
    {
        public enum Currency { Coins, SkillPoints }

        public enum State { Locked, Unlocked, RestrictedByXp, RestrictedByPrevious }
        public State state = State.RestrictedByXp;

        public string syscode;
        public string description;
        public int level;
        public int coinCost;
        public int skillCost;
        public int requiredXp;

        [SerializeField] Image levelIcon;
        [SerializeField] Text levelText;
        [SerializeField] Text descriptionText;
        [SerializeField] Text coinCostText;
        [SerializeField] Text skillCostText;
        [SerializeField] Image coinIcon;
        [SerializeField] Image skillIcon;

        [SerializeField] Image lockIcon;
        [SerializeField] Text xpRequirementText;
        [SerializeField] Image checkIcon;
        [SerializeField] Image backgroundImage;
        [SerializeField] Sprite greenSkillSprite;

        private void OnEnable()
        {
            PlayerEvent.OnSkillLevelChange += SkillLevelChange;
        }

        private void OnDisable()
        {
            PlayerEvent.OnSkillLevelChange -= SkillLevelChange;
        }

        void SkillLevelChange(string syscode, int newLevel)
        {
            if (this.syscode != syscode) { return; }
            if (level == newLevel) { // If this was the skill that was unlocked
                state = State.Unlocked;
                ToggleLockIcon(false);
                ToggleXpRequirementText(false, 0);
                UnlockSkill();
            }
            else if (level == newLevel + 1) {
                if (state == State.RestrictedByPrevious)
                {
                    state = State.Locked;
                    ToggleLockIcon(false);
                }
                if (MainController.Instance.playerData.XPLevel < requiredXp)
                {
                    state = State.RestrictedByXp;
                    ToggleLockIcon(true);
                    // ToggleXpRequirementText(true, requiredXp);
                }
            }
        }

        public void MessageAction()
        {
            switch (state)
            {
                case State.Locked:
                    // TODO: Server-side check. Does the player have enough coins?
                    if (MainController.Instance.playerData.coins >= coinCost) {
                        if (MainController.Instance.playerData.unspentSkill >= skillCost) {
                            UIEvent.SkillUpgrade(syscode, level - 1, description, coinCost, skillCost); }
                        else { UIEvent.InsufficientCurrency(Currency.SkillPoints); }
                    }
                    else { UIEvent.InsufficientCurrency(Currency.Coins); }
                    break;
                case State.RestrictedByXp:
                    UIEvent.SkillLocked("You need atleast Level " + requiredXp.ToString() + " in order to upgrade");
                    break;
                case State.RestrictedByPrevious:
                    UIEvent.SkillLocked("You need to unlock the previous level of this skill");
                    break;
            }
        }

        public void SetIFLObject(IEntryList ifl)
        {
            this.ifl = ifl;
        }
        public void SetLevelImage(Sprite newImage)
        {
            levelIcon.sprite = newImage;
        }
        public void SetLevelText(string level)
        {
            levelText.text = level;
        }
        public void SetDescription(string newText)
        {
            descriptionText.text = newText;
        }
        public void SetCoinCost(int cost)
        {
            coinCostText.text = cost.ToString();
        }
        public void SetSkillCost(int cost)
        {
            skillCostText.text = cost.ToString();
        }

        public void ToggleLockIcon(bool active)
        {
            lockIcon.gameObject.SetActive(active);
        }
        public void ToggleXpRequirementText(bool active, int requiredXp)
        {
            xpRequirementText.text = requiredXp.ToString();
            xpRequirementText.gameObject.SetActive(active);
        }
        public void UnlockSkill()
        {
            coinIcon.gameObject.SetActive(false);
            skillIcon.gameObject.SetActive(false);
            coinCostText.gameObject.SetActive(false);
            skillCostText.gameObject.SetActive(false);

            backgroundImage.sprite = greenSkillSprite;
            checkIcon.gameObject.SetActive(true);
        }
    }
}

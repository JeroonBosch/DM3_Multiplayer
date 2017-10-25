using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class PlayerProfileCanvas : BaseMenuCanvas
    {
        [SerializeField] Image playerAvatarIcon;
        [SerializeField] Image playerAvatarBorder;

        [SerializeField] Text playerNameText;
        [SerializeField] Text playerXpLevelText;
        [SerializeField] Text playerUnspentSkillPointText;
        [SerializeField] Text XpProgressText;
        [SerializeField] RectTransform XpProgressBar;

        [SerializeField] Text gamesWonText;
        [SerializeField] Text tournamentsWonText;
        [SerializeField] Text winningsText;
        [SerializeField] Text rankText;

        [Header("Skills")]
        [SerializeField] Text powerupBlueText;
        [SerializeField] Text powerupGreenText;
        [SerializeField] Text powerupRedText;
        [SerializeField] Text powerupYellowText;
        [SerializeField] Image powerupBlueIcon;
        [SerializeField] Image powerupGreenIcon;
        [SerializeField] Image powerupRedIcon;
        [SerializeField] Image powerupYellowIcon;

        protected override void OnEnable()
        {
            PlayerEvent.OnProfileNameChange += SetProfileName;
            PlayerEvent.OnXpAmountChange += XpAmountChange;
            PlayerEvent.OnUnspentSkillPointAmountChange += UnspentSkillPointAmountChange;
            PlayerEvent.OnSkillLevelChange += SkillLevelChange;

            base.OnEnable();
        }

        protected override void OnDisable()
        {
            PlayerEvent.OnProfileNameChange -= SetProfileName;
            PlayerEvent.OnXpAmountChange -= XpAmountChange;
            PlayerEvent.OnUnspentSkillPointAmountChange -= UnspentSkillPointAmountChange;
            PlayerEvent.OnSkillLevelChange -= SkillLevelChange;

            base.OnDisable();
        }

        void XpAmountChange(int amount)
        {
            SetPlayerXpLevel(amount);
        }
        void UnspentSkillPointAmountChange(int amount)
        {
            SetPlayerSkillLevel(amount);
        }

        public override void Show()
        {
            base.Show();

            // Receive these values from the server and set them.
            SetXpProgress(247);

            SetGamesWonText(33, 67);
            SetTournamentsWonText(12, 24);
            SetWinningsText(91);
            SetRankText(7451);
        }

        public void SetPlayerAvatar(Sprite playerAvatar)
        {
            playerAvatarIcon.sprite = playerAvatar;
        }
        public void SetPlayerAvatarBorder(Sprite border)
        {
            playerAvatarBorder.sprite = border;
        }

        public void SetProfileName(string newName)
        {
            playerNameText.text = newName;
        }
        public void SetPlayerXpLevel(int playerXpLevel)
        {
            playerXpLevelText.text = playerXpLevel.ToString();
        }
        public void SetPlayerSkillLevel(int playerSkillLevel)
        {
            playerUnspentSkillPointText.text = playerSkillLevel.ToString();
        }
        public void SetXpProgress(int newProgress)
        {
            float requiredProgress = 1000; // TODO: Retrieve this value from server.

            float xValue = (float)newProgress / (float)requiredProgress;
            Vector3 newScale = new Vector3(xValue, XpProgressBar.localScale.y, XpProgressBar.localScale.z);
            XpProgressBar.localScale = newScale;

            XpProgressText.text = newProgress.ToString() + "/" + requiredProgress.ToString();
        }

        public void SetGamesWonText(int wonAmount, int totalAmount)
        {
            gamesWonText.text = "Games won: " + wonAmount.ToString() + " of " + totalAmount.ToString();
        }
        public void SetTournamentsWonText(int wonAmount, int totalAmount)
        {
            gamesWonText.text = "Tournaments won: " + wonAmount.ToString() + " of " + totalAmount.ToString();
        }
        public void SetWinningsText(int totalAmount)
        {
            gamesWonText.text = "Total winnings: " + totalAmount.ToString();
        }
        public void SetRankText(int rank)
        {
            rankText.text = "Rank: " + rank.ToString();
        }

        public void SkillLevelChange(string skillColor, int amount)
        {
            Text chosenText = null;
            Image chosenIcon = null;
            switch (skillColor)
            {
                case "blue":
                    chosenText = powerupBlueText;
                    chosenIcon = powerupBlueIcon;
                    break;
                case "green":
                    chosenText = powerupGreenText;
                    chosenIcon = powerupGreenIcon;
                    break;
                case "red":
                    chosenText = powerupRedText;
                    chosenIcon = powerupRedIcon;
                    break;
                case "yellow":
                    chosenText = powerupYellowText;
                    chosenIcon = powerupYellowIcon;
                    break;
            }
            if (chosenText != null && chosenIcon != null)
            {
                int maxLevel = Constants.MaxSkillLevel;
                chosenText.text = amount.ToString() + "/" + maxLevel.ToString();
                chosenIcon.sprite = MainController.Data.sprites.GetSkillSprite(skillColor, amount);
            }
        }

        public void UpgradeSkills()
        {
            GoToScreen(GameObject.Find("UpgradeSkillsScreen").GetComponent<UpgradeSkillCanvas>());
        }

        public void Leaderboards()
        {
            // GoToScreen(GameObject.Find("LeaderBoardsScreen").GetComponent<SelectStageCanvas>());
        }

        public void PreviousScreen()
        {
            GoToScreen(PrevScreen());
        }
    }
}
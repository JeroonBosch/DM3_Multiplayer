using System;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class PlayerProfileCanvas : BaseMenuCanvas
    {
        [SerializeField] Image playerAvatarIcon;
        [SerializeField] Image avatarBorderImage;

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
            PlayerEvent.OnProfileImageChange += OnProfileImageChange;
            PlayerEvent.OnAvatarBorderChange += SetPlayerAvatarBorder;
            PlayerEvent.OnProfileNameChange += SetProfileName;
            PlayerEvent.OnXPLevelChange += XPLevelChange;
            PlayerEvent.OnXPLevelGainChange += XPLevelGainChange;
            PlayerEvent.OnXPLevelGainCurrentChange += XPLevelGainCurrentChange;
            PlayerEvent.OnMatchesTotalChange += OnMatchesTotalChange;
            PlayerEvent.OnMatchesWinsChange += OnMatchesWinsChange;
            PlayerEvent.OnTournamentGamesChange += OnTournamentGamesChange;
            PlayerEvent.OnTournamentWinsChange += OnTournamentWinsChange;
            PlayerEvent.OnTotalWinningsChange += OnTotalWinningsChange;
            PlayerEvent.OnWeeklyRankChange += OnWeeklyRankChange;
            PlayerEvent.OnUnspentSkillPointAmountChange += UnspentSkillPointAmountChange;
            PlayerEvent.OnSkillLevelChange += SkillLevelChange;

            base.OnEnable();
        }

        protected override void OnDisable()
        {
            PlayerEvent.OnProfileImageChange -= OnProfileImageChange;
            PlayerEvent.OnAvatarBorderChange -= SetPlayerAvatarBorder;
            PlayerEvent.OnProfileNameChange -= SetProfileName;
            PlayerEvent.OnXPLevelChange -= XPLevelChange;
            PlayerEvent.OnXPLevelGainChange -= XPLevelGainChange;
            PlayerEvent.OnXPLevelGainCurrentChange -= XPLevelGainCurrentChange;
            PlayerEvent.OnMatchesTotalChange -= OnMatchesTotalChange;
            PlayerEvent.OnMatchesWinsChange -= OnMatchesWinsChange;
            PlayerEvent.OnTournamentGamesChange -= OnTournamentGamesChange;
            PlayerEvent.OnTournamentWinsChange -= OnTournamentWinsChange;
            PlayerEvent.OnTotalWinningsChange -= OnTotalWinningsChange;
            PlayerEvent.OnWeeklyRankChange -= OnWeeklyRankChange;
            PlayerEvent.OnUnspentSkillPointAmountChange -= UnspentSkillPointAmountChange;
            PlayerEvent.OnSkillLevelChange -= SkillLevelChange;

            base.OnDisable();
        }

        void XPLevelChange(int amount)
        {
            SetPlayerXpLevel(amount);
        }
        private void XPLevelGainChange(int amount)
        {
            SetXpProgress(MainController.Instance.playerData.XPLevelGainCurrent, amount);
        }
        private void XPLevelGainCurrentChange(int amount)
        {
            SetXpProgress(amount, MainController.Instance.playerData.XPLevelGain);
        }
        private void OnMatchesTotalChange(int amount)
        {
            if (MainController.Instance != null && MainController.Instance.playerData != null)
            {
                SetGamesWonText(MainController.Instance.playerData.MatchesWins, amount);
            }
        }
        private void OnMatchesWinsChange(int amount)
        {
            if (MainController.Instance != null && MainController.Instance.playerData != null)
            {
                SetGamesWonText(amount, MainController.Instance.playerData.MatchesTotal);
            }
        }
        private void OnTournamentGamesChange(int amount)
        {
            if (MainController.Instance != null && MainController.Instance.playerData != null)
            {
                SetTournamentsWonText(MainController.Instance.playerData.TournamentWins, amount);
            }
        }
        private void OnTournamentWinsChange(int amount)
        {
            if (MainController.Instance != null && MainController.Instance.playerData != null)
            {
                SetTournamentsWonText(amount, MainController.Instance.playerData.TournamentGames);
            }
        }
        private void OnTotalWinningsChange(int amount)
        {
            SetWinningsText(amount);
        }
        private void OnWeeklyRankChange(int amount)
        {
            SetRankText(amount);
        }

        void UnspentSkillPointAmountChange(int amount)
        {
            SetPlayerSkillLevel(amount);
        }

        public void OnProfileImageChange(Texture2D newImage)
        {
            if (newImage == null) { return; }
            SetPlayerAvatar(Sprite.Create(newImage, new Rect(0, 0, newImage.width, newImage.height), new Vector2(0, 0)));
        }
        public void SetPlayerAvatar(Sprite playerAvatar)
        {
            playerAvatarIcon.sprite = playerAvatar;
        }
        public void SetPlayerAvatarBorder(Sprite border)
        {
            avatarBorderImage.sprite = border;
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
        public void SetXpProgress(int newProgress, int requiredXp)
        {
            if (requiredXp == 0) { return; }
            float xValue = (float)newProgress / (float)requiredXp;
            Vector3 newScale = new Vector3(xValue, XpProgressBar.localScale.y, XpProgressBar.localScale.z);
            XpProgressBar.localScale = newScale;

            XpProgressText.text = newProgress.ToString() + "/" + requiredXp.ToString();
        }

        public void SetGamesWonText(int wonAmount, int totalAmount)
        {
            Debug.Log("here");
            gamesWonText.text = "Games won: " + wonAmount.ToString() + " of " + totalAmount.ToString();
        }
        public void SetTournamentsWonText(int wonAmount, int totalAmount)
        {
            tournamentsWonText.text = "Tournaments won: " + wonAmount.ToString() + " of " + totalAmount.ToString();
        }
        public void SetWinningsText(int totalAmount)
        {
            winningsText.text = "Total winnings: " + totalAmount.ToString();
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
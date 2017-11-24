using System;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class MainmenuCanvas : BaseMenuCanvas
    {
        [SerializeField] Image profileImage;
        [SerializeField] Image avatarBorderImage;
        [SerializeField] Text profileNameText;
        [SerializeField] Text xpText;
        [SerializeField] Text unspentSkillPointText;

        protected override void OnEnable()
        {
            PlayerEvent.OnProfileImageChange += SetProfileImage;
            PlayerEvent.OnAvatarBorderChange += OnAvatarBorderChange;
            PlayerEvent.OnProfileNameChange += SetProfileName;
            PlayerEvent.OnXpAmountChange += XpAmountChange;
            PlayerEvent.OnUnspentSkillPointAmountChange += UnspentSkillPointAmountChange;

            base.OnEnable();
        }

        protected override void OnDisable()
        {
            PlayerEvent.OnProfileImageChange -= SetProfileImage;
            PlayerEvent.OnAvatarBorderChange -= OnAvatarBorderChange;
            PlayerEvent.OnProfileNameChange -= SetProfileName;
            PlayerEvent.OnXpAmountChange -= XpAmountChange;
            PlayerEvent.OnUnspentSkillPointAmountChange -= UnspentSkillPointAmountChange;

            base.OnDisable();
        }

        public void SetProfileImage(Texture2D newImage)
        {
            if (newImage == null) { return; }
            profileImage.sprite = Sprite.Create(newImage, new Rect(0, 0, newImage.width, newImage.height), new Vector2(0, 0));
        }
        private void OnAvatarBorderChange(Sprite newSprite)
        {
            avatarBorderImage.sprite = newSprite;
        }
        public void SetProfileName(string newName)
        {
            profileNameText.text = newName;
        }
        void XpAmountChange(int amount)
        {
            xpText.text = amount.ToString();
        }
        void UnspentSkillPointAmountChange(int amount)
        {
            unspentSkillPointText.text = amount.ToString();
        }

        public void PlayNormalGame()
        {
            GoToScreen(GameObject.Find("NormalGameScreen").GetComponent<SelectStageCanvas>());
            PhotonController.Instance.ConnectNormalLobby();
        }

        public void PlayTournament()
        {
            GoToScreen(GameObject.Find("TournamentGameScreen").GetComponent<SelectStageCanvas>());
            PhotonController.Instance.ConnectTournamentLobby();
        }

        public void Preferences()
        {
            GoToScreen(GameObject.Find("SettingsScreen").GetComponent<SettingsCanvas>());
        }

        public void PlayerProfile()
        {
            GoToScreen(GameObject.Find("PlayerProfileScreen").GetComponent<PlayerProfileCanvas>());
        }

        public void Shop()
        {
            GoToScreen(GameObject.Find("ShopScreen").GetComponent<ShopCanvas>());
        }

        public void CoinShop()
        {
            GoToScreen(GameObject.Find("CoinShopScreen").GetComponent<CoinShopCanvas>());
        }

        public void FreeCoins()
        {
            GoToScreen(GameObject.Find("FreeCoinsScreen").GetComponent<FreeCoinsCanvas>());
        }

        public void SkillUpgrades()
        {
            GoToScreen(GameObject.Find("UpgradeSkillsScreen").GetComponent<UpgradeSkillCanvas>());
        }

        public void Gifts()
        {
            GoToScreen(GameObject.Find("GiftsScreen").GetComponent<GiftsCanvas>());
        }

        public void InviteFriends()
        {
            GoToScreen(GameObject.Find("InviteFriendsScreen").GetComponent<InviteFriendsCanvas>());
        }

        public void Leaderboards()
        {
            // GoToScreen(GameObject.Find("LeaderBoardsScreen").GetComponent<SelectStageCanvas>());
        }
    }
}
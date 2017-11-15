using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace Com.Hypester.DM3
{
    public class EndScreenCanvas : BaseMenuCanvas
    {
        public PlayerInfoMatchEnd localPlayerInfo; 
        public PlayerInfoMatchEnd remotePlayerInfo;

        protected override void OnEnable()
        {
            PlayerEvent.OnPlayerWantsRematch += OnPlayerWantsRematch;
        }

        protected override void OnDisable()
        {
            PlayerEvent.OnPlayerWantsRematch -= OnPlayerWantsRematch;
        }

        protected override void Start()
        {
            base.Start();
            Hide();
        }

        public override void Show()
        {
            base.Show();

            foreach (KeyValuePair<int, Player> kvp in PlayerManager.instance.GetAllPlayers()) {
                Player player = kvp.Value;
                PlayerInfoMatchEnd playerInfo = player.IsLocal() ? localPlayerInfo : remotePlayerInfo;

                player.ToggleFingerTracker(false);

                playerInfo.SetCurrentXpLevelText(player.currentXpLevel);
                if (!string.IsNullOrEmpty(player.profileName)) { playerInfo.SetPlayerNameText(player.profileName); }
                if (!string.IsNullOrEmpty(player.avatarBorderSyscode)) { playerInfo.SetBorderImage(MainController.Data.sprites.GetAvatarBorderEntry(player.avatarBorderSyscode).normal); }
                if (player.profilePicSprite != null) { playerInfo.SetAvatarImage(player.profilePicSprite); }
                else if (!string.IsNullOrEmpty(player.profilePicURL))
                {
                    MainController.ServiceAsset.StartCoroutine(MainController.ServiceAsset.ImageFromURL(player.GetPlayerId(), player.profilePicURL, OnLoadPlayerProfileImage));
                }
                // TODO: Add gains here when connected to CMS.
            }
        }

        private void OnLoadPlayerProfileImage(Sprite obj, int playerId)
        {
            if (obj == null) { Debug.LogWarning("EndScreenCanvas(): Failed to load player profile image"); return; }
            if (playerId == PhotonNetwork.player.ID) { localPlayerInfo.SetAvatarImage(obj); } // Local
            else { remotePlayerInfo.SetAvatarImage(obj); } // Remote
        }

        private void OnPlayerWantsRematch(int playerId)
        {
            if (playerId == PhotonNetwork.player.ID) // Local player
            {
                localPlayerInfo.ToggleRematchImage(true);
            } else { remotePlayerInfo.ToggleRematchImage(true); } // Remote player

            if (localPlayerInfo.wantsRematch && remotePlayerInfo.wantsRematch)
            {
                // localPlayerInfo.ToggleRematchImage(false);
                // remotePlayerInfo.ToggleRematchImage(false);
                PhotonController.Instance.Rematch();
            }
        }

        public void SetWinner(int winnerPlayer)
        {
            bool localPlayerWon = (winnerPlayer == 0 && PhotonNetwork.isMasterClient) || (winnerPlayer == 1 && !PhotonNetwork.isMasterClient); // 0 = master client

            PlayerInfoMatchEnd winnerPlayerInfo = localPlayerWon ? localPlayerInfo : remotePlayerInfo;

            winnerPlayerInfo.ToggleWinnerText(true);
            int wonPlayerPhotonId = localPlayerWon ? PhotonNetwork.player.ID : PhotonNetwork.otherPlayers[0].ID; // TODO: foolproof this here
            winnerPlayerInfo.SetBorderImage(MainController.Data.sprites.GetAvatarBorderEntry(PlayerManager.instance.GetPlayerById(wonPlayerPhotonId).avatarBorderSyscode).winner);
        }

        public void BackToMenu ()
        {
            PhotonController.Instance.StopMultiplayer();
        }

        public void RequestRematch()
        {
            PhotonController.Instance.GameController.MyPlayer.photonView.RPC("RPC_RequestRematch", PhotonTargets.AllViaServer, PhotonNetwork.player.ID); // Only the local player calls this.
        }
    }
}
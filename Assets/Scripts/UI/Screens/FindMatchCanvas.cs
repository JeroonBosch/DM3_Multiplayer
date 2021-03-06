﻿using System;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public partial class FindMatchCanvas : BaseMenuCanvas
    {
        [SerializeField] RawImage backgroundImage;
        [SerializeField] Text infoText;

        [SerializeField] Image localPlayerAvatarImage;
        [SerializeField] Image localPlayerBorderImage;
        [SerializeField] Image localPlayerFlagImage;
        [SerializeField] Text localPlayerNameText;
        [SerializeField] Text localPlayerXpText;
        [SerializeField] GameObject localPlayerXpIcon;

        [SerializeField] Animator opponentSearchAnimator;
        [SerializeField] Image remotePlayerAvatarImage;
        [SerializeField] Image remotePlayerBorderImage;
        [SerializeField] Image remotePlayerFlagImage;
        [SerializeField] Text remotePlayerNameText;
        [SerializeField] Text remotePlayerXpText;
        [SerializeField] GameObject remotePlayerXpIcon;

        [SerializeField] Text potSizeTitleText;
        [SerializeField] Text potSizeValueText;
        [SerializeField] Image potSizeValueIcon;

        [SerializeField] GameObject backButton;

        PhotonPlayer remotePlayer;
        private float _timer = 0f;
        private float _timeUntilStart = 3f;
        private bool _starting = false;
        private bool _started = false;

        protected FindMatchState _state;
        protected FindMatchState state { get { return _state; } set { _state = value; _state.OnEnter(); stateName = state.GetType().Name; } }
        public string stateName;
        public FindMatchStateFactory factory;

        private void Awake()
        {
            factory = new FindMatchStateFactory(this);
            state = factory.searchingOpponent;
        }

        protected override void Start ()
        {
            base.Start();

            RefreshLocalPlayerData();

            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player"))
            {
                go.transform.Find("FingerTracker").GetComponent<Image>().enabled = false;
            }
        }

        protected override void Update()
        {
            base.Update();
            state.Update();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SceneManager.sceneLoaded += OnSceneLoaded;
            NetworkEvent.OnPhotonPlayerDisconnected += PhotonPlayerDisconnected;
            PlayerEvent.OnPlayerStatsUpdate += OnPlayerStatsUpdate;
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            NetworkEvent.OnPhotonPlayerDisconnected -= PhotonPlayerDisconnected;
            PlayerEvent.OnPlayerStatsUpdate -= OnPlayerStatsUpdate;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("OnSceneLoaded: " + scene.name);
            if (PhotonController.Instance != null)
            {
                if (!string.IsNullOrEmpty(PhotonController.Instance.latestStageSyscode))
                {
                    Sprite backgroundSprite = MainController.Data.sprites.GetStageArt(PhotonController.Instance.latestStageSyscode).bg;
                    backgroundImage.texture = backgroundSprite ? backgroundSprite.texture : backgroundImage.texture;
                }
                if (PhotonController.Instance.latestPotSize > 0)
                {
                    potSizeTitleText.enabled = true;
                    potSizeValueText.text = PhotonController.Instance.latestPotSize.ToString();
                    potSizeValueIcon.enabled = true;
                }
            }
        }
        private void PhotonPlayerDisconnected(PhotonPlayer otherPlayer)
        {
            Debug.Log("FindMatchCanvas() PhotonPlayerDisconnected");
            if (PhotonNetwork.otherPlayers.Length < 1 && PhotonNetwork.connected)
            {
                state = factory.searchingOpponent;
            }
        }
        private void OnPlayerStatsUpdate(int playerId, Hashtable stats)
        {
            if (playerId != PhotonNetwork.player.ID) // remote player
            {
                Debug.LogError("OnPlayerStatsUpdate(" + stats.Count.ToString() + ")");
                // These stats should never be null objects or empty strings, because if they were, they would not have been added to the hashtable in the first place.
                if (stats.ContainsKey(PlayerProperty.ProfileImageUrl))
                {
                    MainController.ServiceAsset.StartCoroutine(MainController.ServiceAsset.ImageFromURL(playerId, (string)stats[PlayerProperty.ProfileImageUrl], OnLoadRemotePlayerProfileImage));
                }
                if (stats.ContainsKey(PlayerProperty.XpLevel))
                {
                    SetRemotePlayerXpText(((int)stats[PlayerProperty.XpLevel]).ToString());
                }
                if (stats.ContainsKey(PlayerProperty.AvatarBorderSyscode))
                {
                    Debug.LogError("ContainsKey AvatarSys: " + (string)stats[PlayerProperty.AvatarBorderSyscode]);
                    SetRemotePlayerAvatarBorder((string) stats[PlayerProperty.AvatarBorderSyscode]);
                }
            }
        }

        protected void SearchOpponentStart()
        {
            backButton.SetActive(true);

            PhotonNetwork.player.SetCustomProperties(new Hashtable() { { PlayerProperty.State, null } });

            remotePlayer = null;
            factory.gettingGameInfo.remotePlayerUserId = "";
            ResetRemotePlayerData();

            opponentSearchAnimator.enabled = true;

            SetInfoText("Finding opponent...");
        }
        protected void SearchOpponentUpdate()
        {
            if (PhotonNetwork.otherPlayers.Length == 1)
            {
                remotePlayer = PhotonNetwork.otherPlayers[0];
                state = factory.gatheringOpponentData;
            }
        }

        protected void GatherOpponentDataStart()
        {
            backButton.SetActive(false);
            SetInfoText("Gathering opponent info...");
            opponentSearchAnimator.enabled = false;
        }
        protected void GatherOpponentDataUpdate()
        {
            Debug.Log("GatherOpponentDataUpdate()");
            Debug.Log("remotePlayer:" + remotePlayer + ", remotePlayer.CustomProperties[PlayerProperty.UserId]: " + ((string) remotePlayer.CustomProperties[PlayerProperty.UserId]));
            if (remotePlayer != null && remotePlayer.CustomProperties[PlayerProperty.UserId] != null)
            {
                string remotePlayerUserId = (string) remotePlayer.CustomProperties[PlayerProperty.UserId];
                if (remotePlayer.CustomProperties.ContainsKey(PlayerProperty.AvatarBorderSyscode) && !string.IsNullOrEmpty((string) remotePlayer.CustomProperties[PlayerProperty.AvatarBorderSyscode]))
                {
                    string avatarBorderSyscode = (string)remotePlayer.CustomProperties[PlayerProperty.AvatarBorderSyscode];
                    Player remote = PlayerManager.instance.GetPlayerById(remotePlayer.ID);
                    if (remote != null) { remote.avatarBorderSyscode = avatarBorderSyscode; }
                    SetRemotePlayerAvatarBorder(avatarBorderSyscode);
                }
                factory.gettingGameInfo.remotePlayerUserId = remotePlayerUserId;
                state = factory.gettingGameInfo;
            }
        }

        protected void GettingGameInfoStart()
        {
            SetInfoText("Getting game info...");
            MainController.ServiceGame.GetGame((string)PhotonNetwork.room.CustomProperties[RoomProperty.StageId], factory.gettingGameInfo.remotePlayerUserId, OnGetGameInfo);
        }
        protected void GettingGameInfoUpdate()
        {
            if (remotePlayer == null) { Debug.Log("We lost our remote. Fookin a!"); state = factory.searchingOpponent; return; }
            if (PhotonNetwork.player.CustomProperties[PlayerProperty.State] != null && (((PlayerState) PhotonNetwork.player.CustomProperties[PlayerProperty.State]) == PlayerState.GettingGameInfoWaitForOther))
            {
                if (remotePlayer.CustomProperties[PlayerProperty.State] != null && ((PlayerState)remotePlayer.CustomProperties[PlayerProperty.State] == PlayerState.GettingGameInfoWaitForOther)) // Both players got the info. Go!
                {
                    Debug.Log("Both players registered their game (getGame)");
                    state = factory.startingGame;
                }
            }
        }

        private void OnGetGameInfo(bool success, string errorMessage, GameService.GetGameRequestObject result)
        {
            Debug.Log("Got the game info");
            if (!success)
            {
                string errorMsg = string.IsNullOrEmpty(errorMessage) ? "Connection to server failed." : errorMessage;
                UIEvent.Info(errorMsg, PopupType.Error);
                state = factory.searchingOpponent;
                return;
            }
            if (result == null) { UIEvent.Info("Failed to get game info (result).", PopupType.Error); state = factory.searchingOpponent; return; }
            if (result.game == null) { UIEvent.Info("Failed to get game info (game).", PopupType.Error); state = factory.searchingOpponent; return; }
            if (result.game.opponent == null) { UIEvent.Info("Failed to get game info (opponent).", PopupType.Error); state = factory.searchingOpponent; return; }
            if (string.IsNullOrEmpty(result.game.opponent.id)) { UIEvent.Info("Failed to get game info (opponent.id).", PopupType.Error); state = factory.searchingOpponent; return; }
            if (result.game.stage == null) { UIEvent.Info("Failed to get game info (stage).", PopupType.Error); state = factory.searchingOpponent; return; }

            GameService.Opponent opponent = result.game.opponent;
            SetRemotePlayerName(opponent.first_name);
            SetRemotePlayerXpText(opponent.XPlevel.ToString());
            if (!string.IsNullOrEmpty(opponent.pic)) {
                Debug.Log("opponent img url: " + opponent.pic);
                MainController.ServiceAsset.StartCoroutine(MainController.ServiceAsset.ImageFromURL(remotePlayer.ID, opponent.pic, OnLoadRemotePlayerProfileImage));
            }

            DevConsole.gameId = result.game.id.ToString();
            factory.startingGame.gameId = result.game.id;
            Hashtable props = new Hashtable();
            props.Add(PlayerProperty.State, PlayerState.GettingGameInfoWaitForOther);
            props.Add(PlayerProperty.GameId, result.game.id);
            PhotonNetwork.player.SetCustomProperties(props);
        }
        protected void StartingGameStart()
        {
            _timer = 0;
            _starting = true;
            _started = false;
        }
        protected void StartingGameUpdate()
        {
            if (_started) { return; }
            if (remotePlayer == null || PhotonNetwork.otherPlayers.Length < 1)
            {
                Debug.Log("We lost our remote. Fookin a!"); state = factory.searchingOpponent;
                return;
            }

            if (_starting)
            {
                _timer += Time.deltaTime;
                SetInfoText("Game starts in..." + ((int)_timeUntilStart - (int)_timer).ToString());
                if (_timer > _timeUntilStart)
                {
                    SetInfoText("Game Starting");
                    _starting = false;
                    MainController.ServiceGame.StartGame(factory.startingGame.gameId, OnStartGame);
                }
                return;
            }

            if (PhotonNetwork.player.CustomProperties[PlayerProperty.State] != null && ((PlayerState)PhotonNetwork.player.CustomProperties[PlayerProperty.State] == PlayerState.StartingGameWaitForOther))
            {
                if (remotePlayer.CustomProperties[PlayerProperty.State] != null && ((PlayerState)remotePlayer.CustomProperties[PlayerProperty.State] == PlayerState.StartingGameWaitForOther)) // Both players got the info. Go!
                {
                    Player localPlayer = PlayerManager.instance.GetPlayerById(PhotonNetwork.player.ID);
                    localPlayer.opponent = PlayerManager.instance.GetPlayerById(PhotonNetwork.otherPlayers[0].ID);
                    localPlayer.opponent.opponent = localPlayer;

                    Debug.Log("Both players have started their game (startGame)");
                    _started = true;
                    PhotonNetwork.LoadLevel("Match");
                    enabled = false;
                }
            }
        }
            
        private void OnStartGame(bool success, string errorMessage, GameService.StartGameRequestObject result)
        {
            Debug.Log("Got the start info");
            if (!success)
            {
                string errorMsg = string.IsNullOrEmpty(errorMessage) ? "Connection to server failed." : errorMessage;
                UIEvent.Info(errorMsg, PopupType.Error);
                state = factory.searchingOpponent;
                return;
            }
            if (result == null) { UIEvent.Info("Failed to get start info (result).", PopupType.Error); state = factory.searchingOpponent; return; }

            PhotonNetwork.player.SetCustomProperties(new Hashtable() { { PlayerProperty.State, PlayerState.StartingGameWaitForOther } });
        }

        private void RefreshLocalPlayerData()
        {
            MainController.ServiceAsset.StartCoroutine(MainController.ServiceAsset.ImageFromURL(PhotonNetwork.player.ID, MainController.Instance.playerData.pictureURL, OnLoadLocalPlayerProfileImage));
            SetLocalPlayerAvatarBorder(MainController.Instance.playerData.avatarBorderSyscode);
            SetLocalPlayerName(MainController.Instance.playerData.profileName);
            SetLocalPlayerXpText(MainController.Instance.playerData.XPLevel.ToString());
        }
        private void ResetRemotePlayerData()
        {
            Debug.Log("Resetting remote");
            remotePlayerAvatarImage.sprite = MainController.Data.sprites.randomAvatarSheet;
            remotePlayerBorderImage.sprite = MainController.Data.sprites.defaultNormalAvatarBorder;
            // remotePlayerBorderImage; TODO: Reset border
            // SetRemotePlayerFlag("");
            SetRemotePlayerName("");
            SetRemotePlayerXpText("");
        }
        private void OnLoadLocalPlayerProfileImage(Sprite playerSprite, int playerId)
        {
            localPlayerAvatarImage.sprite = playerSprite;

        }
        private void OnLoadRemotePlayerProfileImage(Sprite playerSprite, int playerId)
        {
            if (playerSprite == null) { Debug.LogWarning("playerSprite is null! Returning"); return; }
            remotePlayerAvatarImage.sprite = playerSprite;
            Player remote = PlayerManager.instance.GetPlayerById(playerId);
            if (remote != null) { remote.profilePicSprite = playerSprite; }
        }

        private void SetInfoText(string msg)
        {
            infoText.text = msg;
        }
        private void SetLocalPlayerAvatarBorder(string syscode)
        {
            localPlayerBorderImage.sprite = MainController.Data.sprites.GetAvatarBorderEntry(syscode).normal;
        }
        private void SetRemotePlayerAvatarBorder(string syscode)
        {
            remotePlayerBorderImage.sprite = MainController.Data.sprites.GetAvatarBorderEntry(syscode).normal;
        }
        private void SetLocalPlayerFlag(string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode)) { localPlayerFlagImage.enabled = false; }
        }
        private void SetRemotePlayerFlag(string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode)) { remotePlayerFlagImage.enabled = false; }
        }
        private void SetLocalPlayerName(string name)
        {
            localPlayerNameText.text = name;
        }
        private void SetRemotePlayerName(string name)
        {
            remotePlayerNameText.text = name;
        }
        private void SetLocalPlayerXpText(string xpLevel)
        {
            localPlayerXpText.text = xpLevel;
            localPlayerXpIcon.SetActive(!string.IsNullOrEmpty(xpLevel));
        }
        private void SetRemotePlayerXpText(string xpLevel)
        {
            remotePlayerXpText.text = xpLevel;
            remotePlayerXpIcon.SetActive(!string.IsNullOrEmpty(xpLevel));
        }

        public void PreviousScreen()
        {
            if (PhotonNetwork.inRoom) { PhotonController.Instance.StopMultiplayer(); }
        }
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon;
using ExitGames.Client.Photon;

namespace Com.Hypester.DM3
{
    public class PhotonController : PunBehaviour
    {
        private bool _connect;

        private TypedLobby _normalLobby;
        private TypedLobby _tournamentLobby;

        public bool tournamentMode;

        private GameHandler _game;
        public int gameID_requested;
        public GameHandler GameController { get { return GetGameController(); } set { _game = value; } }

		private string latestStageId = "";
        public string latestStageSyscode = "";
        public int latestPotSize = 0;

        private static PhotonController instance;
        public static PhotonController Instance
        {
            get { return instance ?? (instance = new GameObject("PhotonController").AddComponent<PhotonController>()); }
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            _normalLobby = new TypedLobby() { Name = "NormalGame", Type = LobbyType.Default };
            _tournamentLobby = new TypedLobby() { Name = "Tournament", Type = LobbyType.Default };

            tournamentMode = false;
        }

        private void Update()
        {
            if (PhotonNetwork.inRoom && SceneManager.GetActiveScene().name == "Menu")
            {
                if (PhotonNetwork.room.MaxPlayers == 2)
                    PhotonNetwork.LoadLevel("NormalGame");
                else
                    PhotonNetwork.LoadLevel("TournamentGame");

                Debug.Log("Loading match-making. MaxPlayers(" + PhotonNetwork.room.MaxPlayers + ")");
            }

            if (_connect && !PhotonNetwork.connected && !PhotonNetwork.connecting) { PhotonNetwork.ConnectUsingSettings("v0.2"); }
        }

        private GameHandler GetGameController ()
        {
            if (_game == null)
            {
                foreach (GameHandler gh in FindObjectsOfType<GameHandler>())
                {
                    //Debug.Log("GameHandler with " + gh.GameID + " as ID was found. " + gameID_requested + " is requested.");
                    if (gh.GameID == gameID_requested)
                        return gh;
                }
            } else { return _game; }

            return null;
        }

        public void EnsureConnection ()
        {
            _connect = true;
        }

        public void MatchPlayers(StageEntry se)
        {
            Debug.Log("Match Players");
            // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
            if (PhotonNetwork.connected)
            {
                // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnPhotonRandomJoinFailed() and we'll create one.
                if (PhotonNetwork.insideLobby)
                {
					latestStageId = se.GetStageId();
                    latestStageSyscode = se.GetStageSyscode();
                    latestPotSize = se.coinPrize;
                    Hashtable expectedRoomProps = new Hashtable
                    {
                        { RoomProperty.StageId, se.GetStageId() },
                        { RoomProperty.StageSyscode, se.GetStageSyscode() }
                    };

                    if (PhotonNetwork.lobby == _normalLobby)
						PhotonNetwork.JoinRandomRoom(null, 2); //2 player match
                    else if (PhotonNetwork.lobby == _tournamentLobby)
						PhotonNetwork.JoinRandomRoom(expectedRoomProps, 8); //tournament match
                }
            }
        }

        public void ConnectNormalLobby ()
        {
            PhotonNetwork.JoinLobby(_normalLobby);
            PhotonNetwork.playerName = MainController.Instance.playerData.profileName;
        }

        public override void OnDisconnectedFromPhoton()
        {
            base.OnDisconnectedFromPhoton();
            Debug.Log("OnDisconnectedFromPhoton");

            NetworkEvent.PhotonDisconnected();
        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            Debug.Log("OnConnectedToMaster");
        }
        

        public void ConnectTournamentLobby()
        {
            PhotonNetwork.JoinLobby(_tournamentLobby);
            PhotonNetwork.playerName = MainController.Instance.playerData.profileName;
        }

        public override void OnFailedToConnectToPhoton(DisconnectCause cause)
        {
            base.OnFailedToConnectToPhoton(cause);

            NetworkEvent.FailedToConnectToPhoton(cause);
        }
        public override void OnConnectedToPhoton()
        {
            base.OnConnectedToPhoton();

            Debug.Log("OnConnectedToPhoton");

            NetworkEvent.ConnectedToPhoton();
        }

        public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
        {
            Debug.Log("OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 2}, null);");

            // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
			if (PhotonNetwork.insideLobby && !string.IsNullOrEmpty(latestStageId))
            {
                Debug.Log("We are inside a lobby(" + PhotonNetwork.lobby.ToString() + ") and latestStageId is not null(" + latestStageId.ToString() + ")");
				TypedLobby typedLobby = PhotonNetwork.lobby == _normalLobby ? _normalLobby : _tournamentLobby;

				RoomOptions roomOptions = new RoomOptions ();
                if (PhotonNetwork.lobby == _normalLobby)
                {
                    roomOptions.MaxPlayers = 2;
                } else { roomOptions.MaxPlayers = 8; }
                
				roomOptions.CustomRoomProperties = new Hashtable () { { RoomProperty.StageId, latestStageId }, { RoomProperty.StageSyscode, latestStageSyscode } };

				PhotonNetwork.CreateRoom(null, roomOptions, typedLobby);
            }
        }

        public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
        {
            base.OnPhotonJoinRoomFailed(codeAndMsg);

            Debug.Log("JoinRoomFailed: (" + ((short)codeAndMsg[0]).ToString() + ") " + ((string)codeAndMsg[1]).ToString());
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Room joined. Name: " + PhotonNetwork.room.Name);

            CreatePlayers();
        }

        public void CreatePlayers ()
        {
            PlayerManager.instance.CreatePlayer();

            PlayerData playerData = MainController.Instance.playerData;
            Hashtable playerProps = new Hashtable();

            playerProps.Add(PlayerProperty.AvatarBorderSyscode, playerData.avatarBorderSyscode);
            playerProps.Add(PlayerProperty.UserId, playerData.userId);
            playerProps.Add(PlayerProperty.XpLevel, playerData.xp);
            playerProps.Add(PlayerProperty.BlueSkillLevel, playerData.blueSkill);
            playerProps.Add(PlayerProperty.GreenSkillLevel, playerData.greenSkill);
            playerProps.Add(PlayerProperty.RedSkillLevel, playerData.redSkill);
            playerProps.Add(PlayerProperty.YellowSkillLevel, playerData.yellowSkill);
            if (!string.IsNullOrEmpty(playerData.pictureURL)) { playerProps.Add(PlayerProperty.ProfileImageUrl, playerData.pictureURL); }

            PhotonNetwork.player.SetCustomProperties(playerProps);
        }

        public override void OnLeftRoom()
        {
            Debug.Log("OnLeftRoom; inRoom(" + (PhotonNetwork.inRoom).ToString() + "), SceneManager.GetActiveScene().name != Menu("+ (SceneManager.GetActiveScene().name != "Menu").ToString() + ")");
            foreach (Player player in FindObjectsOfType<Player>())
            {//TODO maybe not needed / move to when leaving room
                Destroy(player.gameObject);
            }
            if (SceneManager.GetActiveScene().name != "Menu")
            {
                Debug.Log("PhotonNetwork.inRoom && SceneManager.GetActiveScene().name != Menu");
                //instead, should set health to 0?
                PhotonNetwork.LoadLevel("Menu");
                Debug.Log("Menu loaded because a player left the match.");
            }
        }

        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
            Debug.Log("joined lobby");
        }

        public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
        {
            base.OnPhotonPlayerConnected(newPlayer);


        }

        public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
        {
            //TODO needs changes! it just goes to menu whenever ANYONE disconnects
            base.OnPhotonPlayerDisconnected(otherPlayer);

            NetworkEvent.PhotonPlayerDisconnected(otherPlayer);

            if (PhotonNetwork.inRoom && PhotonNetwork.room.MaxPlayers == 2 && SceneManager.GetActiveScene().name != "Menu")
            {
                //instead, should set health to 0?
                PhotonNetwork.LoadLevel("Menu");
                PhotonNetwork.LeaveRoom();
                Debug.Log("Menu loaded because a player left the match.");
            } else
            {
                PhotonNetwork.LoadLevel("Menu");
                Debug.Log("Menu loaded because reasons.");
            }
        }

        public override void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedprops)
        {
            Debug.Log("OnPhotonPlayerPropertiesChanged()");
            PhotonPlayer player = playerAndUpdatedprops[0] as PhotonPlayer;
            Hashtable props = playerAndUpdatedprops[1] as Hashtable;
            Hashtable statUpdate = new Hashtable();

            Debug.Log("Player (" + player.ID + "), props counts (" + props.Count + ")");
            if (props.ContainsKey(PlayerProperty.UserId) && props[PlayerProperty.UserId] != null)
            {
                string userId = (string) props[PlayerProperty.UserId];
                if (!string.IsNullOrEmpty(userId)) {
                    statUpdate.Add(PlayerProperty.UserId, userId);
                }
            }
            if (props.ContainsKey(PlayerProperty.AvatarBorderSyscode) && props[PlayerProperty.AvatarBorderSyscode] != null)
            {
                string avatarBorderSyscode = (string)props[PlayerProperty.AvatarBorderSyscode];
                if (!string.IsNullOrEmpty(avatarBorderSyscode))
                {
                    statUpdate.Add(PlayerProperty.AvatarBorderSyscode, avatarBorderSyscode);
                }
            }
            if (props.ContainsKey(PlayerProperty.ProfileImageUrl) && props[PlayerProperty.ProfileImageUrl] != null)
            {
                string profileImageUrl = (string)props[PlayerProperty.ProfileImageUrl];
                if (!string.IsNullOrEmpty(profileImageUrl))
                {
                    statUpdate.Add(PlayerProperty.ProfileImageUrl, profileImageUrl);
                }
            }
            if (props.ContainsKey(PlayerProperty.XpLevel) && props[PlayerProperty.XpLevel] != null)
            {
                int xpLevel = (int)props[PlayerProperty.XpLevel];
                statUpdate.Add(PlayerProperty.XpLevel, xpLevel);
            }
            if (props.ContainsKey(PlayerProperty.State) && props[PlayerProperty.State] != null)
            {
                Debug.Log("props (" + props[PlayerProperty.State] + ")" );
                PlayerState playerState = (PlayerState) props[PlayerProperty.State];
                statUpdate.Add(PlayerProperty.State, playerState);
            }
            if (props.ContainsKey(PlayerProperty.BlueSkillLevel) && props[PlayerProperty.BlueSkillLevel] != null)
            {
                int blueSkillLevel = (int)props[PlayerProperty.BlueSkillLevel];
                statUpdate.Add(PlayerProperty.BlueSkillLevel, blueSkillLevel);
            }
            if (props.ContainsKey(PlayerProperty.GreenSkillLevel) && props[PlayerProperty.GreenSkillLevel] != null)
            {
                int greenSkillLevel = (int)props[PlayerProperty.GreenSkillLevel];
                statUpdate.Add(PlayerProperty.GreenSkillLevel, greenSkillLevel);
            }
            if (props.ContainsKey(PlayerProperty.RedSkillLevel) && props[PlayerProperty.RedSkillLevel] != null)
            {
                int redSkillLevel = (int)props[PlayerProperty.RedSkillLevel];
                statUpdate.Add(PlayerProperty.RedSkillLevel, redSkillLevel);
            }
            if (props.ContainsKey(PlayerProperty.YellowSkillLevel) && props[PlayerProperty.YellowSkillLevel] != null)
            {
                int yellowSkillLevel = (int)props[PlayerProperty.YellowSkillLevel];
                statUpdate.Add(PlayerProperty.YellowSkillLevel, yellowSkillLevel);
            }

            if (statUpdate.Count > 0)
            {
                PlayerEvent.PlayerStatsUpdate(player.ID, statUpdate);
            }
        }

            public void Rematch ()
        {
            Debug.Log("Trying to get a rematch.");
            GameController.photonView.RPC("RPC_SendRematchRequest", PhotonTargets.All);
        }

        public void SetTournamentOpponent (int myJoinNumber)
        {

            if (myJoinNumber == 1 || myJoinNumber == 2)
            {
                gameID_requested = 0;
            }
            else if (myJoinNumber == 3 || myJoinNumber == 4)
            {
                gameID_requested = 1;
            }
            else if (myJoinNumber == 5 || myJoinNumber == 6)
            {
                gameID_requested = 2;
            }
            else if (myJoinNumber == 7 || myJoinNumber == 8)
            {
                gameID_requested = 3;
            }

            tournamentMode = true;
        }
    }
}
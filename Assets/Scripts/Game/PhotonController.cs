using UnityEngine;
using Photon;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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
                //TODO needs looking into but works
                if (PhotonNetwork.room.MaxPlayers == 2)
                    PhotonNetwork.LoadLevel("NormalGame");
                else
                    PhotonNetwork.LoadLevel("TournamentGame");

                Debug.Log("Loading match-making. " + PhotonNetwork.room.MaxPlayers);
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
            // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
            if (PhotonNetwork.connected)
            {
                // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnPhotonRandomJoinFailed() and we'll create one.
                if (PhotonNetwork.insideLobby)
                {
					latestStageId = se.GetStageId ();
					ExitGames.Client.Photon.Hashtable expectedRoomProps = new Hashtable { { RoomProperty.StageId, se.GetStageId() } };
                    if (PhotonNetwork.lobby == _normalLobby)
						PhotonNetwork.JoinRandomRoom(expectedRoomProps, 2); //2 player match
                    else if (PhotonNetwork.lobby == _tournamentLobby)
						PhotonNetwork.JoinRandomRoom(expectedRoomProps, 4); //tournament match
                }
            }
        }

        public void ConnectNormalLobby ()
        {
            PhotonNetwork.JoinLobby(_normalLobby);
            PhotonNetwork.playerName = MainController.Instance.playerData.profileName;
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

            NetworkEvent.ConnectedToPhoton();
        }

        public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
        {
            Debug.Log("OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 2}, null);");

            // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
			if (PhotonNetwork.insideLobby && !string.IsNullOrEmpty(latestStageId))
            {
				TypedLobby typedLobby = PhotonNetwork.lobby == _normalLobby ? _normalLobby : _tournamentLobby;

				RoomOptions roomOptions = new RoomOptions () { MaxPlayers = PhotonNetwork.lobby == _normalLobby ? 2 : 8 };
				roomOptions.CustomRoomProperties = new Hashtable () { { RoomProperty.StageId, latestStageId } };

				PhotonNetwork.CreateRoom(null, roomOptions, typedLobby);
            }
        }

        public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
        {
            base.OnPhotonJoinRoomFailed(codeAndMsg);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Room joined. Name: " + PhotonNetwork.room.Name);

            CreatePlayers();

        }

        public void CreatePlayers ()
        {
            Debug.Log("Destroying previous players...");
            CreatePlayer();
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();
            Debug.Log("Left room.");
            PhotonNetwork.LoadLevel("Menu"); 
            Debug.Log("Menu loaded because room was left.");

            foreach (Player player in FindObjectsOfType<Player>()) //TODO maybe not needed / move to when leaving room
                Destroy(player.gameObject);
        }

        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
        }

        public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
        {
            //TODO needs changes! it just goes to menu whenever ANYONE disconnects
            base.OnPhotonPlayerDisconnected(otherPlayer);
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

        public void CreatePlayer ()
        {
            Debug.Log("Creating player at " + PhotonNetwork.room.PlayerCount + " PlayerCount");

			Player playerGO = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, 0).GetComponent<Player>();

            playerGO.joinNumber = PhotonNetwork.room.PlayerCount;
			playerGO.localID = PhotonNetwork.isMasterClient || playerGO.joinNumber == 3 ? 0 : 1;

            playerGO.UpdateLabels();
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
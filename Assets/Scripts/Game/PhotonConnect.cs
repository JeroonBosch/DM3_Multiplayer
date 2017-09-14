using UnityEngine;
using Photon;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Com.Hypester.DM3
{
    public class PhotonConnect : PunBehaviour
    {
        //public string profileName; //TODO 
        private bool _connect;

        private TypedLobby _normalLobby;
        private TypedLobby _tournamentLobby;

        private List<byte> inactiveGroups, activeGroups;

        private static PhotonConnect instance;
        public static PhotonConnect Instance
        {
            get { return instance ?? (instance = new GameObject("PhotonConnect").AddComponent<PhotonConnect>()); }
        }
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            _normalLobby = new TypedLobby() { Name = "NormalGame", Type = LobbyType.Default };
            _tournamentLobby = new TypedLobby() { Name = "Tournament", Type = LobbyType.Default };

            inactiveGroups = new List<byte>(0);
            activeGroups = new List<byte>(0);
        }

        private void Update()
        {
            if (PhotonNetwork.inRoom && SceneManager.GetActiveScene().name == "Menu")
            {
                if (PhotonNetwork.room.MaxPlayers == 2)
                    PhotonNetwork.LoadLevel("NormalGame");
                else
                    PhotonNetwork.LoadLevel("TournamentGame");
            }
                    

            if (_connect && !PhotonNetwork.connecting && !PhotonNetwork.connected)
                PhotonNetwork.ConnectUsingSettings("v0.2");
        }

        public void EnsureConnection ()
        {
            _connect = true;
        }

        public void MatchPlayers()
        {
            // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
            if (PhotonNetwork.connected)
            {
                // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnPhotonRandomJoinFailed() and we'll create one.

                if (PhotonNetwork.insideLobby)
                {
                    if (PhotonNetwork.lobby == _normalLobby)
                        PhotonNetwork.JoinRandomRoom(null, 2);
                    else
                        PhotonNetwork.JoinRandomRoom(null, 4);
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

        public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
        {
            Debug.Log("OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 2}, null);");

            // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
            if (PhotonNetwork.insideLobby)
            {
                if (PhotonNetwork.lobby == _normalLobby)
                    PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 2 }, _normalLobby);
                else
                    PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 4 }, _tournamentLobby);
            }
            
        }

        public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
        {
            base.OnPhotonJoinRoomFailed(codeAndMsg);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("OnJoinedRoom() called by PUN. Now this client is in a room.");
            foreach (Player player in FindObjectsOfType<Player>())
                Destroy(player.gameObject);
            CreatePlayer();
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();
            Debug.Log("Left room.");
            PhotonNetwork.LoadLevel("Menu");
        }

        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
        }

        public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
        {
            base.OnPhotonPlayerDisconnected(otherPlayer);
            if (PhotonNetwork.inRoom && SceneManager.GetActiveScene().name != "Menu")
            {
                PhotonNetwork.LoadLevel("Menu");
                PhotonNetwork.LeaveRoom();
            } else
            {
                PhotonNetwork.LoadLevel("Menu");
            }
        }

        public void CreatePlayer ()
        {
            Debug.Log("Creating player at " + PhotonNetwork.room.PlayerCount + " PlayerCount");

            GameObject playerGO;
            if (PhotonNetwork.room.MaxPlayers > 3) //Tournament mode.
            {
                activeGroups.Add(1);
                activeGroups.Add(2);
                //activeGroups.Add(3); //Once we go from 4 to 8 player tournaments.
                //activeGroups.Add(4); //Once we go from 4 to 8 player tournaments.

                SetTournamentScreen();

                if (PhotonNetwork.room.PlayerCount >= 3)
                    playerGO = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, activeGroups[1]);
                else
                    playerGO = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, activeGroups[0]);
            } else //1v1 match
                playerGO = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, 0);

            playerGO.GetComponent<Player>().joinNumber = PhotonNetwork.room.PlayerCount;

            if (PhotonNetwork.isMasterClient)
                playerGO.GetComponent<Player>().localID = 0;
            else
                playerGO.GetComponent<Player>().localID = 1;

            playerGO.GetComponent<Player>().UpdateLabels();
        }

        public void Rematch ()
        {
            Debug.Log("Trying to get a rematch.");
            GameObject.Find("Grid").GetComponent<GameHandler>().photonView.RPC("RPC_SendRematchRequest", PhotonTargets.All);
        }

        public void SetTournamentScreen()
        {
            PhotonNetwork.SetInterestGroups(null, activeGroups.ToArray());
            PhotonNetwork.SetSendingEnabled(null, activeGroups.ToArray());
        }

        public void SetTournamentOpponent (int opponentNumber)
        {
            PhotonNetwork.SetInterestGroups(inactiveGroups.ToArray(), activeGroups.ToArray());
            PhotonNetwork.SetSendingEnabled(inactiveGroups.ToArray(), activeGroups.ToArray());
        }
    }
}
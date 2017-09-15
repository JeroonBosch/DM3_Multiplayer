using UnityEngine;
using Photon;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Com.Hypester.DM3
{
    public class PhotonConnect : PunBehaviour
    {
        private bool _connect;

        private TypedLobby _normalLobby;
        private TypedLobby _tournamentLobby;

        public bool tournamentMode;

        public List<byte> allGroups, inactiveGroups, activeGroups;

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

            allGroups = new List<byte>(0);
            inactiveGroups = new List<byte>(0);
            activeGroups = new List<byte>(0);

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

                Debug.Log("Loading match-making. " + PhotonNetwork.room.MaxPlayers);
            }
            /*if (PhotonNetwork.inRoom && SceneManager.GetActiveScene().name == "TournamentGame")
            {
                if (PhotonNetwork.room.MaxPlayers == 2)
                    PhotonNetwork.LoadLevel("Match");
            }*/
                    

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
            Debug.Log("Room joined. Name: " + PhotonNetwork.room.Name);

            allGroups.Clear();
            //activeGroups.Add(0);
            allGroups.Add(1);
            allGroups.Add(2);

            SetAllGroupsActive();

            CreatePlayers();
            //activeGroups.Add(3); //Once we go from 4 to 8 player tournaments.
            //activeGroups.Add(4); //Once we go from 4 to 8 player tournaments.

        }

        public void CreatePlayers ()
        {
            Debug.Log("Destroying previous players...");
            foreach (Player player in FindObjectsOfType<Player>())
                Destroy(player.gameObject);
            CreatePlayer();
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();
            Debug.Log("Left room.");
            PhotonNetwork.LoadLevel("Menu");
            Debug.Log("Menu loaded because room was left.");
        }

        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
        }

        public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
        {
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

            GameObject playerGO;
            if (tournamentMode) //Tournament mode.
            {
                playerGO = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, activeGroups[0]);
            } else //1v1 match
                playerGO = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, 0);

            playerGO.GetComponent<Player>().joinNumber = PhotonNetwork.room.PlayerCount;

            if (PhotonNetwork.isMasterClient || playerGO.GetComponent<Player>().joinNumber == 3) //okay
                playerGO.GetComponent<Player>().localID = 0;
            else
                playerGO.GetComponent<Player>().localID = 1;

            playerGO.GetComponent<Player>().UpdateLabels();
        }

        public void Rematch ()
        {
            Debug.Log("Trying to get a rematch.");
            GameObject.FindWithTag("GameController").GetComponent<GameHandler>().photonView.RPC("RPC_SendRematchRequest", PhotonTargets.All);
        }

        public void SetAllGroupsActive()
        {
            activeGroups.Clear();
            activeGroups.AddRange(allGroups);

            PhotonNetwork.SetInterestGroups(null, activeGroups.ToArray());
            PhotonNetwork.SetSendingEnabled(null, activeGroups.ToArray());
        }

        public void SetTournamentOpponent (int myJoinNumber)
        {
            inactiveGroups.Clear();
            activeGroups.Clear();
            activeGroups.AddRange(allGroups);

            if (myJoinNumber == 1 || myJoinNumber == 2)
            {
                inactiveGroups.Add(activeGroups[1]);
                activeGroups.RemoveAt(1);
            }
            else if (myJoinNumber == 3 || myJoinNumber == 4)
            {
                inactiveGroups.Add(activeGroups[0]);
                activeGroups.RemoveAt(0);
            }

            foreach (Player player in FindObjectsOfType<Player>())
            {
                player.photonView.group = activeGroups[0];
            }

            PhotonNetwork.SetInterestGroups(inactiveGroups.ToArray(), activeGroups.ToArray());
            PhotonNetwork.SetSendingEnabled(inactiveGroups.ToArray(), activeGroups.ToArray());

            tournamentMode = true;
        }
    }
}
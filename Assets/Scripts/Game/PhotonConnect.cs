﻿using UnityEngine;
using Photon;
using UnityEngine.SceneManagement;

namespace Com.Hypester.DM3
{
    public class PhotonConnect : PunBehaviour
    {
        public string profileName; //TODO 
        private bool _connect;

        private static PhotonConnect instance;
        public static PhotonConnect Instance
        {
            get { return instance ?? (instance = new GameObject("PhotonConnect").AddComponent<PhotonConnect>()); }
        }
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (PhotonNetwork.inRoom && SceneManager.GetActiveScene().name != "NormalGame")
                if (PhotonNetwork.room.PlayerCount == 2)
                    PhotonNetwork.LoadLevel("NormalGame");

            if (_connect && !PhotonNetwork.connecting && !PhotonNetwork.connected)
                PhotonNetwork.ConnectUsingSettings("v0.1");
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
                PhotonNetwork.JoinRandomRoom();
            }
        }

        public void ConnectNormalGameroom ()
        {
            PhotonNetwork.JoinLobby(new TypedLobby() { Name = "NormalGame", Type = LobbyType.Default });
            PhotonNetwork.playerName = profileName;
        }

        public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
        {
            Debug.Log("OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 2}, null);");

            // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
            PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 2 }, null);
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

        public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
        {
            base.OnPhotonPlayerDisconnected(otherPlayer);
            if (PhotonNetwork.inRoom && SceneManager.GetActiveScene().name == "NormalGame")
            {
                PhotonNetwork.LoadLevel("Menu");
                PhotonNetwork.LeaveRoom();
            }
        }

        public void CreatePlayer ()
        {
            GameObject playerGO = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, 0);

            if (PhotonNetwork.isMasterClient)
                playerGO.GetComponent<Player>().localID = 0;
            else
                playerGO.GetComponent<Player>().localID = 1;
        }
    }
}
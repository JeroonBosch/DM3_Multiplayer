using UnityEngine;
using Photon;

namespace Com.Hypester.DM3
{
    public class PhotonConnect : PunBehaviour
    {
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
            if (PhotonNetwork.inRoom)
                if (PhotonNetwork.room.PlayerCount >= 2)
                    PhotonNetwork.LoadLevel("NormalGame");
        }

        public void Connect()
        {
            // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
            if (PhotonNetwork.connected)
            {
                // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnPhotonRandomJoinFailed() and we'll create one.
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                // #Critical, we must first and foremost connect to Photon Online Server.
                PhotonNetwork.ConnectUsingSettings("v0.1");
                MainController.Instance.currentScreen.GoToScreen(GameObject.Find("MainmenuScreen").GetComponent<BaseMenuCanvas>());
            }
        }

        public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
        {
            Debug.Log("OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 2}, null);");

            // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
            PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 2 }, null);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("OnJoinedRoom() called by PUN. Now this client is in a room.");
        }
    }
}
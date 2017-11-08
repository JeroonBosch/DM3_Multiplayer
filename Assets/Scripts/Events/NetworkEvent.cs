using UnityEngine;

namespace Com.Hypester.DM3
{
    public class NetworkEvent
    {
        public delegate void PhotonConnectFailAction(DisconnectCause cause);
        public delegate void PhotonConnectSuccessAction();
        public delegate void PhotonPlayerDisconnectedAction(PhotonPlayer otherPlayer);

        public static event PhotonConnectFailAction OnFailedToConnectToPhoton;
        public static event PhotonConnectSuccessAction OnConnectedToPhoton;
        public static event PhotonPlayerDisconnectedAction OnPhotonPlayerDisconnected;

        public static void FailedToConnectToPhoton(DisconnectCause cause)
        {
            if (OnFailedToConnectToPhoton != null) OnFailedToConnectToPhoton(cause);
        }
        public static void ConnectedToPhoton()
        {
            if (OnConnectedToPhoton != null) OnConnectedToPhoton();
        }
        public static void PhotonPlayerDisconnected(PhotonPlayer otherPlayer)
        {
            if (OnPhotonPlayerDisconnected != null) OnPhotonPlayerDisconnected(otherPlayer);
        }
    }
}
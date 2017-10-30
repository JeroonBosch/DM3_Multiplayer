using UnityEngine;

namespace Com.Hypester.DM3
{
    public class NetworkEvent
    {
        public delegate void PhotonConnectFailAction(DisconnectCause cause);
        public delegate void PhotonConnectSuccessAction();

        public static event PhotonConnectFailAction OnFailedToConnectToPhoton;
        public static event PhotonConnectSuccessAction OnConnectedToPhoton;

        public static void FailedToConnectToPhoton(DisconnectCause cause)
        {
            if (OnFailedToConnectToPhoton != null) OnFailedToConnectToPhoton(cause);
        }
        public static void ConnectedToPhoton()
        {
            if (OnConnectedToPhoton != null) OnConnectedToPhoton();
        }
    }
}
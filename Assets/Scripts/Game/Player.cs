using Photon;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class Player : Photon.MonoBehaviour
    {
        public int localID;

        #region private variables
        int profileID;
        int coins;
        string profileName;
        string portraitURL;
        #endregion

        // Use this for initialization
        void Start()
        {
            if (gameObject.GetComponent<PhotonView>().isMine)
                transform.Find("FingerTracker").GetComponent<Image>().enabled = false;
        }

        void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                stream.SendNext(profileName);
                stream.SendNext(localID);
            }
            else
            {
                profileName = (string)stream.ReceiveNext();
                localID = (int)stream.ReceiveNext();
            }
        }

        #region SelectionRPCs
        public void NewSelection(Vector2 pos)
        {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("RPCAddToSelection", PhotonTargets.All, pos);
        }

        [PunRPC]
        void RPCAddToSelection(Vector2 pos)
        {
            GameObject.Find("Grid").GetComponent<GameHandler>().AddToSelection(pos);
        }

        public void RemoveSelection(Vector2 pos)
        {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("RPCRemoveFromSelection", PhotonTargets.All, pos);
        }

        [PunRPC]
        void RPCRemoveFromSelection(Vector2 pos)
        {
            GameObject.Find("Grid").GetComponent<GameHandler>().RemoveFromSelection(pos);
        }

        public void RemoveAllSelections()
        {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("RPCRemoveAllSelections", PhotonTargets.All);
        }

        [PunRPC]
        void RPCRemoveAllSelections()
        {
            GameObject.Find("Grid").GetComponent<GameHandler>().RemoveSelections();
        }
        #endregion
    }
}
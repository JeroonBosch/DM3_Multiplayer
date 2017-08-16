using Photon;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class Player : Photon.MonoBehaviour
    {
        public int localID;
        public string profileName;
        public bool wantsRematch;

        #region private variables
        int profileID;
        string portraitURL;

        private GameObject _game;
        #endregion

        // Use this for initialization
        void Start()
        {
            DontDestroyOnLoad(gameObject);
            GetComponent<Canvas>().worldCamera = Camera.main;

            if (gameObject.GetComponent<PhotonView>().isMine) {
                profileName = PhotonNetwork.playerName;
                transform.Find("FingerTracker").GetComponent<Image>().enabled = false;
            }

            if (localID == 0) {
                foreach (GameObject player1name in GameObject.FindGameObjectsWithTag("Player_1_Name"))
                {
                    player1name.GetComponent<Text>().text = profileName;
                }
            } else
            {
                foreach (GameObject player2name in GameObject.FindGameObjectsWithTag("Player_2_Name"))
                {
                    player2name.GetComponent<Text>().text = profileName;
                }
            }

            wantsRematch = false;
        }

        public void Reset()
        {
            Start();
            photonView.RPC("SendName", PhotonTargets.All, profileName);
        }

        private void Update()
        {
            if (_game == null)
                _game = GameObject.Find("Grid");

            if (_game && _game.GetComponent<GameHandler>().MyPlayer != null)
            {
                if (_game.GetComponent<GameHandler>().IsMyTurn())
                    transform.Find("FingerTracker").GetComponent<Image>().enabled = false;
                else if (!gameObject.GetComponent<PhotonView>().isMine && _game.GetComponent<GameHandler>().Active)
                    transform.Find("FingerTracker").GetComponent<Image>().enabled = true;
            }

            GetComponent<Canvas>().worldCamera = Camera.main;
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

            if (localID == 0)
            {
                foreach (GameObject player1name in GameObject.FindGameObjectsWithTag("Player_1_Name"))
                {
                    player1name.GetComponent<Text>().text = profileName;
                }
            }
            else
            {
                foreach (GameObject player2name in GameObject.FindGameObjectsWithTag("Player_2_Name"))
                {
                    player2name.GetComponent<Text>().text = profileName;
                }
            }
        }

        public string GetName ()
        {
            return profileName;
        }

        #region SelectionRPCs
        public void NewSelection(Vector2 pos)
        {
            photonView.RPC("RPCAddToSelection", PhotonTargets.All, pos);
        }
        [PunRPC]
        void RPCAddToSelection(Vector2 pos)
        {
            _game.GetComponent<GameHandler>().AddToSelection(pos);
        }


        public void RemoveSelection(Vector2 pos)
        {
            photonView.RPC("RPCRemoveFromSelection", PhotonTargets.All, pos);
        }
        [PunRPC]
        void RPCRemoveFromSelection(Vector2 pos)
        {
            _game.GetComponent<GameHandler>().RemoveFromSelection(pos);
        }


        public void RemoveAllSelections()
        {
            photonView.RPC("RPCRemoveAllSelections", PhotonTargets.All);
        }
        [PunRPC]
        void RPCRemoveAllSelections()
        {
            _game.GetComponent<GameHandler>().RemoveSelections();
        }
        #endregion

        public void InitiateCombo()
        {
            photonView.RPC("RPCInitiateCombo", PhotonTargets.All);
        }
        [PunRPC]
        void RPCInitiateCombo()
        {
            _game.GetComponent<GameHandler>().InitiateCombo();
        }

        [PunRPC]
        public void RequestRematch ()
        {
            wantsRematch = true;
        }

        [PunRPC]
        public void SendName(string profName)
        {
            profileName = profName;

            if (localID == 0)
            {
                foreach (GameObject player1name in GameObject.FindGameObjectsWithTag("Player_1_Name"))
                {
                    player1name.GetComponent<Text>().text = profileName;
                }
            }
            else
            {
                foreach (GameObject player2name in GameObject.FindGameObjectsWithTag("Player_2_Name"))
                {
                    player2name.GetComponent<Text>().text = profileName;
                }
            }
        }

        public void PowerClicked (int color)
        {
            if (PhotonNetwork.isMasterClient)
                _game.GetComponent<GameHandler>().PowerClicked(color);
            else
                photonView.RPC("RPC_PowerClick", PhotonTargets.Others, color);
        }
        [PunRPC]
        void RPC_PowerClick(int color)
        {
            _game.GetComponent<GameHandler>().PowerClicked(color);
        }
    }
}
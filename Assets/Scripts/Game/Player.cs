using Photon;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class Player : Photon.MonoBehaviour
    {
        public int localID;
        public int joinNumber;
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

            wantsRematch = false;

            UpdateLabels();
        }

        public void UpdateLabels ()
        {
            if (joinNumber == 1)
            {
                foreach (GameObject player1name in GameObject.FindGameObjectsWithTag("Player_1_Name"))
                {
                    player1name.GetComponent<Text>().text = profileName;
                }
            }
            else if (joinNumber == 2)
            {
                foreach (GameObject player2name in GameObject.FindGameObjectsWithTag("Player_2_Name"))
                {
                    player2name.GetComponent<Text>().text = profileName;
                }
            }
            else if (joinNumber == 3)
            {
                foreach (GameObject player3name in GameObject.FindGameObjectsWithTag("Player_3_Name"))
                {
                    player3name.GetComponent<Text>().text = profileName;
                }
            }
            else
            {
                foreach (GameObject player4name in GameObject.FindGameObjectsWithTag("Player_4_Name"))
                {
                    player4name.GetComponent<Text>().text = profileName;
                }
            }

            if (photonView.isMine)
            {
                foreach (GameObject myName in GameObject.FindGameObjectsWithTag("MyPlayer_Name"))
                {
                    myName.GetComponent<Text>().text = profileName;
                }
            }
            else
            {
                foreach (GameObject myName in GameObject.FindGameObjectsWithTag("OpponentPlayer_Name"))
                {
                    myName.GetComponent<Text>().text = profileName;
                }
            }
        }

        public PlayerInterface FindInterface ()
        {
            PlayerInterface playerInterface = null;

            foreach (PlayerInterface tryInterface in FindObjectsOfType<PlayerInterface>())
            {
                if ((photonView.isMine && tryInterface.playerNumber == 0) || (!photonView.isMine && tryInterface.playerNumber == 1))
                    return tryInterface;
            }

            return playerInterface;
        }

        public float GetHealth ()
        {
            if (localID == 0)
                return _game.GetComponent<GameHandler>().healthPlayerOne;
            return _game.GetComponent<GameHandler>().healthPlayerTwo;
        }

        public void Reset()
        {
            Start();
            photonView.RPC("RPC_SendName", PhotonTargets.All, profileName);
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
                stream.SendNext(joinNumber);
            }
            else
            {
                profileName = (string)stream.ReceiveNext();
                localID = (int)stream.ReceiveNext();
                joinNumber = (int)stream.ReceiveNext();
            }

            UpdateLabels();
        }

        public string GetName ()
        {
            return profileName;
        }

        #region SelectionRPCs
        public void NewSelection(Vector2 pos)
        {
            iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.SelectionChange);

            photonView.RPC("RPC_AddToSelection", PhotonTargets.All, pos);
        }
        [PunRPC]
        void RPC_AddToSelection(Vector2 pos)
        {
            _game.GetComponent<GameHandler>().AddToSelection(pos);
        }


        public void RemoveSelection(Vector2 pos)
        {
            photonView.RPC("RPC_RemoveFromSelection", PhotonTargets.All, pos);
        }
        [PunRPC]
        void RPC_RemoveFromSelection(Vector2 pos)
        {
            _game.GetComponent<GameHandler>().RemoveFromSelection(pos);
        }


        public void RemoveAllSelections()
        {
            photonView.RPC("RPC_RemoveAllSelections", PhotonTargets.All);
        }
        [PunRPC]
        void RPC_RemoveAllSelections()
        {
            _game.GetComponent<GameHandler>().RemoveSelections();
        }
        #endregion

        public void InitiateCombo()
        {
            iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.ImpactLight);

            photonView.RPC("RPC_InitiateCombo", PhotonTargets.All);
        }
        [PunRPC]
        void RPC_InitiateCombo()
        {
            _game.GetComponent<GameHandler>().InitiateCombo();
        }

        [PunRPC]
        public void RPC_RequestRematch()
        {
            wantsRematch = true;
        }

        [PunRPC]
        public void RPC_SendName(string profName)
        {
            profileName = profName;

            UpdateLabels();
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
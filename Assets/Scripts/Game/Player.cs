﻿using Photon;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class Player : Photon.MonoBehaviour
    {
        public int localID;
        public bool wantsRematch;

        #region private variables
        int profileID;
        string profileName;
        string portraitURL;

        private GameObject _game;
        #endregion

        // Use this for initialization
        void Start()
        {
            DontDestroyOnLoad(gameObject);
            GetComponent<Canvas>().worldCamera = Camera.current;

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

        private void Update()
        {
            if (_game == null)
                _game = GameObject.Find("Grid");

            if (_game && _game.GetComponent<GameHandler>().MyPlayer != null)
            {
                if (_game.GetComponent<GameHandler>().IsMyTurn())
                    transform.Find("FingerTracker").GetComponent<Image>().enabled = false;
                else if (!gameObject.GetComponent<PhotonView>().isMine)
                    transform.Find("FingerTracker").GetComponent<Image>().enabled = true;
            }
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
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("RPCAddToSelection", PhotonTargets.All, pos);
        }
        [PunRPC]
        void RPCAddToSelection(Vector2 pos)
        {
            _game.GetComponent<GameHandler>().AddToSelection(pos);
        }


        public void RemoveSelection(Vector2 pos)
        {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("RPCRemoveFromSelection", PhotonTargets.All, pos);
        }
        [PunRPC]
        void RPCRemoveFromSelection(Vector2 pos)
        {
            _game.GetComponent<GameHandler>().RemoveFromSelection(pos);
        }


        public void RemoveAllSelections()
        {
            PhotonView photonView = PhotonView.Get(this);
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
            PhotonView photonView = PhotonView.Get(this);
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
    }
}
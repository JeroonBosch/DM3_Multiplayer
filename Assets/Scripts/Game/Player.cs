﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Com.Hypester.DM3
{
    //Each player in a room creates his/her own 'Player'
    //Class name could be changed to represent it being a photon view with client-control rather than master-client control
    public class Player : Photon.MonoBehaviour
    {
        private int playerId;

        public int localID;
        public int joinNumber; //only for tournaments
        public string profileName;
        public string profilePicURL;
        public bool wantsRematch;

        public PlayerInterface playerInterface;
        public Player opponent;

        [SerializeField] Canvas playerCanvas;
        [SerializeField] Image fingerTrackerImage;

        // Use this for initialization
        void Start()
        {
            DontDestroyOnLoad(gameObject);
            playerCanvas.worldCamera = Camera.main;

            if (photonView.isMine) {
                profileName = MainController.Instance.playerData.profileName;
                profilePicURL = MainController.Instance.playerData.pictureURL;
                ToggleFingerTracker(false);
            }

            wantsRematch = false;

            UpdateLabels();
        }

        private void Update()
        {
            if (PhotonController.Instance.GameController == null || !PhotonController.Instance.GameController.gameObject.GetActive())
                return;
            else if (PhotonController.Instance.GameController && PhotonController.Instance.GameController.MyPlayer != null)
            {
                if (PhotonController.Instance.GameController.IsMyTurn()) { ToggleFingerTracker(false); }
                else if (!photonView.isMine && PhotonController.Instance.GameController.Active) { ToggleFingerTracker(true); }
            }
            playerCanvas.worldCamera = Camera.main;
        }

        void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            transform.position = Vector3.zero;
            SetPlayerId(info.sender.ID);
            PlayerManager.instance.AddPlayer(playerId, this);
        }

        void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                stream.SendNext(profileName);
                stream.SendNext(profilePicURL);
                stream.SendNext(localID);
                stream.SendNext(joinNumber);
            }
            else
            {
                profileName = (string)stream.ReceiveNext();
                profilePicURL = (string)stream.ReceiveNext();
                localID = (int)stream.ReceiveNext();
                joinNumber = (int)stream.ReceiveNext();
            }

            UpdateLabels();
        }

        public int GetRequestedGameID ()
        {
            if (joinNumber == 1 || joinNumber == 2)
                return 0;
            else if (joinNumber == 3 || joinNumber == 4)
                return 1;
            return 0;
        }

        public bool IsRelevantGame(int id)
        {
            if (GetRequestedGameID() == id)
                return true;
            else
                return false;
        }

        public bool IsRelevantPlayer ()
        {
            if (!photonView.isMine) {
                Player myPlayer = null;
                foreach (Player player in FindObjectsOfType<Player>())
                {
                    if (player.photonView.isMine)
                    {
                        myPlayer = player;
                    }
                }
                if (myPlayer != null)
                {
                    if (myPlayer.opponent = this)
                        return true;
                }
            } else
            {
                return true;
            }
            return false;
        }

        public void UpdateLabels()
        {
            if (joinNumber == 1)
                SetTextToProfileName("Player_1_Name");
            else if (joinNumber == 2)
                SetTextToProfileName("Player_2_Name");
            else if (joinNumber == 3)
                SetTextToProfileName("Player_3_Name");
            else
                SetTextToProfileName("Player_4_Name");

            if (photonView.isMine)
                SetTextToProfileName("MyPlayer_Name");
            else
                if (IsRelevantPlayer())
                    SetTextToProfileName("OpponentPlayer_Name");


            if (profilePicURL != "") { 
                //Avatars
                if (joinNumber == 1)
                    SetImageToProfilePic("Player_1_Avatar");
                else if (joinNumber == 2)
                    SetImageToProfilePic("Player_2_Avatar");
                else if (joinNumber == 3)
                    SetImageToProfilePic("Player_3_Avatar");
                else
                    SetImageToProfilePic("Player_4_Avatar");

                if (photonView.isMine)
                    SetImageToProfilePic("MyPlayer_Avatar");
                else
                    if (IsRelevantPlayer())
                        SetImageToProfilePic("OpponentPlayer_Avatar");
            }
        }

        private void SetTextToProfileName(string tag)
        {
            foreach (GameObject name in GameObject.FindGameObjectsWithTag(tag))
            {
                name.GetComponent<Text>().text = profileName;
            }
        }

        private void SetImageToProfilePic(string tag)
        {
            foreach (GameObject avatar in GameObject.FindGameObjectsWithTag(tag))
            {
                StartCoroutine(ImageFromURL(avatar.GetComponent<Image>()));
            }
        }

        public void ToggleFingerTracker(bool turnOn)  { fingerTrackerImage.enabled = turnOn; }

        private IEnumerator ImageFromURL (Image img)
        {
            WWW www = new WWW(profilePicURL);
            yield return www;
            img.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));
        }

        public PlayerInterface FindInterface ()
        {
            PlayerInterface playerInterface = null;

            foreach (PlayerInterface tryInterface in FindObjectsOfType<PlayerInterface>())
            {
                if ((photonView.isMine && tryInterface.owner == PlayerInterface.Owner.Local) || (!photonView.isMine && tryInterface.owner == PlayerInterface.Owner.Remote))
                    return tryInterface;
            }

            return playerInterface;
        }

        public float GetHealth ()
        {
            if (localID == 0)
                return PhotonController.Instance.GameController.healthPlayerOne;
            return PhotonController.Instance.GameController.healthPlayerTwo;
        }

        public void Reset()
        {
            Start();
            photonView.RPC("RPC_SendName", PhotonTargets.All, profileName);
        }

        public string GetName ()
        {
            return profileName;
        }

        public int GetPlayerId() { return playerId; }
        public void SetPlayerId(int id) { playerId = id; }

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
    }
}
using Photon;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Com.Hypester.DM3
{
    public class Player : Photon.MonoBehaviour
    {
        public int localID;
        public int joinNumber; //only for tournaments?
        public string profileName;
        public string profilePicURL;
        //public Texture2D profilePic;
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
                profileName = MainController.Instance.playerData.profileName;
                profilePicURL = MainController.Instance.playerData.pictureURL;
                transform.Find("FingerTracker").GetComponent<Image>().enabled = false;
            }

            wantsRematch = false;

            UpdateLabels();
        }

        private void Update()
        {
            if (_game == null || !_game.GetActive())
                _game = GameObject.FindWithTag("GameController");

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
            photonView.RPC("RPC_PowerClick", PhotonTargets.All, color);
        }
        [PunRPC]
        void RPC_PowerClick(int color)
        {
            _game.GetComponent<GameHandler>().PowerClicked(color);
        }
    }
}
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
        public bool wantsRematch;

        public Player opponent;

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
            if (PhotonConnect.Instance.GameController == null || !PhotonConnect.Instance.GameController.gameObject.GetActive())
                return;
            else if (PhotonConnect.Instance.GameController && PhotonConnect.Instance.GameController.MyPlayer != null)
            {
                if (PhotonConnect.Instance.GameController.IsMyTurn())
                    transform.Find("FingerTracker").GetComponent<Image>().enabled = false;
                else if (!gameObject.GetComponent<PhotonView>().isMine && PhotonConnect.Instance.GameController.Active)
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
                return PhotonConnect.Instance.GameController.healthPlayerOne;
            return PhotonConnect.Instance.GameController.healthPlayerTwo;
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
            //This is only done by the selecting player, on his local device.
            iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.SelectionChange);

            photonView.RPC("RPC_AddToSelection", PhotonTargets.All, PhotonConnect.Instance.GameController.GameID, pos);
        }
        [PunRPC]
        void RPC_AddToSelection(int gameID, Vector2 pos)
        {
            if (IsRelevantGame(PhotonConnect.Instance.GameController.GameID))
                PhotonConnect.Instance.GameController.AddToSelection(pos);
        }


        public void RemoveSelection(Vector2 pos)
        {
            photonView.RPC("RPC_RemoveFromSelection", PhotonTargets.All, PhotonConnect.Instance.GameController.GameID, pos);
        }
        [PunRPC]
        void RPC_RemoveFromSelection(int gameID, Vector2 pos)
        {
            if (IsRelevantGame(PhotonConnect.Instance.GameController.GameID))
                PhotonConnect.Instance.GameController.RemoveFromSelection(pos);
        }


        public void RemoveAllSelections()
        {
            photonView.RPC("RPC_RemoveAllSelections", PhotonTargets.All, PhotonConnect.Instance.GameController.GameID);
        }
        [PunRPC]
        void RPC_RemoveAllSelections(int gameID)
        {
            if (IsRelevantGame(PhotonConnect.Instance.GameController.GameID))
                PhotonConnect.Instance.GameController.RemoveSelections();
        }
        #endregion

        public void InitiateCombo()
        {
            iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.ImpactLight);

            photonView.RPC("RPC_InitiateCombo", PhotonTargets.All, PhotonConnect.Instance.GameController.GameID);
        }
        [PunRPC]
        void RPC_InitiateCombo(int gameID)
        {
            if (IsRelevantGame(PhotonConnect.Instance.GameController.GameID))
                PhotonConnect.Instance.GameController.InitiateCombo();
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
            photonView.RPC("RPC_PowerClick", PhotonTargets.All, PhotonConnect.Instance.GameController.GameID, color);
        }
        [PunRPC]
        void RPC_PowerClick(int gameID, int color)
        {
            if (IsRelevantGame(PhotonConnect.Instance.GameController.GameID))
                PhotonConnect.Instance.GameController.PowerClicked(color);
        }
    }
}
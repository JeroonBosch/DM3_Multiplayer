using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace Com.Hypester.DM3
{
    //Each player in a room creates his/her own 'Player'
    //Class name could be changed to represent it being a photon view with client-control rather than master-client control
    public class Player : Photon.MonoBehaviour
    {
        [Header("Identification")]
        private int playerId;
        public int localID;
        public int joinNumber; //only for tournaments

        [Header("Data")]
        public string profileName;
        public string profilePicURL;
        public Sprite profilePicSprite;
        public int currentXpLevel;

        public PlayerInterface playerInterface;
        public Player opponent;

        [SerializeField] Canvas playerCanvas;
        [SerializeField] Image fingerTrackerImage;

        // Use this for initialization
        void Start()
        {
            DontDestroyOnLoad(gameObject);
            playerCanvas.worldCamera = Camera.main;

            if (photonView.isMine && PhotonNetwork.player.ID == GetPlayerId()) {
                profileName = MainController.Instance.playerData.profileName;
                profilePicURL = MainController.Instance.playerData.pictureURL;
                ToggleFingerTracker(false);
            }
        }

        protected void OnEnable()
        {
            PlayerEvent.OnPlayerStatsUpdate += OnPlayerStatsUpdate;
        }
        protected void OnDisable()
        {
            PlayerEvent.OnPlayerStatsUpdate -= OnPlayerStatsUpdate;
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

        private void OnPlayerStatsUpdate(int playerId, ExitGames.Client.Photon.Hashtable stats)
        {
            if (playerId != GetPlayerId()) { return; }

            if (stats.ContainsKey(PlayerProperty.ProfileImageUrl) && !string.IsNullOrEmpty(((string)stats[PlayerProperty.ProfileImageUrl])))
            {
                MainController.ServiceAsset.StartCoroutine(MainController.ServiceAsset.ImageFromURL(playerId, (string)stats[PlayerProperty.ProfileImageUrl], OnLoadPlayerProfileImage));
            }
            if (stats.ContainsKey(PlayerProperty.XpLevel))
            {
                currentXpLevel = (int)stats[PlayerProperty.XpLevel];
            }
        }

        private void OnLoadPlayerProfileImage(Sprite newImage, int playerId)
        {
            if (newImage != null) { profilePicSprite = newImage; }
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

        public bool IsLocal() { return (PhotonNetwork.player.ID == playerId); }

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
        public void RPC_RequestRematch(int playerId)
        {
            PlayerEvent.PlayerWantsRematch(playerId);
        }

        [PunRPC]
        public void RPC_SendName(string profName)
        {
            profileName = profName;
        }
    }
}
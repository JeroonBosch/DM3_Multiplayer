using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class EndScreenCanvas : BaseMenuCanvas
    {
        private Transform _myPlayer;
        private Transform _enemyPlayer;

        private Image _myRematchBubble;
        private Image _enemyRematchBubble;
        private Image _myRematchButton;
        private Image _enemyRematchButton;

        private int _winnerPlayer;
        public int winnerPlayer { set { SetWinnerPlayer(value); } }

        protected override void Start()
        {
            base.Start();
            Hide();

            if (PhotonNetwork.isMasterClient)
            {
                _myPlayer = GameObject.Find("Player1_EndInterface").transform;
                _enemyPlayer = GameObject.Find("Player2_EndInterface").transform;
            }
            else
            {
                _myPlayer = GameObject.Find("Player2_EndInterface").transform;
                _enemyPlayer = GameObject.Find("Player1_EndInterface").transform;
            }

            _myRematchBubble = _myPlayer.Find("RematchBubble").GetComponent<Image>();
            _myRematchButton = _myPlayer.Find("RematchButton").GetComponent<Image>();
            _enemyRematchBubble = _enemyPlayer.Find("RematchBubble").GetComponent<Image>();
            _enemyRematchButton = _enemyPlayer.Find("RematchButton").GetComponent<Image>();

            _myRematchBubble.enabled = false;
            _enemyRematchBubble.enabled = false;
            _myRematchButton.enabled = true;
            _enemyRematchButton.enabled = false;

            Destroy(_enemyPlayer.Find("Reward").gameObject);

            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player"))
            {
                go.transform.Find("FingerTracker").GetComponent<Image>().enabled = false;
            }
        }

        public override void Show()
        {
            base.Show();

            foreach (GameObject nameObject in GameObject.FindGameObjectsWithTag("Player_1_Name"))
            {
                Text nameText = nameObject.GetComponent<Text>();
                if (PhotonNetwork.isMasterClient)
                    nameText.text = PhotonController.Instance.GameController.MyPlayer.profileName;
                else
                    nameText.text = PhotonController.Instance.GameController.EnemyPlayer.profileName;
            }
            foreach (GameObject nameObject in GameObject.FindGameObjectsWithTag("Player_2_Name"))
            {
                Text nameText = nameObject.GetComponent<Text>();
                if (!PhotonNetwork.isMasterClient)
                    nameText.text = PhotonController.Instance.GameController.MyPlayer.profileName;
                else
                    nameText.text = PhotonController.Instance.GameController.EnemyPlayer.profileName;
            }
        }

        private void SetWinnerPlayer (int winnerPlayer)
        {
            _winnerPlayer = winnerPlayer;
            if (_winnerPlayer == 0)
            {
                if (PhotonNetwork.isMasterClient)
                    Destroy(_enemyPlayer.Find("AvatarWinnerBorder").gameObject);
                else
                    Destroy(_myPlayer.Find("AvatarWinnerBorder").gameObject);
            }
            else
            {
                if (PhotonNetwork.isMasterClient)
                    Destroy(_myPlayer.Find("AvatarWinnerBorder").gameObject);
                else
                    Destroy(_enemyPlayer.Find("AvatarWinnerBorder").gameObject);
            }
        }

        protected override void Update()
        {
            base.Update();

            if (PhotonController.Instance.GameController != null && PhotonController.Instance.GameController.EnemyPlayer != null)
            {
                if (PhotonController.Instance.GameController.EnemyPlayer.wantsRematch && _enemyRematchBubble.enabled == false)
                {
                    _enemyRematchBubble.enabled = true;
                }

                if (PhotonController.Instance.GameController.MyPlayer.wantsRematch && _myRematchBubble.enabled == false)
                {
                    _myRematchBubble.enabled = true;
                    _myRematchButton.enabled = false;
                }

                if (PhotonController.Instance.GameController.MyPlayer.wantsRematch && PhotonController.Instance.GameController.EnemyPlayer.wantsRematch && PhotonNetwork.isMasterClient)
                {
                    PhotonController.Instance.Rematch();
                    PhotonController.Instance.GameController.MyPlayer.wantsRematch = false;
                    PhotonController.Instance.GameController.EnemyPlayer.wantsRematch = false;
                }


            }
        }

        public void BackToMenu ()
        {
            if (PhotonNetwork.inRoom)
            {
                PhotonNetwork.LeaveRoom();
                PhotonNetwork.LoadLevel("Menu");
            } else
            {
                PhotonNetwork.LoadLevel("Menu");
            }
        }

        public void RequestRematch()
        {
            PhotonController.Instance.GameController.MyPlayer.photonView.RPC("RPC_RequestRematch", PhotonTargets.Others);
            PhotonController.Instance.GameController.MyPlayer.wantsRematch = true;
        }
    }
}
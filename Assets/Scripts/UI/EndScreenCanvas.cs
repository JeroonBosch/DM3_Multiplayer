using UnityEngine;
using UnityEngine.UI;
using Lean.Touch;
using System.Collections.Generic;
using UnityEngine.EventSystems;

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
        private GameHandler _game;

        public int winnerPlayer;

        protected override void Start()
        {
            base.Start();
            _game = GameObject.Find("Grid").GetComponent<GameHandler>();

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

            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player"))
            {
                go.transform.Find("FingerTracker").GetComponent<Image>().enabled = false;
            }
        }

        public override void Show()
        {
            base.Show();

            if (winnerPlayer == 0)
            {
                if (PhotonNetwork.isMasterClient)
                    _enemyPlayer.Find("AvatarWinnerBorder").GetComponent<Image>().enabled = false ;
                else
                    _myPlayer.Find("AvatarWinnerBorder").GetComponent<Image>().enabled = false;
            } else
            {
                if (PhotonNetwork.isMasterClient)
                    _myPlayer.Find("AvatarWinnerBorder").GetComponent<Image>().enabled = false;
                else
                    _enemyPlayer.Find("AvatarWinnerBorder").GetComponent<Image>().enabled = false;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (_game != null && _game.EnemyPlayer != null)
            {
                if (_game.EnemyPlayer.wantsRematch && _enemyRematchBubble.enabled == false)
                {
                    _enemyRematchBubble.enabled = true;
                }

                if (_game.MyPlayer.wantsRematch && _myRematchBubble.enabled == false)
                {
                    _myRematchBubble.enabled = true;
                    _myRematchButton.enabled = false;
                }

                if (_game.MyPlayer.wantsRematch && _game.EnemyPlayer.wantsRematch && PhotonNetwork.isMasterClient)
                {
                    PhotonConnect.Instance.Rematch();
                    _game.MyPlayer.wantsRematch = false;
                    _game.EnemyPlayer.wantsRematch = false;
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
            _game.MyPlayer.photonView.RPC("RequestRematch", PhotonTargets.Others);
            _game.MyPlayer.wantsRematch = true;
        }
    }
}
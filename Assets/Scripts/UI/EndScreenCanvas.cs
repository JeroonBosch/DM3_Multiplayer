using UnityEngine;
using UnityEngine.UI;
using Lean.Touch;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Com.Hypester.DM3
{
    public class EndScreenCanvas : BaseMenuCanvas
    {
        private Image _rematchBubble;
        private GameHandler _game;

        protected override void Start()
        {
            base.Start();
            _rematchBubble = GameObject.Find("RematchBubble").GetComponent<Image>();
            _game = GameObject.Find("Grid").GetComponent<GameHandler>();

            _rematchBubble.enabled = false;

            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player"))
            {
                go.transform.Find("FingerTracker").GetComponent<Image>().enabled = false;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (_game != null && _game.EnemyPlayer != null)
            {
                if (_game.EnemyPlayer.wantsRematch && _rematchBubble.enabled == false)
                {
                    _rematchBubble.enabled = true;
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
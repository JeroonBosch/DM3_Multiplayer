using UnityEngine;
using System.Collections;

namespace Com.Hypester.DM3
{
    public class TrapPower : Photon.MonoBehaviour
    {
        private GameHandler _game;

        public Player ownerPlayer;
        public Vector2 position;
        public Vector2 mirrorPosition;

        private bool _isPickedUp = false;
        public bool isPickedUp { get { return _isPickedUp; } }

        private BaseTile _overBasetile;
        public BaseTile overBasetile { set { _overBasetile = value; } get { return _overBasetile; } }


        private void Start()
        {
            _game = GameObject.Find("Grid").GetComponent<GameHandler>();

            if (!photonView.isMine)
                ownerPlayer = _game.EnemyPlayer;
        }

        private void Update()
        {
            if (!photonView.isMine)
            {
                transform.localPosition = new Vector2 (mirrorPosition.x, mirrorPosition.y + 2f); //No idea why the +2f is needed, but it is..
            } else
            {
                position = transform.localPosition;
            }
        }

        void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                stream.SendNext(position);
            }
            else
            {
                position = (Vector2)stream.ReceiveNext();
                mirrorPosition = MirrorPosition(position);
            }
        }

        public void PickUp ()
        {
            _isPickedUp = true;
            Debug.Log("Trap picked up.");
        }

        private Vector2 MirrorPosition (Vector2 pos)
        {
            Vector2 mirrored = new Vector2(-pos.x, -pos.y);
            return mirrored;
        }

        public void Place()
        {
            if (_isPickedUp && _overBasetile != null) {
                //Do stuff here.
            } else
            {
                _isPickedUp = false;
            }
        }
    }
}
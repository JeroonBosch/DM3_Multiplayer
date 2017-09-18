using UnityEngine;
using System.Collections;

namespace Com.Hypester.DM3
{
    public class YellowPower : Photon.MonoBehaviour
    {
        public Player ownerPlayer;
        public Vector2 position;
        public Vector2 mirrorPosition;

        private Vector2 _velocity;
        private Vector2 _curPos;
        private Vector2 _lastPos;

        private bool _isFlying = false;
        public bool isFlying { get { return _isFlying; } }

        private bool _isPickedUp = false;
        public bool isPickedUp { get { return _isPickedUp; } }

        private float _speed = 20f;

        private void Start()
        {
            _curPos = transform.position;

            if (!photonView.isMine)
                ownerPlayer = PhotonConnect.Instance.GameController.EnemyPlayer;
        }

        private void Update()
        {
            if (!photonView.isMine)
            {
                if (!_isFlying)
                    transform.localPosition = new Vector2(mirrorPosition.x, mirrorPosition.y + 2f); //No idea why the +2f is needed, but it is..
                else
                {
                    Vector2 mirrorVelocity = new Vector2(-_velocity.x, -_velocity.y);
                    Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
                    rb.velocity = mirrorVelocity * _speed;
                }
            }
            else
            {
                if (!_isFlying)
                {
                    _velocity = new Vector2(_curPos.x - _lastPos.x, _curPos.y - _lastPos.y);
                    _lastPos = _curPos;
                    _curPos = transform.position;
                }
                else
                {
                    if (_velocity.x < .3f && _velocity.y < .3f)
                    {
                        _isFlying = false;
                        GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
                    }
                    else
                    {
                        //position = transform.localPosition;
                    }

                    if (transform.localPosition.x > 5f || transform.localPosition.y > 5f || transform.localPosition.x < -5f || transform.localPosition.y < -5f)
                    {
                        Destroy(gameObject);
                        photonView.RPC("RPC_DestroyFireball", PhotonTargets.Others);
                    }
                }
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

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "OpponentPlayer_Avatar" && photonView.isMine)
            {
                PhotonConnect.Instance.GameController.photonView.RPC("RPC_FireballHit", PhotonTargets.All);
            }
            else
            {
                Debug.Log("hit " + collision.gameObject.name);
            }
        }

        public void Hide ()
        {
            gameObject.SetActive(false);
        }

        public void PickUp()
        {
            _isPickedUp = true;
            Debug.Log("Fireball picked up.");
        }

        private Vector2 MirrorPosition(Vector2 pos)
        {
            Vector2 mirrored = new Vector2(-pos.x, -pos.y);
            return mirrored;
        }

        public void Fly()
        {
            if (_isPickedUp && !_isFlying)
            {
                _isFlying = true;
                Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
                rb.velocity = _velocity * _speed;

                photonView.RPC("RPC_Fly", PhotonTargets.Others, _velocity);
            }
            else
            {
                _isPickedUp = false;
            }
        }

        [PunRPC]
        private void RPC_Fly(Vector2 velo)
        {
            _isFlying = true;
            _velocity = velo;
            Vector2 mirrorVelocity = new Vector2(-_velocity.x, -_velocity.y);
            Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
            rb.velocity = mirrorVelocity * _speed;
        }

        [PunRPC]
        private void RPC_DestroyFireball()
        {
            Destroy(gameObject);
        }
    }
}
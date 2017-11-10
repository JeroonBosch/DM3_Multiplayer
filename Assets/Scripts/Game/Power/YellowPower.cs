using UnityEngine;
using System.Collections;

namespace Com.Hypester.DM3
{
    public class YellowPower : Photon.MonoBehaviour
    {
        //Fireball power. Besides 'Player' class, the only other photon view class that's client-side rather than master-client.

        public int gameID;
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

        private float _speed = 4f;

        public Transform target;
        int targetRetries = 20;

        private void Update()
        {
            MoveTowardsTarget();
            /*
            if (gameID != PhotonController.Instance.gameID_requested)
            {
                Hide();
            } else { 
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
            */
        }

        private void MoveTowardsTarget()
        {
            if (target == null) { Debug.LogWarning("Target is null."); targetRetries -= 1; if (targetRetries < 0) { Destroy(gameObject); } }

            targetRetries = 10;

            float step = _speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target.position, step);

            if (Vector3.Distance(transform.position, target.transform.position) <= Mathf.Epsilon)
            {
                Debug.Log("Fireball reached the target!");
                if (PhotonNetwork.isMasterClient)
                {
                    PhotonController.Instance.GameController.photonView.RPC("RPC_FireballHit", PhotonTargets.All, PlayerManager.instance.GetPlayerIdByPlayer(ownerPlayer));
                }
                Destroy(gameObject);
            }
        }

        /*
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "OpponentPlayer_Avatar" && photonView.isMine)
            {
                PhotonController.Instance.GameController.photonView.RPC("RPC_FireballHit", PhotonTargets.All);
            }
            else
            {
                Debug.Log("hit " + collision.gameObject.name);
            }
        }
        */
    }
}
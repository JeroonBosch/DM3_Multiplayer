using UnityEngine;
using System.Collections;

namespace Com.Hypester.DM3
{
    public class Wiggle : MonoBehaviour
    {
        private bool _wiggling;
        private float _direction = 1f; //Either 1f or -1f
        private float _wiggleCounter;
        private bool _lockedOut; //Paused?

        private void Update()
        {
            if (PhotonConnect.Instance.GameController != null) {
                if (_wiggling && !_lockedOut && PhotonConnect.Instance.GameController.IsMyTurn())
                {
                    _wiggleCounter += Time.deltaTime;

                    //animation here.
                    transform.Rotate(0, 0, _direction * 3f);

                    if (_direction == 1f && transform.rotation.z > .1f)
                        _direction = -1f;
                    if (_direction == -1f && transform.rotation.z < -.1f)
                        _direction = 1f;

                    if (_wiggleCounter > Constants.WiggleDuration)
                    {
                        _wiggleCounter = 0f;
                        _lockedOut = true;
                        Quaternion r = transform.rotation;
                        transform.rotation = new Quaternion(r.x, r.y, 0f, r.w);
                    }

                }
                else if (_wiggling && _lockedOut && PhotonConnect.Instance.GameController.IsMyTurn())
                {
                    _wiggleCounter += Time.deltaTime;

                    if (_wiggleCounter > Constants.WigglePause)
                    {
                        _wiggleCounter = 0f;
                        _lockedOut = false;
                    }

                }
                else if (_wiggling && !PhotonConnect.Instance.GameController.IsMyTurn()) { 
                    _wiggleCounter = 0f;
                    _lockedOut = false;
                    Quaternion r = transform.rotation;
                    transform.rotation = new Quaternion(r.x, r.y, 0f, r.w);
                }
            }
        }

        public void StartWiggle()
        {
            _wiggling = true;
        }

        public void StopWiggle()
        {
            _wiggling = false;
            _wiggleCounter = 0f;
            Quaternion r = transform.rotation;
            transform.rotation = new Quaternion(r.x, r.y, 0f, r.w);
        }
    }
}
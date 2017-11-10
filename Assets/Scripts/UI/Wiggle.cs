using UnityEngine;
using System.Collections;

namespace Com.Hypester.DM3
{
    public class Wiggle : MonoBehaviour
    {
        //Simple shake/wiggle effect. Currently used for power ups. When a power up is ready for use, it wiggles from time to time.
        private bool _wiggling;
        private float _direction = 1f; //Either 1f or -1f
        private float _wiggleCounter;
        private bool _lockedOut; //Paused?

        private void Update()
        {
            GameHandler gameHandler = PhotonController.Instance.GameController;
            if (gameHandler != null) {
                if (_wiggling && !_lockedOut && gameHandler.IsMyTurn())
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
                else if (_wiggling && _lockedOut && gameHandler.IsMyTurn())
                {
                    _wiggleCounter += Time.deltaTime;

                    if (_wiggleCounter > Constants.WigglePause)
                    {
                        _wiggleCounter = 0f;
                        _lockedOut = false;
                    }

                }
                else if (_wiggling && !gameHandler.IsMyTurn()) { 
                    _wiggleCounter = 0f;
                    _lockedOut = false;
                    Quaternion r = transform.rotation;
                    transform.rotation = new Quaternion(r.x, r.y, 0f, r.w);
                }
            }
        }

        public void ToggleWiggle(bool start)
        {
            _wiggling = start;
            if (!start)
            {
                _wiggleCounter = 0f;
                Quaternion r = transform.rotation;
                transform.rotation = new Quaternion(r.x, r.y, 0f, r.w);
            }
        }
    }
}
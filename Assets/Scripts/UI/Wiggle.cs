﻿using UnityEngine;
using System.Collections;

namespace Com.Hypester.DM3
{
    public class Wiggle : MonoBehaviour
    {
        private GameHandler _game;
        private bool _wiggling;
        private float _direction = 1f;

        private float _wiggleLockoutPause = 4f;
        private float _wiggleDuration = 1f;
        private float _wiggleCounter;
        private bool _lockedOut;

        private void Start()
        {
            _game = GameObject.Find("Grid").GetComponent<GameHandler>();
        }

        private void Update()
        {
            if (_game != null) {
                if (_wiggling && !_lockedOut && _game.IsMyTurn())
                {
                    _wiggleCounter += Time.deltaTime;

                    //animation here.
                    transform.Rotate(0, 0, _direction * 3f);

                    if (_direction == 1f && transform.rotation.z > .1f)
                        _direction = -1f;
                    if (_direction == -1f && transform.rotation.z < -.1f)
                        _direction = 1f;

                    if (_wiggleCounter > _wiggleDuration)
                    {
                        _wiggleCounter = 0f;
                        _lockedOut = true;
                        Quaternion r = transform.rotation;
                        transform.rotation = new Quaternion(r.x, r.y, 0f, r.w);
                    }

                }
                else if (_wiggling && _lockedOut && _game.IsMyTurn())
                {
                    _wiggleCounter += Time.deltaTime;

                    if (_wiggleCounter > _wiggleLockoutPause)
                    {
                        _wiggleCounter = 0f;
                        _lockedOut = false;
                    }

                }
                else if (_wiggling && !_game.IsMyTurn()) { 
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
﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Com.Hypester.DM3
{
    public class BoosterFiller : MonoBehaviour
    {
        [SerializeField] ParticleSystem particle;

        private Transform _rt;
        private Vector2 _startPosition;
        private Vector2 _endPosition;
        private Player _targetPlayer;

        private float _travelTime = .8f;
        private float _travellingFor = 0f;
        private float _randomDirection;

        private bool _damageApplied = false;
        private int _count = 1;
        private float _timeToDelay;
        private float _delayTimer;
        private bool _playing = false;
        private SkillColor skillColor;
        private Color _color;

        public void Init(Player targetPlayer, int count, int color)
        {
            _count = count;
            GameObject powerObject = null;
            _targetPlayer = targetPlayer;

            skillColor = ColorUtility.IntToSkillColor(color);
            PlayerInterface targetPlayerInterface = targetPlayer.playerInterface;
            powerObject = targetPlayerInterface.GetSkillButtonBySkillColor(skillColor).gameObject;
            _color = ColorUtility.GetUnityColorBySkillColor(skillColor);

            _timeToDelay = _count * Constants.DelayAfterTileDestruction;
            _delayTimer = 0f;

            _rt = transform;
            _startPosition = _rt.position;

            _endPosition = new Vector2();
            _endPosition = powerObject.transform.position;

            _randomDirection = Random.Range(-1f, 1f);

            Destroy(gameObject, _travelTime + _timeToDelay);
        }

        private void OnDestroy()
        {
            if (!_damageApplied)
                ApplyDamage();
        }

        // Update is called once per frame
        private void Update()
        {
            if (_delayTimer < _timeToDelay)
            {
                _delayTimer += Time.deltaTime; //time in seconds
            } else {
                if (!_playing) {
                    ParticleSystem.MainModule main = particle.main;
                    main.startColor = new ParticleSystem.MinMaxGradient(_color);
                    particle.Play();
                    _playing = true;
                }
                _travellingFor += Time.deltaTime; //time in seconds
                float t = _travellingFor / _travelTime;
                t = Mathf.Min(t, 1f);

                Vector2 p0 = _startPosition;
                Vector2 p1 = new Vector2(_startPosition.x + 1.5f * _randomDirection, _startPosition.y);
                Vector2 p2 = new Vector2(_endPosition.x + 1.5f * _randomDirection, _endPosition.y);
                Vector3 p3 = _endPosition;
                if (_rt != null) { _rt.position = CalculateBezierPoint(t, p0, p1, p2, p3); } // TODO: this is null in rare cases
            }
        }

        //P0 is start position, P1 is start curve, P2 is end-curve, P3 is end position
        Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector2 p = uuu * p0; //first term
            p += 3 * uu * t * p1; //second term
            p += 3 * u * tt * p2; //third term
            p += ttt * p3; //fourth term

            return p;
        }

        private void ApplyDamage()
        {
            _damageApplied = true;

            _targetPlayer.playerInterface.AnimateSkillButton(skillColor);
        }
    }
}
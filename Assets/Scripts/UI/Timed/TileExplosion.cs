using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Com.Hypester.DM3
{
    public class TileExplosion : MonoBehaviour
    {
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

        public void Init(Player targetPlayer, int count, Sprite image)
        {
            _count = count;
            _timeToDelay = _count * Constants.DelayAfterTileDestruction;
            _delayTimer = 0f;

            _targetPlayer = targetPlayer;
            _rt = transform;
            _startPosition = _rt.position;

            _endPosition = new Vector2();
            if (_targetPlayer.localID == GameObject.FindWithTag("GameController").GetComponent<GameHandler>().MyPlayer.localID)
                _endPosition = GameObject.Find("MyAvatar").transform.position;
            else
                _endPosition = GameObject.Find("OpponentAvatar").transform.position;

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
                    transform.Find("Particle").GetComponent<ParticleSystem>().Play();
                    _playing = true;
                }
                _travellingFor += Time.deltaTime; //time in seconds
                float t = _travellingFor / _travelTime;
                t = Mathf.Min(t, 1f);

                Vector2 p0 = _startPosition;
                Vector2 p1 = new Vector2(_startPosition.x + 3 * _randomDirection, _startPosition.y);
                Vector2 p2 = new Vector2(_endPosition.x + 3 * _randomDirection, _endPosition.y);
                Vector3 p3 = _endPosition;
                _rt.position = CalculateBezierPoint(t, p0, p1, p2, p3);
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

            _targetPlayer.FindInterface().AnimateHealth(); 

            if (GameObject.FindGameObjectsWithTag("ReceiveDamageEffect").Length < 15) { 
                GameObject explosion = Instantiate(Resources.Load("ParticleEffects/PlayerReceiveDamage")) as GameObject;

                float randomX = Random.Range(-.5f, .5f);
                float randomY = Random.Range(-.5f, .5f);
                explosion.transform.position = new Vector2 (_endPosition.x + randomX, _endPosition.y + randomY);
            }

            if ((_targetPlayer.localID == 0 && PhotonNetwork.isMasterClient) || (_targetPlayer.localID == 1 && !PhotonNetwork.isMasterClient))
                iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.ImpactMedium);
        }
    }
}
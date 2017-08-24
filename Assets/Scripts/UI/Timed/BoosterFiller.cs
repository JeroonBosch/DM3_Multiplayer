using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Com.Hypester.DM3
{
    public class BoosterFiller : MonoBehaviour
    {
        private Transform _rt;
        private Vector2 _startPosition;
        private Vector2 _endPosition;
        private Player _targetPlayer;

        private float _travelTime = .8f;
        private float _travellingFor = 0f;
        private float _randomDirection;

        //private bool _damageApplied = false;
        private int _count = 1;
        private float _timeToDelay;
        private float _delayTimer;
        private bool _playing = false;
        private Color _color;

        public void Init(Player targetPlayer, int count, int color)
        {
            _count = count;

            bool myBooster = true;
            GameObject powerObject = null;

            _targetPlayer = targetPlayer;
            if (_targetPlayer.localID == GameObject.Find("Grid").GetComponent<GameHandler>().MyPlayer.localID)
                myBooster = false;

            switch (color)
            {
                case 0 :
                    _color = Color.yellow;
                    if (myBooster)
                        powerObject = GameObject.Find("MyYellow");
                    else
                        powerObject = GameObject.Find("EnemyYellow");
                    break;
                case 1:
                    _color = Color.blue;
                    if (myBooster)
                        powerObject = GameObject.Find("MyBlue");
                    else
                        powerObject = GameObject.Find("EnemyBlue");
                    break;
                case 2:
                    _color = Color.green;
                    if (myBooster)
                        powerObject = GameObject.Find("MyGreen");
                    else
                        powerObject = GameObject.Find("EnemyGreen");
                    break;
                default:
                    _color = Color.red;
                    if (myBooster)
                        powerObject = GameObject.Find("MyRed");
                    else
                        powerObject = GameObject.Find("EnemyRed");
                    break;
            }

            _timeToDelay = _count * Constants.DelayAfterTileDestruction;
            _delayTimer = 0f;

            _rt = transform;
            _startPosition = _rt.position;

            _endPosition = new Vector2();
            _endPosition = powerObject.transform.position;

            _randomDirection = Random.Range(-1f, 1f);

            Destroy(gameObject, _travelTime + _timeToDelay);
        }

        /*private void OnDestroy()
        {
            if (!_damageApplied)
                ApplyDamage();
        }*/

        // Update is called once per frame
        private void Update()
        {
            if (_delayTimer < _timeToDelay)
            {
                _delayTimer += Time.deltaTime; //time in seconds
            } else {
                if (!_playing) {
                    ParticleSystem.MainModule main = transform.Find("Particle").GetComponent<ParticleSystem>().main;
                    main.startColor = new ParticleSystem.MinMaxGradient(_color);
                    transform.Find("Particle").GetComponent<ParticleSystem>().Play();
                    _playing = true;
                }
                _travellingFor += Time.deltaTime; //time in seconds
                float t = _travellingFor / _travelTime;
                t = Mathf.Min(t, 1f);

                Vector2 p0 = _startPosition;
                Vector2 p1 = new Vector2(_startPosition.x + 5 * _randomDirection, _startPosition.y);
                Vector2 p2 = new Vector2(_endPosition.x + 5 * _randomDirection, _endPosition.y);
                Vector3 p3 = _endPosition;
                Debug.Log(p0);
                Debug.Log(p1);
                Debug.Log(p2);
                Debug.Log(p3);
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

        /*private void ApplyDamage()
        {
            _damageApplied = true;

            if (GameObject.FindGameObjectsWithTag("ReceiveDamageEffect").Length < 15) { 
                GameObject explosion = Instantiate(Resources.Load("ParticleEffects/PlayerReceiveDamage")) as GameObject;

                float randomX = Random.Range(-.5f, .5f);
                float randomY = Random.Range(-.5f, .5f);
                explosion.transform.position = new Vector2 (_endPosition.x + randomX, _endPosition.y + randomY);
            }
        }*/
    }
}
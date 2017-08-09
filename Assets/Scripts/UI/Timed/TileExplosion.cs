using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Com.Hypester.DM3
{
    public class TileExplosion : MonoBehaviour
    {
        private RectTransform _rt;
        private Vector2 _startPosition;
        private Player _targetPlayer;

        private float _travelTime = .66f;
        private float _travellingFor = 0f;
        private float _random;

        private bool _damageApplied = false;
        private float _damageMultiplier = 1;

        public void Init(Player targetPlayer, float damageMultiplier)
        {
            _damageMultiplier = damageMultiplier;

            _targetPlayer = targetPlayer;
            _rt = GetComponent<RectTransform>();
            _startPosition = _rt.position;
            _random = Random.Range(-1f, 1f);


            //_travelTime = .4f + count * .5f;
            Destroy(this.gameObject, _travelTime);
        }

        private void OnDestroy()
        {
            //RootController.Instance.EnableControls();
            if (!_damageApplied)
                ApplyDamage();
        }

        // Update is called once per frame
        private void Update()
        {
                Vector2 endPosition = new Vector2();
                if (_targetPlayer.localID == GameObject.Find("Grid").GetComponent<GameHandler>().MyPlayer.localID)
                    endPosition = GameObject.Find("MyAvatar").GetComponent<RectTransform>().position;
                else
                    endPosition = GameObject.Find("OpponentAvatar").GetComponent<RectTransform>().position;

                _travellingFor += Time.deltaTime; //time in seconds
                float t = _travellingFor / _travelTime;
                t = Mathf.Min(t, 1f);

                Vector2 p0 = _startPosition;
                Vector2 p1 = new Vector2(_startPosition.x + 400 * _random, _startPosition.y);
                Vector2 p2 = new Vector2(endPosition.x + 400 * _random, endPosition.y);
                //Vector2 p2 = endPosition;
                Vector3 p3 = endPosition;
                _rt.position = CalculateBezierPoint(t, p0, p1, p2, p3);
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
            //_damageApplied = true;
            //_targetPlayer.NormalExplosion();
            //_targetPlayer.ReceiveDamage(1 * _damageMultiplier);
        }
    }
}
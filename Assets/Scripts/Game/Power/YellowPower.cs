using UnityEngine;

namespace Com.Hypester.DM3
{
    public class YellowPower : Photon.MonoBehaviour
    {
        public Player ownerPlayer;
        public Transform target;

        private Vector2 _startPosition;
        private Vector2 _endPosition;
        float _travellingFor = 0f;
        private float _travelTime = 1.2f;
        private float _randomDirection;
        bool isInited = false;
        bool reachedDestination = false;

        private float _speed = 20f;
        int targetRetries = 20;

        public void Init()
        {
            _startPosition = transform.position;
            _endPosition = target.position;
            _randomDirection = Random.Range(-1f, 1f);
            isInited = true;
        }

        private void Update()
        {
            if (isInited) { MoveTowardsTarget(); }
        }

        private void MoveTowardsTarget()
        {
            if (reachedDestination) { return; }
            if (target == null) { Debug.LogWarning("Target is null."); targetRetries -= 1; if (targetRetries < 0) { Destroy(gameObject); } }

            targetRetries = 10;

            // Moving
            _travellingFor += Time.deltaTime; //time in seconds
            float t = _travellingFor / _travelTime;
            t = Mathf.Min(t, 1f);

            Vector2 p0 = _startPosition;
            Vector2 p1 = new Vector2(_startPosition.x + 3 * _randomDirection, _startPosition.y);
            Vector2 p2 = new Vector2(_endPosition.x + 3 * _randomDirection, _endPosition.y);
            Vector3 p3 = _endPosition;
            transform.position = CalculateBezierPoint(t, p0, p1, p2, p3);

            if (Vector3.Distance(transform.position, _endPosition) <= 0.1f || _travellingFor >= _travelTime)
            {
                reachedDestination = true;
                ownerPlayer.opponent.playerInterface.AnimateAvatar();
                Debug.Log("Fireball reached the target!");
                if (PhotonNetwork.isMasterClient)
                {
                    PhotonController.Instance.GameController.photonView.RPC("RPC_FireballHit", PhotonTargets.All, PlayerManager.instance.GetPlayerIdByPlayer(ownerPlayer));
                }
                Destroy(gameObject);
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
    }
}
using UnityEngine;

namespace Com.Hypester.DM3
{
    public class YellowPower : Photon.MonoBehaviour
    {
        public Player ownerPlayer;
        public Transform target;

        private float _speed = 4f;
        int targetRetries = 20;

        private void Update()
        {
            MoveTowardsTarget();
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
    }
}
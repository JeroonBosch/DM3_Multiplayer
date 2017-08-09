using UnityEngine;
using System.Collections;

namespace Com.Hypester.DM3
{
    public class DestroyAfterX : MonoBehaviour
    {
        public float destroyAfterTime = 2f;
        private float _timer = 0f;

        void Update()
        {
            if (_timer > destroyAfterTime)
                Destroy(gameObject);
            else
                _timer += Time.deltaTime;
        }
    }
}
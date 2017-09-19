using UnityEngine;
using System.Collections;

namespace Com.Hypester.DM3
{
    public class TimedEffect : MonoBehaviour
    {
        public GameObject prefabToCreate;
        public TileView basetileToHide;

        public float createAfterTime = 2f;
        private float _timer = 0f;

        void Update()
        {
            if (_timer > createAfterTime) {
                GameObject go = Instantiate(prefabToCreate) as GameObject;
                go.transform.position = gameObject.transform.position;

                if (basetileToHide != null) { 
                    basetileToHide.color = Constants.AmountOfColors;
                    basetileToHide.isBeingDestroyed = false;
                }
                Destroy(gameObject);
            }
            else
                _timer += Time.deltaTime;
        }
    }
}
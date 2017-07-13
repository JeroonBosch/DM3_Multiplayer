using UnityEngine;
using UnityEngine.UI;
using Lean.Touch;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Com.Hypester.DM3
{
    public class TestMirrorPosition : MonoBehaviour
    {
        LeanFinger _finger;
        // Update is called once per frame
        void Update()
        {
            if (_finger != null)
            {
                //gameObject.GetComponent<RectTransform>().position = _finger.GetWorldPosition(1f);
                Vector2 pos = _finger.GetWorldPosition(1f);
                transform.position = pos;
                transform.localPosition = new Vector2(-transform.localPosition.x, -transform.localPosition.y);
            }
        }

        private void OnEnable()
        {
            LeanTouch.OnFingerDown += OnFingerDown;
            LeanTouch.OnFingerUp += OnFingerUp;
        }

        private void OnDisable()
        {
            LeanTouch.OnFingerDown -= OnFingerDown;
            LeanTouch.OnFingerUp -= OnFingerUp;
        }

        void OnFingerDown(LeanFinger finger)
        {
            if (finger.Index == 0)
            {
                _finger = finger;
            }
        }

        void OnFingerUp(LeanFinger finger)
        {
            if (finger.Index == 0)
            {
                _finger = null;
            }
        }
    }
}
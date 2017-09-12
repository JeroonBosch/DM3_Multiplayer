using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lean.Touch;

namespace Com.Hypester.DM3
{
    public class StageCarousel : MonoBehaviour
    {
        private List<GameObject> _carouselObjects;
        private int _selectedIndex = 0;
        private int _highestIndex;
        private Coroutine _moveCoroutine;

        private GameObject _buttonLeft;
        private GameObject _buttonRight;

        private void Start()
        {
            _carouselObjects = new List<GameObject>();
            foreach (Transform child in transform)
            {
                if (child.tag == "CarouselStage")
                    _carouselObjects.Add(child.gameObject);
            }


            _highestIndex = _carouselObjects.Count - 1;

            _buttonLeft = transform.parent.Find("ButtonLeft").Find("ArrowLeft").gameObject;
            _buttonRight = transform.parent.Find("ButtonRight").Find("ArrowRight").gameObject;
        }

        private void Update()
        {
            if (_highestIndex >= (_selectedIndex + 1))
                _buttonRight.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
            else
                _buttonRight.GetComponent<Image>().color = new Color(1f, 1f, 1f, .3f);

            if (0 <= (_selectedIndex - 1))
                _buttonLeft.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
            else
                _buttonLeft.GetComponent<Image>().color = new Color(1f, 1f, 1f, .3f);
        }

        IEnumerator SmoothMove(Vector2 startpos , Vector2 endpos, float seconds)
        {

            float t = 0.0f;
            while (t <= 1.0)
            {
                t += Time.deltaTime / seconds;
                transform.localPosition = Vector2.Lerp(startpos, endpos, Mathf.SmoothStep(0.0f, 1.0f, t));
                yield return null;
            }
            _moveCoroutine = null;
        }

            private void OnEnable()
        {
            LeanTouch.OnFingerSwipe += OnFingerSwipe;
        }

        private void OnDisable()
        {
            LeanTouch.OnFingerSwipe -= OnFingerSwipe;
        }

        private void OnFingerSwipe(LeanFinger finger)
        {
            var swipe = finger.SwipeScreenDelta;

            if (swipe.x < -Mathf.Abs(swipe.y))
            {
                SelectNext();
            }

            if (swipe.x > Mathf.Abs(swipe.y))
            {
                SelectPrevious();
            }
        }

        //button functions
        public void SelectNext ()
        {
            if (_moveCoroutine != null)
                return;

            if (_highestIndex >= (_selectedIndex + 1))
                _selectedIndex++;

            Vector2 wantedPosition = new Vector2(-1000f * _selectedIndex, 0f);
            Vector2 startspot = transform.localPosition;
            _moveCoroutine = StartCoroutine(SmoothMove(startspot, wantedPosition, 1.0f));
        }

        public void SelectPrevious()
        {
            if (_moveCoroutine != null)
                return;

            if (0 <= (_selectedIndex - 1))
                _selectedIndex--;

            Vector2 wantedPosition = new Vector2(-1000f * _selectedIndex, 0f);
            Vector2 startspot = transform.localPosition;
            _moveCoroutine = StartCoroutine(SmoothMove(startspot, wantedPosition, 1.0f));
        }
    }
}
using UnityEngine;

namespace Com.Hypester.DM3
{
    public class BaseTile : MonoBehaviour
    {
        public Vector2 position { get; set; }

        private Vector2 _startPos;
        private Vector2 _endPos;
        private bool _animating;

        private void Start()
        {
            _startPos = new Vector2();
            _endPos = new Vector2();
            _animating = false;
        }

        private void Update()
        {
            if (_animating)
            {
                transform.localPosition = Vector2.MoveTowards(transform.localPosition, _endPos, .05f);
            }
        }

        public void Animate(int curPlayer)
        {
            /*if (!_animating) { 
                Debug.Log("Animating " + position);
                if (curPlayer == 0)
                    _startPos = new Vector2(transform.localPosition.x, transform.localPosition.y + Constants.tileHeight);
                else
                    _startPos = new Vector2(transform.localPosition.x, transform.localPosition.y - Constants.tileHeight);
                _endPos = new Vector2(transform.localPosition.x, transform.localPosition.y);
                transform.localPosition = _startPos;
                _animating = true;
            }*/
        }
    }
}
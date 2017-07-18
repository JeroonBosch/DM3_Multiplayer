using UnityEngine;

namespace Com.Hypester.DM3
{
    public class BaseTile : MonoBehaviour
    {
        public Vector2 position { get; set; }

        private Vector2 _startPos;
        private Vector2 _endPos;
        private bool _animate;

        private void Start()
        {
            _startPos = new Vector2();
            _endPos = new Vector2();
            _animate = false;
        }

        private void Update()
        {
            if (_animate)
            {
                Vector2.MoveTowards(_endPos, _startPos, .2f);
            }
        }

        public void Animate (int curPlayer)
        {
            if (curPlayer == 0)
                _startPos = new Vector2(transform.localPosition.x, transform.localPosition.y + Constants.tileHeight);
            else
                _startPos = new Vector2(transform.localPosition.x, transform.localPosition.y - Constants.tileHeight);
            _endPos = new Vector2(transform.localPosition.x, transform.localPosition.y);
            _animate = true;
        }
    }
}
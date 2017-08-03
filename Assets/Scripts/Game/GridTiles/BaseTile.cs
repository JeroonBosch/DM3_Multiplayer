using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class BaseTile : MonoBehaviour
    {
        public Vector2 position { get; set; }

        private Vector2 _startPos;
        private Vector2 _endPos;
        private bool _animating;

        private int _color = 0;
        public int color { get { return _color;  } set { _color = value; ColorChange(); } }

        private bool _selected;
        public bool SetSelected { set { _selected = value; SelectionChange(); } }

        private void Start()
        {
            _startPos = new Vector2();
            _endPos = new Vector2();
            _animating = false;
        }

        private void Update()
        {
            if (_animating)
                transform.localPosition = Vector2.MoveTowards(transform.localPosition, _endPos, 10f);

            if (transform.localPosition.x == _endPos.x && transform.localPosition.y == _endPos.y)
                _animating = false;
        }

        private void ColorChange ()
        {
            GetComponent<Image>().sprite = HexSprite(TileTypes.EColor.yellow + _color);
        }

        private void SelectionChange()
        {
            if (_selected)
                GetComponent<Image>().sprite = HexSpriteSelected(TileTypes.EColor.yellow + _color);
            else 
                GetComponent<Image>().sprite = HexSprite(TileTypes.EColor.yellow + _color);
        }

        public void Animate (float distance)
        {
            if (!_animating)
            {
                //Debug.Log("Animating " + position + " d: " + distance);
                //if (curPlayer == 0)
                //    _startPos = new Vector2(transform.localPosition.x, transform.localPosition.y - distance);
                //else
                _startPos = new Vector2(transform.localPosition.x, transform.localPosition.y + distance);
                _endPos = new Vector2(transform.localPosition.x, transform.localPosition.y);
                transform.localPosition = _startPos;
                _animating = true;
            }
        }

        #region SpriteRendering
        public Sprite HexSprite(TileTypes.EColor color)
        {
            if (color == TileTypes.EColor.blue)
                return Resources.LoadAll<Sprite>("Tiles/TilesHexa128x128")[1];
            else if (color == TileTypes.EColor.green)
                return Resources.LoadAll<Sprite>("Tiles/TilesHexa128x128")[2];
            else if (color == TileTypes.EColor.red)
                return Resources.LoadAll<Sprite>("Tiles/TilesHexa128x128")[3];
            return Resources.LoadAll<Sprite>("Tiles/TilesHexa128x128")[0]; //Yellow
        }

        public Sprite HexSpriteSelected(TileTypes.EColor color)
        {
            if (color == TileTypes.EColor.blue)
                return Resources.LoadAll<Sprite>("Tiles/TilesHexaSelected128x128")[1];
            else if (color == TileTypes.EColor.green)
                return Resources.LoadAll<Sprite>("Tiles/TilesHexaSelected128x128")[2];
            else if (color == TileTypes.EColor.red)
                return Resources.LoadAll<Sprite>("Tiles/TilesHexaSelected128x128")[3];
            return Resources.LoadAll<Sprite>("Tiles/TilesHexaSelected128x128")[0]; //Yellow
        }
        #endregion
    }
}
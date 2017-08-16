using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Com.Hypester.DM3
{
    public class BaseTile : MonoBehaviour
    {
        //Purely visualized version of the 'Tile' class, in Grid.cs. None of this data is transferred.
        public Vector2 position { get; set; }

        private Vector2 _startPos;
        private Vector2 _endPos;
        private bool _animating;

        private int _color = 0;
        public int color { get { return _color;  } set { _color = value; ColorChange(); } }

        private GameObject _boosterObj;
        private int _boosterLevel = 0;
        public int boosterLevel { get { return _boosterLevel; } set { if (_boosterLevel != value) { _boosterLevel = value; BoosterChange(); } } }

        private bool _selected;
        public bool SetSelected { set { _selected = value; SelectionChange(); } }

        private bool _collateral;
        public bool collateral { get { return _collateral; } set { _collateral = value; CollateralChange(); } }

        /*private bool _isBeingDestroyed;
        public bool isBeingDestroyed { get { return _isBeingDestroyed; }}*/

        private void Start()
        {
            _startPos = new Vector2();
            _endPos = new Vector2();
            _animating = false;
        }

        private void Update()
        {
            if (_animating)
                transform.localPosition = Vector2.MoveTowards(transform.localPosition, _endPos, 500f * Time.deltaTime);

            if (transform.localPosition.x == _endPos.x && transform.localPosition.y == _endPos.y)
                _animating = false;
        }

        private void ColorChange ()
        {
            if (_color < Constants.AmountOfColors)
                GetComponent<Image>().sprite = HexSprite(TileTypes.EColor.yellow + _color);
            else
                GetComponent<Image>().enabled = false;
        }

        private void BoosterChange()
        {
            if (_boosterObj != null)
            {
                Destroy(_boosterObj);
                _boosterObj = null;
            }

            if (_boosterLevel == 1)
            {
                _boosterObj = Instantiate(Resources.Load("Tiles/Modifications/Booster1")) as GameObject;
                _boosterObj.transform.SetParent(transform, false);
                _boosterObj.transform.rotation = new Quaternion(0f, 0f, 0f, _boosterObj.transform.rotation.w);
            } else if (_boosterLevel == 2)
            {
                _boosterObj = Instantiate(Resources.Load("Tiles/Modifications/Booster2")) as GameObject;
                _boosterObj.transform.SetParent(transform, false);
                _boosterObj.transform.rotation = new Quaternion(0f, 0f, 0f, _boosterObj.transform.rotation.w);
            } else if  (_boosterLevel == 3)
            {
                _boosterObj = Instantiate(Resources.Load("Tiles/Modifications/Booster3")) as GameObject;
                _boosterObj.transform.SetParent(transform, false);
                _boosterObj.transform.rotation = new Quaternion(0f, 0f, 0f, _boosterObj.transform.rotation.w);
            }
        }

        private void SelectionChange()
        {
            if (_selected)
                GetComponent<Image>().sprite = HexSpriteSelected(TileTypes.EColor.yellow + _color);
            else 
                GetComponent<Image>().sprite = HexSprite(TileTypes.EColor.yellow + _color);
        }

        private void CollateralChange ()
        {
            if (!_selected && _collateral)
                GetComponent<Image>().sprite = HexSpriteCollateral(TileTypes.EColor.yellow + _color);
            else if (_selected)
                GetComponent<Image>().sprite = HexSpriteSelected(TileTypes.EColor.yellow + _color);
            else
                GetComponent<Image>().sprite = HexSprite(TileTypes.EColor.yellow + _color);
        }

        public void Animate (float distance)
        {
            if (!_animating)
            {
                _startPos = new Vector2(transform.localPosition.x, transform.localPosition.y + distance);
                _endPos = new Vector2(transform.localPosition.x, transform.localPosition.y);
                transform.localPosition = _startPos;
                _animating = true;
            }
        }

        public List<BaseTile> ListCollateralDamage (GameHandler grid, float radius)
        {
            //GameHandler grid = GameObject.Find("Grid").GetComponent<GameHandler>();
            List<BaseTile> toDestroy = new List<BaseTile>();

            if (_boosterLevel == 1) { 
                List<BaseTile> adjacentInRadius = grid.FindAdjacentTiles(position, radius);
                foreach (BaseTile tile in adjacentInRadius)
                {
                    if (tile)// && !tile.isBeingDestroyed)
                        if (!toDestroy.Contains(tile) && !(tile.position.x == position.x && tile.position.y == position.y))
                            toDestroy.Add(tile);
                }
            }
            else if (_boosterLevel == 2)
            {
                List<Vector2> positions = new List<Vector2>();

                bool odd = (position.x % 2 == 1);
                float oddx = odd ? 1f : 0;
                for (int i = 0; i < Constants.gridYsize; i++) //Vertical
                {
                    float px = position.x;
                    float py = i;
                    positions.Add(new Vector2(px, py));
                }
                float diag1Start = Mathf.Floor((position.y + position.x * 0.5f) - 0.5f * oddx);
                for (int i = 0; i < Constants.gridXsize; i++) //Diag.1 direction: \ topleft to bottomright
                {
                    float px = i;
                    float py = Mathf.Floor(diag1Start + 0.5f - i * 0.5f);
                    if (Exists(px, py))
                        positions.Add(new Vector2(px, py));
                }
                float diag2Start = Mathf.Floor((position.y - position.x * 0.5f) - 0.5f * oddx);
                for (int i = 0; i < Constants.gridXsize; i++) //Diag.2 direction: / bottomleft to topright
                {
                    float px = i;
                    float py = Mathf.Floor(diag2Start + 0.5f + i * 0.5f);
                    if (Exists(px, py))
                        positions.Add(new Vector2(px, py));
                }


                for (int i = 0; i < positions.Count; i++)
                {
                    BaseTile baseTile = grid.BaseTileAtPos(positions[i]);
                    if (baseTile)// && !baseTile.isBeingDestroyed)
                        toDestroy.Add(baseTile);
                }

                List<BaseTile> adjacentInRadius = grid.FindAdjacentTiles(new Vector2(position.x, position.y), radius);
                foreach (BaseTile tile in adjacentInRadius)
                {
                    if (tile)// && !tile.isBeingDestroyed)
                        if (!toDestroy.Contains(tile) && !(tile.position.x == position.x && tile.position.y == position.y))
                            toDestroy.Add(tile);
                }
            }
            else if (_boosterLevel == 3)
            {
                List<Vector2> positions = new List<Vector2>();
                bool odd = (position.x % 2 == 1);
                float oddx = odd ? 1f : 0;
                for (int i = 0; i < Constants.gridXsize; i++) //Horizontal
                {
                    float px = position.x;
                    float py = i;
                    positions.Add(new Vector2(px, py));
                }
                float diag1Start = Mathf.Floor((position.y + position.x * 0.5f) - 0.5f * oddx);
                for (int i = 0; i < Constants.gridXsize; i++) //Diag.1 direction: \ topleft to bottomright
                {
                    float px = i;
                    float py = Mathf.Floor(diag1Start + 0.5f - i * 0.5f);
                    if (Exists(px, py))
                        positions.Add(new Vector2(px, py));
                }
                float diag2Start = Mathf.Floor((position.y - position.x * 0.5f) - 0.5f * oddx);
                for (int i = 0; i < Constants.gridXsize; i++) //Diag.2 direction: / bottomleft to topright
                {
                    float px = i;
                    float py = Mathf.Floor(diag2Start + 0.5f + i * 0.5f);
                    if (Exists(px, py))
                        positions.Add(new Vector2(px, py));
                }


                for (int i = 0; i < positions.Count; i++)
                {
                    BaseTile baseTile = grid.BaseTileAtPos(positions[i]);
                    if (baseTile)// && !baseTile.isBeingDestroyed)
                        toDestroy.Add(baseTile);
                }

                float newRadius = Mathf.Max(2f, radius);
                List<BaseTile> adjacentInRadius = grid.FindAdjacentTiles(new Vector2(position.x, position.y), newRadius);
                foreach (BaseTile tile in adjacentInRadius)
                {
                    if (tile)// && !tile.isBeingDestroyed)
                        if (!toDestroy.Contains(tile) && !(tile.position.x == position.x && tile.position.y == position.y))
                            toDestroy.Add(tile);
                }
            }

            return toDestroy;
        }

        private bool Exists(float x, float y)
        {
            if (x < Constants.gridXsize && x > -1 && y < Constants.gridYsize && y > -1)
                return true;
            else
                return false;
        }

        public float DistanceToTile (BaseTile tile)
        {
            Vector2 thisPosition = new Vector2(position.x, position.y);
            Vector2 thatPosition = new Vector2(tile.position.x, tile.position.y);

            if (thisPosition.x % 2 != 0f)
                thisPosition = new Vector2(thisPosition.x, thisPosition.y - 0.5f);
            if (thatPosition.x % 2 != 0f)
                thatPosition = new Vector2(thatPosition.x, thatPosition.y - 0.5f);

            return Vector2.Distance(thisPosition, thatPosition);
        }

        public bool IsAdjacentTo(BaseTile tile)
        {
            if (DistanceToTile(tile) <= Constants.DistanceBetweenTiles)
                return true;
            else
                return false;
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

        public Sprite HexSpriteCollateral(TileTypes.EColor color)
        {
            if (color == TileTypes.EColor.blue)
                return Resources.LoadAll<Sprite>("Tiles/TilesHexaExplode128x128")[1];
            else if (color == TileTypes.EColor.green)
                return Resources.LoadAll<Sprite>("Tiles/TilesHexaExplode128x128")[2];
            else if (color == TileTypes.EColor.red)
                return Resources.LoadAll<Sprite>("Tiles/TilesHexaExplode128x128")[3];
            return Resources.LoadAll<Sprite>("Tiles/TilesHexaExplode128x128")[0]; //Yellow
        }
        #endregion
    }
}
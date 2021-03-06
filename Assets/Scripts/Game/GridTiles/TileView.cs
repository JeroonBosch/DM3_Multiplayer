﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Com.Hypester.DM3
{
    public class TileView : MonoBehaviour
    {
        [SerializeField] GameObject booster1;
        [SerializeField] GameObject booster2;
        [SerializeField] GameObject booster3;
        [SerializeField] GameObject boosterTrap;
        [SerializeField] GameObject boosterTrapHover;
        [SerializeField] GameObject boosterDiagonal;

        [SerializeField] Image tileImage;
        float activeSaturation = 0.5f;
        float deactiveSaturation = 0.19f;
        float activeContrast = 0.5f;
        float deactiveContrast = 0.38f;

        public static List<TileView> areaList = new List<TileView>();

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

        private GameObject _trapHover;

        private bool _selected;
        public bool SetSelected { set { _selected = value; SelectionChange(); } }

        private bool _collateral;
        public bool collateral { get { return _collateral; } set { _collateral = value; PhotonController.Instance.GameController.UpdateCollateral(this); CollateralChange(); } }

        private bool _isBeingDestroyed;
        public bool isBeingDestroyed { get { return _isBeingDestroyed; } set { _isBeingDestroyed = value;} }

        protected virtual void Start()
        {
            _startPos = new Vector2();
            _endPos = new Vector2();
            _animating = false;

            if (tileImage == null) { tileImage = GetComponent<Image>(); }
        }

        protected virtual void Update()
        {
            if (_animating)
                transform.localPosition = Vector2.MoveTowards(transform.localPosition, _endPos, Constants.TileDroppingSpeed * Time.deltaTime);

            if (transform.localPosition.x == _endPos.x && transform.localPosition.y == _endPos.y)
                _animating = false;
        }

        protected void ColorChange ()
        {
            if (_color < Constants.AmountOfColors)
                tileImage.sprite = HexSprite(TileTypes.EColor.yellow + _color);
            else
                tileImage.enabled = false;
        }

        protected void BoosterChange()
        {
            if (_boosterObj != null)
            {
                Destroy(_boosterObj);
                _boosterObj = null;
            }

            GameObject boosterPrefab = null;
            switch (_boosterLevel)
            {
                case 1:
                    boosterPrefab = booster1;
                    break;
                case 2:
                    boosterPrefab = boosterDiagonal;
                    break;
                case 3:
                    boosterPrefab = booster2;
                    break;
                case 4:
                    boosterPrefab = booster3;
                    break;
                case 5:
                case 6:
                    boosterPrefab = boosterTrap;
                    break;
            }

            if (boosterPrefab == null) { return; }
            _boosterObj = Instantiate(boosterPrefab);
            _boosterObj.transform.SetParent(transform, false);
            _boosterObj.transform.rotation = Quaternion.identity;

            if (_boosterLevel == 5) { _boosterObj.GetComponent<TrapBooster>().ownerPlayer = 0; }
            else if (_boosterLevel == 6) { _boosterObj.GetComponent<TrapBooster>().ownerPlayer = 1; }
        }

        public void TrapHovered ()
        {
            _trapHover = Instantiate(boosterTrapHover) as GameObject;
            _trapHover.transform.SetParent(transform, false);
            _trapHover.transform.rotation = new Quaternion(0f, 0f, 0f, _trapHover.transform.rotation.w);
        }

        public void TrapNotHovered()
        {
            if (_trapHover != null)
            {
                Destroy(_trapHover);
                _trapHover = null;
            }
        }

        protected void SelectionChange()
        {
            if (_selected)
                tileImage.sprite = HexSpriteSelected(TileTypes.EColor.yellow + _color);
            else
                tileImage.sprite = HexSprite(TileTypes.EColor.yellow + _color);
        }

        protected void CollateralChange ()
        {
            if (!_selected && _collateral)
                tileImage.sprite = HexSpriteCollateral(TileTypes.EColor.yellow + _color);
            else if (_selected)
                tileImage.sprite = HexSpriteSelected(TileTypes.EColor.yellow + _color);
            else
                tileImage.sprite = HexSprite(TileTypes.EColor.yellow + _color);
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

        public void GetArea()
        {
            List<TileView> adjacentTiles = PhotonController.Instance.GameController.FindAdjacentTiles(position, 1);
            foreach (TileView tv in adjacentTiles)
            {
                if (tv != this && tv.color == color && !areaList.Contains(tv))
                {
                    areaList.Add(tv);
                    tv.GetArea();
                }
            }
        }

        public List<TileView> ListCollateralDamage (GameHandler grid, float radius)
        {
            //GameHandler grid = GameObject.FindWithTag("GameController").GetComponent<GameHandler>();
            List<TileView> toDestroy = new List<TileView>();

            if (_boosterLevel == 1) {

                bool oddx = position.x % 2 != 0;

                float posBotY = oddx ? position.y : (position.y+1);
                float posTopY = oddx ? position.y-1 : position.y;

                // Bottom
                TileView tileBottomLeft = grid.TileViewAtPos(new Vector2(position.x+1, posBotY));
                if (tileBottomLeft != null && Exists(tileBottomLeft.position.x, tileBottomLeft.position.y) && !toDestroy.Contains(tileBottomLeft)) { toDestroy.Add(tileBottomLeft); }
                TileView tileBottomRight = grid.TileViewAtPos(new Vector2(position.x - 1, posBotY));
                if (tileBottomRight != null && Exists(tileBottomRight.position.x, tileBottomRight.position.y) && !toDestroy.Contains(tileBottomRight)) { toDestroy.Add(tileBottomRight); }

                // Top
                TileView tileTopLeft = grid.TileViewAtPos(new Vector2(position.x+1, posTopY));
                if (tileTopLeft != null && Exists(tileTopLeft.position.x, tileTopLeft.position.y) && !toDestroy.Contains(tileTopLeft)) { toDestroy.Add(tileTopLeft); }
                TileView tileTopRight = grid.TileViewAtPos(new Vector2(position.x-1, posTopY));
                if (tileTopRight != null && Exists(tileTopRight.position.x, tileTopRight.position.y) && !toDestroy.Contains(tileTopRight)) { toDestroy.Add(tileTopRight); }
                
            }
            else if (_boosterLevel == 2)
            {
                List<Vector2> positions = new List<Vector2>();

                bool odd = (position.x % 2 == 1);
                float oddx = odd ? 1f : 0;

                float diag1Start = Mathf.Floor((position.y + position.x * 0.5f) - 0.5f * oddx);
                for (int i = 0; i < Constants.gridXsize; i++) //Diag.1 direction: \ topleft to bottomright
                {
                    // if (Mathf.Abs(i - position.x) > 1) { continue; } // used to control the range of the diagonal
                    float px = i;
                    float py = Mathf.Floor(diag1Start + 0.5f - i * 0.5f);
                    if (Exists(px, py))
                        positions.Add(new Vector2(px, py));
                }

                for (int i = 0; i < positions.Count; i++)
                {
                    TileView baseTile = grid.TileViewAtPos(positions[i]);
                    if (baseTile)// && !baseTile.isBeingDestroyed)
                        toDestroy.Add(baseTile);
                }
            }
            else if (_boosterLevel == 3)
            {
                List<TileView> adjacentInRadius = grid.FindAdjacentTiles(position, radius);
                foreach (TileView tile in adjacentInRadius)
                {
                    if (tile)// && !tile.isBeingDestroyed)
                        if (!toDestroy.Contains(tile) && !(tile.position.x == position.x && tile.position.y == position.y))
                            toDestroy.Add(tile);
                }
                /*
                List<Vector2> positions = new List<Vector2>();
                for (int i = -Constants.BoosterLevel2Vertical; i < (Constants.BoosterLevel2Vertical + 1); i++) //Vertical
                {
                    float px = position.x;
                    float py = position.y + i;
                    if (Exists(px, py)) { positions.Add(new Vector2(px, py)); }
                }

                bool odd = (position.x % 2 == 1);
                float oddx = odd ? 1f : 0;

                float diag1Start = Mathf.Floor((position.y + position.x * 0.5f) - 0.5f * oddx);
                for (int i = 0; i < Constants.gridXsize; i++) //Diag.1 direction: \ topleft to bottomright
                {
                    if (Mathf.Abs(i - position.x) > Constants.BoosterLevel2Vertical) { continue; }
                    float px = i;
                    float py = Mathf.Floor(diag1Start + 0.5f - i * 0.5f);
                    if (Exists(px, py))
                        positions.Add(new Vector2(px, py));
                }
                float diag2Start = Mathf.Floor((position.y - position.x * 0.5f) - 0.5f * oddx);
                for (int i = 0; i < Constants.gridXsize; i++) //Diag.2 direction: / bottomleft to topright
                {
                    if (Mathf.Abs(i - position.x) > Constants.BoosterLevel2Vertical) { continue; }
                    float px = i;
                    float py = Mathf.Floor(diag2Start + 0.5f + i * 0.5f);
                    if (Exists(px, py))
                        positions.Add(new Vector2(px, py));
                }
                
                for (int i = 0; i < positions.Count; i++)
                {
                    TileView baseTile = grid.TileViewAtPos(positions[i]);
                    if (baseTile)// && !baseTile.isBeingDestroyed)
                        toDestroy.Add(baseTile);
                }
                */
            }
            else if (_boosterLevel == 4)
            {
                List<Vector2> positions = new List<Vector2>();

                /*
                for (int i = 0; i < Constants.gridXsize; i++) //Horizontal
                {
                    float px = position.x;
                    float py = i;
                    positions.Add(new Vector2(px, py));
                }
                */

                for (int i = -Constants.BoosterLevel2Vertical; i < (Constants.BoosterLevel2Vertical + 1); i++) //Vertical
                {
                    float px = position.x;
                    float py = position.y + i;
                    if (Exists(px, py)) { positions.Add(new Vector2(px, py)); }
                }

                bool odd = (position.x % 2 == 1);
                float oddx = odd ? 1f : 0;

                float diag1Start = Mathf.Floor((position.y + position.x * 0.5f) - 0.5f * oddx);
                for (int i = 0; i < Constants.gridXsize; i++) //Diag.1 direction: \ topleft to bottomright
                {
                    if (Mathf.Abs(i - position.x) > 1) { continue; } // used to control the range of the diagonal
                    float px = i;
                    float py = Mathf.Floor(diag1Start + 0.5f - i * 0.5f);
                    if (Exists(px, py))
                        positions.Add(new Vector2(px, py));
                }
                float diag2Start = Mathf.Floor((position.y - position.x * 0.5f) - 0.5f * oddx);
                for (int i = 0; i < Constants.gridXsize; i++) //Diag.2 direction: / bottomleft to topright
                {
                    if (Mathf.Abs(i - position.x) > 1) { continue; } // used to control the range of the diagonal
                    float px = i;
                    float py = Mathf.Floor(diag2Start + 0.5f + i * 0.5f);
                    if (Exists(px, py))
                        positions.Add(new Vector2(px, py));
                }

                for (int i = 0; i < positions.Count; i++)
                {
                    TileView baseTile = grid.TileViewAtPos(positions[i]);
                    if (baseTile)// && !baseTile.isBeingDestroyed)
                        toDestroy.Add(baseTile);
                }

                TileView leftMostTile = grid.TileViewAtPos(new Vector2(position.x + 2, position.y));
                if (leftMostTile != null && Exists(leftMostTile.position.x, leftMostTile.position.y) && !toDestroy.Contains(leftMostTile)) { toDestroy.Add(leftMostTile); }
                TileView rightMostTile = grid.TileViewAtPos(new Vector2(position.x - 2, position.y));
                if (rightMostTile != null && Exists(rightMostTile.position.x, rightMostTile.position.y) && !toDestroy.Contains(rightMostTile)) { toDestroy.Add(rightMostTile); }

                /*

                float newRadius = Mathf.Max(2f, radius);
                List<TileView> adjacentInRadius = grid.FindAdjacentTiles(new Vector2(position.x, position.y), newRadius);
                foreach (TileView tile in adjacentInRadius)
                {
                    if (tile)// && !tile.isBeingDestroyed)
                        if (!toDestroy.Contains(tile) && !(tile.position.x == position.x && tile.position.y == position.y))
                            toDestroy.Add(tile);
                }

                */
            }

            return toDestroy;
        }

        protected bool Exists(float x, float y)
        {
            if (x < Constants.gridXsize && x > -1 && y < Constants.gridYsize && y > -1)
                return true;
            else
                return false;
        }

        public float DistanceToTile (TileView tile)
        {
            Vector2 thisPosition = new Vector2(position.x, position.y);
            Vector2 thatPosition = new Vector2(tile.position.x, tile.position.y);

            if (thisPosition.x % 2 != 0f)
                thisPosition = new Vector2(thisPosition.x, thisPosition.y - 0.5f);
            if (thatPosition.x % 2 != 0f)
                thatPosition = new Vector2(thatPosition.x, thatPosition.y - 0.5f);

            return Vector2.Distance(thisPosition, thatPosition);
        }

        public bool IsAdjacentTo(TileView tile)
        {
            if (DistanceToTile(tile) <= Constants.DistanceBetweenTiles)
                return true;
            else
                return false;
        }

        #region SpriteRendering
        public Sprite HexSprite(TileTypes.EColor color)
        {
            return MainController.Data.sprites.GetNormalSprite(color);
        }

        public Sprite HexSpriteSelected(TileTypes.EColor color)
        {
            return MainController.Data.sprites.GetSelectedSprite(color);
        }

        public Sprite HexSpriteCollateral(TileTypes.EColor color)
        {
            return MainController.Data.sprites.GetCollateralSprite(color);
        }

        public void ToggleTileState(bool activate)
        {
            Debug.Log(activate);
            tileImage.material.SetFloat("_Saturation", (activate ? activeSaturation : deactiveSaturation));
            tileImage.material.SetFloat("_Contrast", (activate ? activeContrast : activeSaturation));
        }
        #endregion
    }
}
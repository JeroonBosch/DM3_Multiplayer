using UnityEngine;
using UnityEngine.UI;
using Lean.Touch;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Com.Hypester.DM3
{
    public class PlayGameCanvas : BaseMenuCanvas
    {

        #region private variables
        List<Vector2> _selectedTiles;
        LeanFinger _finger;
        GameHandler _game;
        #endregion

        protected override void Start()
        {
            base.Start();
            _selectedTiles = new List<Vector2>();
            _game = GameObject.Find("Grid").GetComponent<GameHandler>();
        }

        protected override void Update()
        {
            base.Update();
            if (_selectedTiles.Count > 0)
            {
                Vector2 vec = FindNearestTileToFinger();
                if (!_selectedTiles.Contains(vec) && _game.TileAtPos(new Vector2(vec.x, vec.y)).color == _game.TileAtPos(new Vector2(_selectedTiles[0].x, _selectedTiles[0].y)).color && DistanceToBetweenPos(vec, _selectedTiles[_selectedTiles.Count-1]) < 1.5f)
                {
                    _selectedTiles.Add(vec);
                    NewSelectedTile(vec);
                }
            }

            if (_finger != null)
                if (GameObject.Find("FingerTracker")) {
                    Transform tf = GameObject.Find("FingerTracker").transform;
                    tf.position = _finger.GetWorldPosition(1f);
                    tf.localPosition = new Vector2(-tf.localPosition.x, -tf.localPosition.y);
                }
        }

        public override void Show()
        {
            base.Show();

            foreach (Player player in FindObjectsOfType<Player>())
                player.transform.SetParent(transform, false);

            _game.Show();
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
                GameObject interactionObject = null;

                GraphicRaycaster gRaycast = GetComponent<GraphicRaycaster>();
                PointerEventData ped = new PointerEventData(null);
                ped.position = finger.GetSnapshotScreenPosition(1f);
                List<RaycastResult> results = new List<RaycastResult>();
                gRaycast.Raycast(ped, results);

                if (results != null && results.Count > 0)
                {
                    bool resultFound = false;
                    for (int i = 0; i < results.Count; i++)
                    {
                        if (!resultFound)
                            if (results[i].gameObject.tag == "Tile")
                                interactionObject = results[i].gameObject;
                    }
                }

                if (interactionObject)
                {
                    if (interactionObject.tag == "Tile")
                    {
                        _selectedTiles.Add(interactionObject.GetComponent<BaseTile>().position);
                        _finger = finger;
                        _game.MyPlayer.NewSelection(_selectedTiles[0]);
                    }
                }
            }
        }

        void OnFingerUp(LeanFinger finger)
        {
            if (finger.Index == 0)
            {
                if (_selectedTiles.Count > 2) {
                    _game.MyPlayer.InitiateCombo();
                    Debug.Log("Combo time");
                } else
                {
                    _game.MyPlayer.RemoveAllSelections();
                }
                _selectedTiles.Clear();
                _finger = null;
            }
        }

        private Vector2 FindNearestTileToFinger()
        {
            Vector2 tilePos = new Vector2();

            float minDist = Mathf.Infinity;
            Vector3 currentPos = _finger.GetWorldPosition(1f);
            foreach (BaseTile tile in FindObjectsOfType<BaseTile>())
            {
                float dist = Vector3.Distance(tile.transform.position, currentPos);
                if (dist < minDist)
                {
                    tilePos = tile.position;
                    minDist = dist;
                }
            }

            return tilePos;
        }

        private float DistanceToBetweenPos(Vector2 position, Vector2 position2)
        {
            BaseTile newTile = _game.BaseTileAtPos(position);
            BaseTile prevTile = _game.BaseTileAtPos(position2);

            return Vector3.Distance(newTile.position, prevTile.position);
        }

        void NewSelectedTile (Vector2 position)
        {
            //BaseTile newTile = _game.BaseTileAtPos(position);
            //int prevIndex = _selectedTiles.IndexOf(position) - 1;
            //BaseTile prevTile = _game.BaseTileAtPos(_selectedTiles[prevIndex]);
            _game.MyPlayer.NewSelection(position);
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
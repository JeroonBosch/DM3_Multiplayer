using UnityEngine;
using UnityEngine.UI;
using Lean.Touch;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Com.Hypester.DM3
{
    public class PlayUI : MonoBehaviour
    {

        #region private variables
        List<Vector2> _selectedTiles;
        LeanFinger _finger;
        #endregion

        void Start()
        {
            _selectedTiles = new List<Vector2>();
        }

        void Update()
        {
            if (_selectedTiles.Count > 0)
            {
                Debug.Log("Tracking finger.");
                Vector2 vec = FindNearestTileToFinger();
                if (!_selectedTiles.Contains(vec))
                    _selectedTiles.Add(vec);
            }

            foreach (BaseTile tile in FindObjectsOfType<BaseTile>())
            {
                int color = GameObject.Find("Grid").GetComponent<GameHandler>().GetTileAtPosition(new Vector2 ((int)tile.position.x, (int)tile.position.y)).color;
                tile.GetComponent<Image>().sprite = HexSprite(TileTypes.EColor.yellow + color);
                if (_selectedTiles.Contains(tile.position))
                {
                    tile.GetComponent<Image>().sprite = HexSpriteSelected(TileTypes.EColor.yellow + color);
                }
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
            Debug.Log("Click.");
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
                        Debug.Log("Should be tracking finger.");
                    }
                }
            }
        }

        void OnFingerUp(LeanFinger finger)
        {
            if (finger.Index == 0)
            {
                _selectedTiles.Clear();
                _finger = null;
            }
        }

        private Vector2 FindNearestTileToFinger()
        {
            Vector2 tilePos = new Vector2();

            float minDist = Mathf.Infinity;
            Vector3 currentPos = _finger.GetWorldPosition(1f, Camera.current);
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
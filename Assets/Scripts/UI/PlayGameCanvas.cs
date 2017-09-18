using UnityEngine;
using UnityEngine.UI;
using Lean.Touch;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Com.Hypester.DM3
{
    public class PlayGameCanvas : BaseMenuCanvas
    {
        //This class is attached to the Canvas that also contains the grid.
        //The canvas has a 'Graphic Raycaster' required for selecting tiles.
        //Therefor this class also implements all the touch controls.

        List<Vector2> _selectedTiles;
        LeanFinger _finger;

        protected override void Start()
        {
            base.Start();

            _selectedTiles = new List<Vector2>();
        }

        protected override void Update()
        {
            base.Update();

            if (_finger != null)
            {
                if (!GameObject.FindGameObjectWithTag("ActiveFireball") && !GameObject.FindGameObjectWithTag("ActiveTrap"))
                {
                    if (_selectedTiles.Count > 0)
                    {
                        Vector2 vec = FindNearestTileToFinger();
                        //Select new tile.
                        if (!_selectedTiles.Contains(vec) && PhotonConnect.Instance.GameController.TileAtPos(new Vector2(vec.x, vec.y)).color == PhotonConnect.Instance.GameController.TileAtPos(new Vector2(_selectedTiles[0].x, _selectedTiles[0].y)).color && IsAdjacentPosition(vec, _selectedTiles[_selectedTiles.Count - 1]))
                        {
                            _selectedTiles.Add(vec);
                            NewSelectedTile(vec);
                        }
                        //Remove if already selected, plus remove all previously selected ones.
                        else if (_selectedTiles.Contains(vec) && _selectedTiles[_selectedTiles.Count - 1] != vec && IsAdjacentPosition(vec, _selectedTiles[_selectedTiles.Count - 1]))
                        {
                            int index = _selectedTiles.IndexOf(vec);
                            for (int i = index + 1; i < _selectedTiles.Count; i++)
                            {
                                RemoveSelectedTile(_selectedTiles[i]);
                                _selectedTiles.RemoveAt(i);
                            }
                        }
                    }

                    if (GameObject.Find("FingerTracker"))
                    {
                        Transform tf = GameObject.Find("FingerTracker").transform;
                        tf.position = _finger.GetWorldPosition(1f);
                        tf.localPosition = new Vector2(-tf.localPosition.x, -tf.localPosition.y);
                    }
                } else 
                {
                    if (GameObject.FindGameObjectWithTag("ActiveFireball"))
                    {
                        Transform fb = GameObject.FindGameObjectWithTag("ActiveFireball").transform;
                        if (!fb.GetComponent<YellowPower>().isFlying && fb.GetComponent<YellowPower>().isPickedUp)
                        {
                            fb.position = _finger.GetWorldPosition(1f);
                            fb.GetComponent<YellowPower>().position = fb.localPosition;
                        }
                    } else
                    {
                        Transform trap = GameObject.FindGameObjectWithTag("ActiveTrap").transform;
                        if (trap.GetComponent<TrapPower>().isPickedUp)
                        {
                            Vector2 vec = FindNearestTileToFinger();
                            BaseTile trapTile = PhotonConnect.Instance.GameController.BaseTileAtPos(vec);
                            if (trapTile != null)
                            {
                                trap.GetComponent<TrapPower>().overBasetile = trapTile;
                            }
                            trap.position = _finger.GetWorldPosition(1f);
                        }
                    }
                }
            }
        }

        public override void Show()
        {
            base.Show();
            //if (PhotonNetwork.isMasterClient) {
            foreach (GameHandler gh in FindObjectsOfType<GameHandler>()) { 
                gh.Show();
                if (gh.GameID != PhotonConnect.Instance.gameID_requested)
                    gh.Hide();
            }
            //}
            //else
            //PhotonConnect.Instance.GameController.Show(); //Activates the game.
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
            if (finger.Index == 0 && PhotonConnect.Instance.GameController.IsMyTurn())
            {
                if (!GameObject.FindGameObjectWithTag("ActiveFireball") && !GameObject.FindGameObjectWithTag("ActiveTrap"))
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
                                if (results[i].gameObject.tag == "Tile" || results[i].gameObject.tag == "Power")
                                {
                                    interactionObject = results[i].gameObject;
                                    resultFound = true;
                                }
                        }
                    }

                    if (interactionObject)
                    {
                        if (interactionObject.tag == "Tile")
                        {
                            _selectedTiles.Add(interactionObject.GetComponent<BaseTile>().position);
                            _finger = finger;
                            PhotonConnect.Instance.GameController.MyPlayer.NewSelection(_selectedTiles[0]);
                        }
                        else if (interactionObject.tag == "Power")
                        {
                            if (interactionObject.name == "MyBlue")
                                PhotonConnect.Instance.GameController.MyPlayer.PowerClicked(1);
                            if (interactionObject.name == "MyGreen")
                                PhotonConnect.Instance.GameController.MyPlayer.PowerClicked(2);
                            if (interactionObject.name == "MyRed")
                                PhotonConnect.Instance.GameController.MyPlayer.PowerClicked(3);
                            if (interactionObject.name == "MyYellow")
                                PhotonConnect.Instance.GameController.MyPlayer.PowerClicked(0);
                        }
                    }
                } else
                {
                    if (GameObject.FindGameObjectWithTag("ActiveFireball")) { 
                        YellowPower fireball = GameObject.FindGameObjectWithTag("ActiveFireball").GetComponent<YellowPower>();
                        if (fireball.GetComponent<PhotonView>().isMine)
                        {
                            fireball.PickUp();
                            _finger = finger;
                        }
                    }
                    else
                    {
                        TrapPower trap = GameObject.FindGameObjectWithTag("ActiveTrap").GetComponent<TrapPower>();
                        trap.PickUp();
                        _finger = finger;
                    }
                }
            }
        }

        void OnFingerUp(LeanFinger finger)
        {
            if (finger.Index == 0 && PhotonConnect.Instance.GameController.IsMyTurn())
            {
                if (!GameObject.FindGameObjectWithTag("ActiveFireball") && !GameObject.FindGameObjectWithTag("ActiveTrap")) { 
                    if (_selectedTiles.Count > 2) {
                        PhotonConnect.Instance.GameController.MyPlayer.InitiateCombo();
                    } else
                    {
                        PhotonConnect.Instance.GameController.MyPlayer.RemoveAllSelections();
                    }
                    _selectedTiles.Clear();
                } else
                {
                    if (GameObject.FindGameObjectWithTag("ActiveFireball"))
                    {
                        GameObject fireball = GameObject.FindGameObjectWithTag("ActiveFireball");
                        if (fireball.GetComponent<PhotonView>().isMine)
                        {
                            fireball.GetComponent<YellowPower>().Fly();
                        }
                    } else
                    {
                        TrapPower trap = GameObject.FindGameObjectWithTag("ActiveTrap").GetComponent<TrapPower>();
                        trap.GetComponent<TrapPower>().Place();
                    }
                }
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

        private float DistanceBetweenPos(Vector2 position, Vector2 position2)
        {
            BaseTile newTile = PhotonConnect.Instance.GameController.BaseTileAtPos(position);
            BaseTile prevTile = PhotonConnect.Instance.GameController.BaseTileAtPos(position2);

            return newTile.DistanceToTile(prevTile);
        }

        private bool IsAdjacentPosition(Vector2 position, Vector2 position2)
        {
            BaseTile newTile = PhotonConnect.Instance.GameController.BaseTileAtPos(position);
            BaseTile prevTile = PhotonConnect.Instance.GameController.BaseTileAtPos(position2);

            return newTile.IsAdjacentTo(prevTile);
        }

        void NewSelectedTile (Vector2 position)
        {
            PhotonConnect.Instance.GameController.MyPlayer.NewSelection(position);
        }

        void RemoveSelectedTile (Vector2 position)
        {
            PhotonConnect.Instance.GameController.MyPlayer.RemoveSelection(position);
        }

        public void EndGame (int winnerPlayer)
        {
            GoToScreen(GameObject.Find("EndScreen").GetComponent<BaseMenuCanvas>());
            GameObject.Find("EndScreen").GetComponent<EndScreenCanvas>().winnerPlayer = winnerPlayer;
            enabled = false;
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
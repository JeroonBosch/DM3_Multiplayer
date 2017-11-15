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
        //Thus, this class also implements all the touch controls and actually calls RPC's of the GameHandler.

        [SerializeField] EndScreenCanvas endScreenCanvas;
        
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
                GameObject activeTrap = GameObject.FindGameObjectWithTag("ActiveTrap");
                if (!activeTrap)
                {
                    if (_selectedTiles.Count > 0)
                    {
                        Vector2 vec = FindNearestTileToFinger();
                        //Select new tile.
                        if (!_selectedTiles.Contains(vec) && PhotonController.Instance.GameController.TileAtPos(new Vector2(vec.x, vec.y)).color == PhotonController.Instance.GameController.TileAtPos(new Vector2(_selectedTiles[0].x, _selectedTiles[0].y)).color && IsAdjacentPosition(vec, _selectedTiles[_selectedTiles.Count - 1]))
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

                    GameObject fingerTracker = GameObject.Find("FingerTracker");
                    if (fingerTracker)
                    {
                        Transform tf = fingerTracker.transform;
                        tf.position = _finger.GetWorldPosition(1f);
                        tf.localPosition = new Vector2(-tf.localPosition.x, -tf.localPosition.y);
                    }
                }
                else 
                {
                    Transform trap = activeTrap.transform;
                    if (trap.GetComponent<TrapPower>().isPickedUp)
                    {
                        Vector2 vec = FindNearestTileToFinger();
                        TileView trapTile = PhotonController.Instance.GameController.TileViewAtPos(vec);
                        if (trapTile != null)
                        {
                            trap.GetComponent<TrapPower>().overBasetile = trapTile;
                        }
                        trap.position = _finger.GetWorldPosition(1f);
                    }
                }
            }
        }

        public override void Show()
        {
            base.Show();
            foreach (GameHandler gh in FindObjectsOfType<GameHandler>()) { 
                gh.Show();
                if (gh.GameID != PhotonController.Instance.gameID_requested)
                    gh.Hide();
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
            if (finger.Index == 0 && PhotonController.Instance.GameController.IsMyTurn())
            {
                GameObject activeTrap = GameObject.FindGameObjectWithTag("ActiveTrap");
                if (!activeTrap)
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
                            _selectedTiles.Add(interactionObject.GetComponent<TileView>().position);
                            _finger = finger;
                            StartNewSelection();
                        }
                    }
                } else
                {
                    TrapPower trap = activeTrap.GetComponent<TrapPower>();
                    trap.PickUp();
                    _finger = finger;
                }
            }
        }

        void OnFingerUp(LeanFinger finger)
        {
            if (finger.Index == 0 && PhotonController.Instance.GameController.IsMyTurn())
            {
                GameObject activeTrap = GameObject.FindGameObjectWithTag("ActiveTrap");
                if (!activeTrap) { 
                    if (_selectedTiles.Count > 2) {
                        InitiateCombo();
                    } else
                    {
                        RemoveAllSelections();
                    }
                    _selectedTiles.Clear();
                } else
                {
                    TrapPower trap = activeTrap.GetComponent<TrapPower>();
                    trap.GetComponent<TrapPower>().Place();
                }
                _finger = null;
            }
        }

        private Vector2 FindNearestTileToFinger()
        {
            Vector2 tilePos = new Vector2();

            float minDist = Mathf.Infinity;
            Vector3 currentPos = _finger.GetWorldPosition(1f);
            foreach (TileView tile in FindObjectsOfType<TileView>())
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
            TileView newTile = PhotonController.Instance.GameController.TileViewAtPos(position);
            TileView prevTile = PhotonController.Instance.GameController.TileViewAtPos(position2);

            return newTile.DistanceToTile(prevTile);
        }

        private bool IsAdjacentPosition(Vector2 position, Vector2 position2)
        {
            TileView newTile = PhotonController.Instance.GameController.TileViewAtPos(position);
            TileView prevTile = PhotonController.Instance.GameController.TileViewAtPos(position2);

            return newTile.IsAdjacentTo(prevTile);
        }


        #region selections
        //All local selections, to be applied to my GameController.
        void StartNewSelection ()
        {
            //PhotonController.Instance.GameController.MyPlayer.NewSelection(_selectedTiles[0]);
            iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.SelectionChange);
            PhotonController.Instance.GameController.photonView.RPC("RPC_AddToSelection", PhotonTargets.All, _selectedTiles[0]);
        }

        void NewSelectedTile (Vector2 position)
        {
            //PhotonController.Instance.GameController.MyPlayer.NewSelection(position);
            iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.SelectionChange);
            PhotonController.Instance.GameController.photonView.RPC("RPC_AddToSelection", PhotonTargets.All, position);
        }

        void RemoveSelectedTile (Vector2 position)
        {
            //PhotonController.Instance.GameController.MyPlayer.RemoveSelection(position);
            iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.SelectionChange);
            PhotonController.Instance.GameController.photonView.RPC("RPC_RemoveFromSelection", PhotonTargets.All, position);
        }

        void RemoveAllSelections ()
        {
            //PhotonController.Instance.GameController.MyPlayer.RemoveAllSelections();
            PhotonController.Instance.GameController.photonView.RPC("RPC_RemoveSelections", PhotonTargets.All);
        }

        void InitiateCombo ()
        {
            //PhotonController.Instance.GameController.MyPlayer.InitiateCombo();
            iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.ImpactLight);
            PhotonController.Instance.GameController.photonView.RPC("RPC_InitiateCombo", PhotonTargets.All);
        }
        #endregion

        public void EndGame (int winnerPlayer)
        {
            GoToScreen(endScreenCanvas);
            endScreenCanvas.SetWinner(winnerPlayer);
            enabled = false;
        }

        public void Surrender()
        {
            UIEvent.Surrender();
        }
    }
}
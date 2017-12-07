using UnityEngine;
using UnityEngine.UI;
using Lean.Touch;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

namespace Com.Hypester.DM3
{
    public class PlayGameCanvas : BaseMenuCanvas
    {
        //This class is attached to the Canvas that also contains the grid.
        //The canvas has a 'Graphic Raycaster' required for selecting tiles.
        //Thus, this class also implements all the touch controls and actually calls RPC's of the GameHandler.

        [SerializeField] EndScreenCanvas endScreenCanvas;

        [Header("TextPrefabs")]
        [SerializeField] GameObject smallTextPopupPrefab;
        [SerializeField] GameObject bigTextPopupPrefab;

        [Header("TextLocations")]
        [SerializeField] RectTransform localAvatarTop;
        [SerializeField] RectTransform remoteAvatarBot;

        [SerializeField] GameObject gameBoardCover;

        TileView startingTile = null;
        Dictionary<Vector2, TileView> _selectedTiles;
        LeanFinger _finger;

        protected override void Start()
        {
            base.Start();

            _selectedTiles = new Dictionary<Vector2, TileView>();
        }

        protected override void Update()
        {
            base.Update();

            if (_finger != null)
            {
                GameObject activeTrap = GameObject.FindGameObjectWithTag("ActiveTrap");
                if (!activeTrap)
                {
                    if (PhotonController.Instance.GameController.IsMyTurn())
                    {
                        TileView nearestTileToFinger = FindNearestTileToFinger();
                        
                        // False if selection was made, the color is same and exists in selected tiles. True if no selection was made or if (select was made but color is different or nearest not in selection)
                        if (!(startingTile != null && _selectedTiles.Count > 0 && nearestTileToFinger.color == startingTile.color && _selectedTiles.ContainsKey(nearestTileToFinger.position)))
                        {
                            TileView.areaList.Add(nearestTileToFinger);
                            nearestTileToFinger.GetArea();
                            if (TileView.areaList.Count > 2)
                            {
                                RemoveAllSelections();
                                startingTile = nearestTileToFinger;
                                _selectedTiles.Clear();
                                _selectedTiles.Add(nearestTileToFinger.position, nearestTileToFinger);
                                foreach (TileView t in TileView.areaList)
                                {
                                    if (!_selectedTiles.ContainsKey(t.position)) { _selectedTiles.Add(t.position, t); } // TODO: The key should not be in there. Optimize.
                                }
                                StartNewSelection(nearestTileToFinger.position);
                            }
                            TileView.areaList.Clear();
                        }
                    } else if (_selectedTiles.Count > 0 && !PhotonController.Instance.GameController.IsMyTurn())
                    {
                        RemoveAllSelections();
                        _selectedTiles.Clear();
                        startingTile = null;
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
                        TileView nearestTile = FindNearestTileToFinger();
                        TileView trapTile = PhotonController.Instance.GameController.TileViewAtPos(nearestTile.position);
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

            ToggleTiles(PhotonController.Instance.GameController.IsMyTurn());
        }

        private void OnEnable()
        {
            LeanTouch.OnFingerDown += OnFingerDown;
            LeanTouch.OnFingerUp += OnFingerUp;

            UIEvent.OnTurnChange += OnTurnChange;
            UIEvent.OnBoosterTrigger += OnBoosterTrigger;
            UIEvent.OnBoosterTriggerDouble += OnBoosterTriggerDouble;
            UIEvent.OnBoosterTriggerTriple += OnBoosterTriggerTriple;
            UIEvent.OnBoosterTriggerMulti += OnBoosterTriggerMulti;
            UIEvent.OnOpponentTrapPlaced += OnOpponentTrapPlaced;
            UIEvent.OnOpponentTrapTrigger += OnOpponentTrapTrigger;
            UIEvent.OnLocalTrapTrigger += OnLocalTrapTrigger;
            UIEvent.OnShieldActivate += OnShieldActivate;
            UIEvent.OnShieldHit += OnShieldHit;
            UIEvent.OnHeal += OnHeal;
            UIEvent.OnSkillNotFull += OnSkillNotFull;
        }

        private void OnDisable()
        {
            LeanTouch.OnFingerDown -= OnFingerDown;
            LeanTouch.OnFingerUp -= OnFingerUp;

            UIEvent.OnTurnChange -= OnTurnChange;
            UIEvent.OnBoosterTrigger -= OnBoosterTrigger;
            UIEvent.OnBoosterTriggerDouble -= OnBoosterTriggerDouble;
            UIEvent.OnBoosterTriggerTriple -= OnBoosterTriggerTriple;
            UIEvent.OnBoosterTriggerMulti -= OnBoosterTriggerMulti;
            UIEvent.OnOpponentTrapPlaced -= OnOpponentTrapPlaced;
            UIEvent.OnOpponentTrapTrigger -= OnOpponentTrapTrigger;
            UIEvent.OnLocalTrapTrigger -= OnLocalTrapTrigger;
            UIEvent.OnShieldActivate -= OnShieldActivate;
            UIEvent.OnShieldHit -= OnShieldHit;
            UIEvent.OnHeal -= OnHeal;
            UIEvent.OnSkillNotFull -= OnSkillNotFull;
        }

        private void OnBoosterTriggerDouble()
        {
            Debug.Log("Triggering double!");
            TextPopup textPopup = Instantiate(bigTextPopupPrefab, this.transform, false).GetComponent<TextPopup>();
            textPopup.textImage.sprite = MainController.Data.sprites.great;
            textPopup.animator.SetTrigger("Popup");
        }
        private void OnBoosterTriggerTriple()
        {
            TextPopup textPopup = Instantiate(bigTextPopupPrefab, this.transform, false).GetComponent<TextPopup>();
            textPopup.textImage.sprite = MainController.Data.sprites.super;
            textPopup.animator.SetTrigger("Popup");
        }
        private void OnBoosterTriggerMulti()
        {
            TextPopup textPopup = Instantiate(bigTextPopupPrefab, this.transform, false).GetComponent<TextPopup>();
            textPopup.textImage.sprite = MainController.Data.sprites.fantastic;
            textPopup.animator.SetTrigger("Popup");
        }

        private void OnBoosterTrigger(Vector2 pos, int boosterLevel)
        {
            Transform parentTransform = PhotonController.Instance.GameController.transform;
            TextPopup textPopup = Instantiate(bigTextPopupPrefab, PhotonController.Instance.GameController.transform, false).GetComponent<TextPopup>();
            textPopup.textImage.sprite = MainController.Data.sprites.GetBoosterSpriteByLevel(boosterLevel);
            textPopup.transform.localPosition = pos;
            textPopup.transform.localRotation = parentTransform.localRotation;
            textPopup.animator.SetTrigger("Popup");
        }

        private void OnLocalTrapTrigger(Vector2 pos)
        {
            Transform parentTransform = PhotonController.Instance.GameController.transform;
            TextPopup textPopup = Instantiate(bigTextPopupPrefab, PhotonController.Instance.GameController.transform, false).GetComponent<TextPopup>();
            textPopup.textImage.sprite = MainController.Data.sprites.trapTriggeredOuch;
            textPopup.transform.localPosition = pos;
            textPopup.transform.localRotation = parentTransform.localRotation;
            textPopup.animator.SetTrigger("Popup");
        }

        private void OnOpponentTrapTrigger(Vector2 pos)
        {
            Transform parentTransform = PhotonController.Instance.GameController.transform;
            TextPopup textPopup = Instantiate(bigTextPopupPrefab, PhotonController.Instance.GameController.transform, false).GetComponent<TextPopup>();
            textPopup.textImage.sprite = MainController.Data.sprites.trapTriggered;
            textPopup.transform.localPosition = pos;
            textPopup.transform.localRotation = parentTransform.localRotation;
            textPopup.animator.SetTrigger("Popup");
        }

        private void OnOpponentTrapPlaced()
        {
            TextPopup textPopup = Instantiate(bigTextPopupPrefab, this.transform, false).GetComponent<TextPopup>();
            textPopup.textImage.sprite = MainController.Data.sprites.opponentPlacedTrap;
            textPopup.animator.SetTrigger("Popup");
        }

        private void OnSkillNotFull(SkillColor color)
        {
            Vector2 pos = PhotonController.Instance.GameController.MyPlayer.playerInterface.GetSkillButtonBySkillColor(color).textPopupTransform.position;
            TextPopup textPopup = Instantiate(smallTextPopupPrefab, this.transform, false).GetComponent<TextPopup>();
            textPopup.textImage.sprite = MainController.Data.sprites.notFull;
            textPopup.transform.position = pos;
            textPopup.animator.SetTrigger("Popup");
        }

        private void OnTurnChange(bool localPlayer)
        {
            TextPopup textPopup = Instantiate(bigTextPopupPrefab, this.transform, false).GetComponent<TextPopup>();
            textPopup.textImage.sprite = localPlayer ? MainController.Data.sprites.localTurn : MainController.Data.sprites.opponentTurn;
            textPopup.animator.SetTrigger("Popup");

            ToggleTiles(localPlayer);

            PlayerInterface playerInterface = localPlayer ? PhotonController.Instance.GameController.MyPlayer.playerInterface : PhotonController.Instance.GameController.EnemyPlayer.playerInterface;
            if (playerInterface != null)
            {
                playerInterface.avatarImageAnimator.SetTrigger("Wobble");
                playerInterface.avatarBorderAnimator.SetTrigger("Wobble");
            }
        }

        private void OnShieldActivate(bool localPlayer)
        {
            Vector2 pos = localPlayer ? localAvatarTop.position : remoteAvatarBot.position;
            TextPopup textPopup = Instantiate(bigTextPopupPrefab, this.transform, false).GetComponent<TextPopup>();
            textPopup.transform.position = pos;
            textPopup.textImage.sprite = MainController.Data.sprites.shieldActivated;
            textPopup.animator.SetTrigger("Popup");
        }
        private void OnShieldHit(bool localPlayer)
        {
            Vector2 pos = localPlayer ? localAvatarTop.position : remoteAvatarBot.position;
            TextPopup textPopup = Instantiate(bigTextPopupPrefab, this.transform, false).GetComponent<TextPopup>();
            textPopup.transform.position = pos;
            textPopup.textImage.sprite = MainController.Data.sprites.shieldBlocked;
            textPopup.animator.SetTrigger("Popup");
        }
        private void OnHeal(bool localPlayer)
        {
            Vector2 pos = localPlayer ? localAvatarTop.position : remoteAvatarBot.position;
            TextPopup textPopup = Instantiate(bigTextPopupPrefab, this.transform, false).GetComponent<TextPopup>();
            textPopup.transform.position = pos;
            textPopup.textImage.sprite = MainController.Data.sprites.healed;
            textPopup.animator.SetTrigger("Popup");
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

                    if (results.Count > 0)
                    {
                        foreach (RaycastResult result in results)
                        {
                            if (result.gameObject.tag == "Tile") { interactionObject = result.gameObject; break; }
                        }
                    }

                    if (interactionObject)
                    {
                        if (interactionObject.tag == "Tile")
                        {
                            TileView tv = interactionObject.GetComponent<TileView>();
                            TileView.areaList.Add(tv);
                            tv.GetArea();
                            if (TileView.areaList.Count > 2)
                            {
                                startingTile = tv;
                                if (!_selectedTiles.ContainsKey(tv.position)) { _selectedTiles.Add(tv.position, tv); }
                                foreach (TileView t in TileView.areaList)
                                {
                                    if (!_selectedTiles.ContainsKey(t.position)) { _selectedTiles.Add(t.position, t); } // TODO: The key should not be in there. Optimize this.
                                }
                                StartNewSelection(tv.position);
                            }
                            TileView.areaList.Clear();
                            _finger = finger;
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
                    if (_selectedTiles.Count > 2 && startingTile != null) {
                        InitiateCombo();
                    } else
                    {
                        RemoveAllSelections();
                    }
                    startingTile = null;
                    _selectedTiles.Clear();
                } else
                {
                    TrapPower trap = activeTrap.GetComponent<TrapPower>();
                    trap.GetComponent<TrapPower>().Place();
                }
                _finger = null;
            }
            if (!PhotonController.Instance.GameController.IsMyTurn())
            {
                RemoveAllSelections();
                _selectedTiles.Clear();
                startingTile = null;
            }
        }

        private TileView FindNearestTileToFinger()
        {
            TileView nearestTile = null;

            float minDist = Mathf.Infinity;
            Vector3 currentPos = _finger.GetWorldPosition(1f);
            foreach (TileView tile in FindObjectsOfType<TileView>())
            {
                float dist = Vector3.Distance(tile.transform.position, currentPos);
                if (dist < minDist)
                {
                    nearestTile = tile;
                    minDist = dist;
                }
            }

            return nearestTile;
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

        void ToggleTiles(bool activate)
        {
            if (PhotonController.Instance == null || PhotonController.Instance.GameController == null) { return; }
            foreach (KeyValuePair<Vector2, TileView> kvp in PhotonController.Instance.GameController.GetAllBaseTiles())
            {
                TileView tile = kvp.Value; if (tile == null) { continue; }
                tile.ToggleTileState(activate);
            }
        }


        #region selections
        //All local selections, to be applied to my GameController.
        void StartNewSelection (Vector2 startingTilePos)
        {
            //PhotonController.Instance.GameController.MyPlayer.NewSelection(_selectedTiles[0]);
            iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.SelectionChange);
            PhotonController.Instance.GameController.photonView.RPC("RPC_AddToSelection", PhotonTargets.All, startingTilePos);
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
            PhotonController.Instance.GameController.photonView.RPC("RPC_InitiateCombo", PhotonTargets.All, startingTile.position);
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
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using Photon;
using ExitGames.Client.Photon;
using Lean.Touch;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Com.Hypester.DM3
{
    public class GameHandler : Photon.MonoBehaviour, IPunObservable
    {
        #region private variables
        private bool _isActive;
        public bool Active { get { return _isActive; } }
        private Grid _grid;
        private int _curPlayer;
        private List<BaseTile> _baseTiles;
        private Player _myPlayer;
        private Player _enemyPlayer;
        private List<Vector2> _selectedTiles;
        private List<Vector2> _collateralTiles;

        private bool _gridReceived = false;
        private bool _gridVisualized = false;
        private bool _gameDone = false;

        private GameContext _gameContext;

        private float _timer = 0f; //Generic timer.

        private bool _rematchRequested = false;
        #endregion

        #region public game logic
        public Player MyPlayer { get { return _myPlayer; } }
        public Player EnemyPlayer { get { return _enemyPlayer; } }

        public GameStates gameState;

        public float healthPlayerOne;
        public float healthPlayerTwo;
        public float turnTimer;

        //powers
        public int P1_PowerBlue = 0;
        public int P1_PowerGreen = 0;
        public int P1_PowerRed = 0;
        public int P1_PowerYellow = 0;
        public int P2_PowerBlue = 0;
        public int P2_PowerGreen = 0;
        public int P2_PowerRed = 0;
        public int P2_PowerYellow = 0;

        public bool P1_ShieldActive = false;
        public bool P2_ShieldActive = false;

        private bool _MC_endTurnDelay; //Master Client only.
        private float _MC_endTurnDelayTimer; //Master Client only.
        #endregion

        public delegate byte[] SerializeMethod(object customObject);
        public delegate object DeserializeMethod(byte[] serializedCustomObject);

        private void Start()
        {
            PhotonPeer.RegisterType(typeof(Tile), (byte)'L', SerializeTile, DeserializeTile);
            PhotonPeer.RegisterType(typeof(AnimateTile), (byte)'A', SerializeAnimTile, DeserializeAnimTile);
            PhotonPeer.RegisterType(typeof(Grid), (byte)'G', SerializeGrid, DeserializeGrid);

            _isActive = false;
            _gameDone = false;
            _selectedTiles = new List<Vector2>();
            _curPlayer = 0;

            turnTimer = 0;
            healthPlayerOne = Constants.PlayerStartHP;
            healthPlayerTwo = Constants.PlayerStartHP;

            _grid = new Grid();

            gameState = new GameStates();
            SetGameState(GameStates.EGameState.search);

            _gameContext = GameObject.Find("GameContext").GetComponent<GameContext>();

            if (PhotonNetwork.isMasterClient)
            {
                GenerateGrid();
            }
        }

        private void Update()
        {
            if (_isActive)
            {
                if (_gridReceived && !_gridVisualized)
                    VisualizeGrid();

                if (PhotonNetwork.isMasterClient)
                {
                    if (_MC_endTurnDelay)
                        _MC_endTurnDelayTimer += Time.deltaTime;

                    //bool healthDoneDropping = false;
                    //if (MyPlayer.FindInterface().GetShownHitpoints() <= MyPlayer.GetHealth() && EnemyPlayer.FindInterface().GetShownHitpoints() <= EnemyPlayer.GetHealth())
                    //    healthDoneDropping = true;
                    

                    if (_MC_endTurnDelayTimer > Constants.TimeBetweenTurns) {
                        if (!_gameDone)
                        {
                            Refill();
                            _MC_endTurnDelay = false;
                            _MC_endTurnDelayTimer = 0f;

                            if (_curPlayer == 0)
                                _curPlayer = 1;
                            else
                                _curPlayer = 0;
                            photonView.RPC("RPC_TurnEnded", PhotonTargets.All, _curPlayer);
                        }
                        else {
                            int winnerPlayer = 0;
                            if (healthPlayerOne <= 0)
                                winnerPlayer = 1;

                            photonView.RPC("RPC_EndGame", PhotonTargets.All, winnerPlayer);
                        }
                    }

                    if (gameState.State == GameStates.EGameState.inTurn && _baseTiles != null && GridHasEmptyTiles())
                    {
                        Debug.Log("Missing some tiles.");
                        GridUpdate();
                    }
                } else
                {
                    if (gameState.State == GameStates.EGameState.inTurn && _baseTiles != null && GridHasEmptyTiles())
                    {
                        photonView.RPC("RPC_RequestGridData", PhotonTargets.MasterClient);
                    }
                }
            }

            RematchUpdate();
        }

        private void RematchUpdate()
        {
            if (_rematchRequested) { 
                _timer += Time.deltaTime;

                if (_timer > 2f)
                {
                    _rematchRequested = false;
                    _timer = 0f;
                    PhotonNetwork.LoadLevel("NormalGame");
                }
            }
        }

        private void FixedUpdate()
        {
            //Do timer stuff in Fixed update, for consistency.
            if (_isActive && gameState.State == GameStates.EGameState.inTurn)
            {
                if (turnTimer > Constants.TurnTime)
                {
                    EndTurn();

                    if (GetPlayerByID(_curPlayer) == MyPlayer)
                        _gameContext.ShowText("You ran out of time! Turn skipped.");
                }
                else
                    turnTimer += Time.fixedDeltaTime;
            } else
                turnTimer = 0f;
        }

        public void Show()
        {
            if (!_isActive)
            {
                _isActive = true;
                SetGameState(GameStates.EGameState.matchFound);

                Player[] players = FindObjectsOfType<Player>();
                foreach (Player player in players)
                {
                    if (player.GetComponent<PhotonView>().isMine)
                        _myPlayer = player;
                    else
                        _enemyPlayer = player;
                }


                if (_gridReceived)
                    VisualizeGrid();

                PhotonView photonView = PhotonView.Get(this);
                if (PhotonNetwork.isMasterClient)
                {
                    photonView.RPC("RPCSendGridData", PhotonTargets.All, _grid);
                }


                if (_myPlayer.localID == 1 && !PhotonNetwork.isMasterClient)
                {
                    transform.rotation = new Quaternion(0f, 0f, 180f, transform.rotation.w);
                } else
                {
                    transform.rotation = new Quaternion(0f, 0f, 0f, transform.rotation.w);
                }
                PlayerInterface[] interfaces = FindObjectsOfType<PlayerInterface>();
                foreach (PlayerInterface playerInterface in interfaces)
                {
                    playerInterface.SetAvatars();
                }

                _myPlayer.Reset();
                _enemyPlayer.Reset();
            }
        }

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (_isActive)
            {
                if (stream.isWriting)
                {
                    stream.SendNext(_curPlayer);
                    stream.SendNext(healthPlayerOne);
                    stream.SendNext(healthPlayerTwo);
                }
                else
                {
                    _curPlayer = (int)stream.ReceiveNext();
                    float prevHealthOne = healthPlayerOne;
                    healthPlayerOne = (float)stream.ReceiveNext();
                    float prevHealthTwo = healthPlayerTwo;
                    healthPlayerTwo = (float)stream.ReceiveNext();

                    if (prevHealthOne != healthPlayerOne) 
                    {
                        float difference = healthPlayerOne - prevHealthOne;
                        string diffString = difference.ToString();
                        if (difference > 0)
                            diffString = "+" + difference.ToString();

                        photonView.RPC("RPC_HealthChanged", PhotonTargets.MasterClient, 0, diffString);
                        if (MyPlayer.localID == 0)
                            _gameContext.ShowMyText(difference.ToString());
                        else
                            _gameContext.ShowEnemyText(difference.ToString());
                    }
                    if (prevHealthTwo != healthPlayerTwo)
                    {
                        float difference = healthPlayerTwo - prevHealthTwo;
                        string diffString = difference.ToString();
                        if (difference > 0)
                            diffString = "+" + difference.ToString();

                        photonView.RPC("RPC_HealthChanged", PhotonTargets.MasterClient, 1, diffString);
                        if (MyPlayer.localID == 1)
                            _gameContext.ShowMyText(difference.ToString());
                        else
                            _gameContext.ShowEnemyText(difference.ToString());
                    }
                }
            }
        }

        private Player GetPlayerByID(int number)
        {
            foreach (Player player in FindObjectsOfType<Player>())
            {
                if (player.localID == number)
                    return player;
            }
            return null;
        }

        private Player GetNextPlayer(int number)
        {
            foreach (Player player in FindObjectsOfType<Player>())
            {
                if (player.localID != number)
                    return player;
            }
            return null;
        }

        [PunRPC]
        private void RPC_HealthChanged (int targetPlayer, string difference)
        {
            if (targetPlayer == 0)
            {
                if (MyPlayer.localID == 0)
                    _gameContext.ShowMyText(difference);
                else
                    _gameContext.ShowEnemyText(difference);
            } else
            {
                if (MyPlayer.localID == 1)
                    _gameContext.ShowMyText(difference);
                else
                    _gameContext.ShowEnemyText(difference);
            }
        }

        private void SetGameState (GameStates.EGameState state)
        {
            if (PhotonNetwork.isMasterClient)
            {
                gameState.State = state;
                photonView.RPC("RPC_SyncState", PhotonTargets.Others, (int)state);
            }
        }

        [PunRPC]
        private void RPC_SyncState (int stateNo)
        {
            gameState.State = GameStates.EGameState.search + stateNo;
        }

        #region Grid functions
        void GenerateGrid()
        {
            _gridReceived = true;
            Debug.Log("Grid generated (by master client)");

            _grid.data = new Tile[Constants.gridXsize, Constants.gridYsize];

            for (int x = 0; x < Constants.gridXsize; x++)
            {
                for (int y = 0; y < Constants.gridYsize; y++)
                {
                    _grid.data[x, y] = new Tile();
                    _grid.data[x, y].boosterLevel = 0;
                    _grid.data[x, y].color = UnityEngine.Random.Range(0, Constants.AmountOfColors);
                    _grid.data[x, y].x = x;
                    _grid.data[x, y].y = y;
                }
            }
        }

        void VisualizeGrid()
        {
            _gridVisualized = true;
            Debug.Log("Grid visualized. (done once)");

            foreach (Transform child in transform)
                Destroy(child.gameObject);

            _baseTiles = new List<BaseTile>();

            for (int x = 0; x < Constants.gridXsize; x++)
            {
                for (int y = 0; y < Constants.gridYsize; y++)
                {
                    GameObject tile = Instantiate(Resources.Load("Tiles/Tile")) as GameObject;
                    tile.name = "Tile (" + x + "," + y + ")";
                    tile.transform.SetParent(transform, false);
                    tile.GetComponent<BaseTile>().position = new Vector2(x, y);
                    _baseTiles.Add(tile.GetComponent<BaseTile>());
                    if (x % 2 == 0)
                        tile.transform.localPosition = new Vector3((-Constants.gridXsize / 2 + x) * Constants.tileWidth + Constants.tileWidth / 2, (-Constants.gridYsize / 2 + y) * Constants.tileHeight + (Constants.tileHeight * .75f), 0f);
                    else
                        tile.transform.localPosition = new Vector3((-Constants.gridXsize / 2 + x) * Constants.tileWidth + Constants.tileWidth / 2, (-Constants.gridYsize / 2 + y) * Constants.tileHeight + (Constants.tileHeight * 0.25f), 0f);

                    if (_grid.data[x, y].color < Constants.AmountOfColors)
                    {
                        tile.GetComponent<Image>().enabled = true;
                        tile.GetComponent<BaseTile>().color = _grid.data[x, y].color;
                        tile.GetComponent<BaseTile>().SetSelected = false;

                        //tile.GetComponent<Image>().sprite = HexSprite(TileTypes.EColor.yellow + _grid.data[x, y].color);
                    }
                    else
                        tile.GetComponent<Image>().enabled = false;
                }
            }
            SetGameState(GameStates.EGameState.inTurn);
        }

        void GridUpdate()
        {
            if (_gridVisualized)
            {
                Debug.Log("Grid updated.");

                for (int x = 0; x < Constants.gridXsize; x++)
                {
                    for (int y = 0; y < Constants.gridYsize; y++)
                    {
                        Vector2 pos = new Vector2(x, y);
                        BaseTile tile = BaseTileAtPos(pos);
                        if (tile != null)
                        {
                            if (_grid.data[x, y].color < Constants.AmountOfColors)
                            {
                                tile.GetComponent<Image>().enabled = true;
                                //tile.GetComponent<Image>().sprite = HexSprite(TileTypes.EColor.yellow + _grid.data[x, y].color);
                                tile.color = _grid.data[x, y].color;
                                tile.boosterLevel = _grid.data[x, y].boosterLevel;
                                tile.SetSelected = false;
                            }
                            else { 
                                tile.GetComponent<Image>().enabled = false;
                            }
                        }
                    }
                }
                SetGameState(GameStates.EGameState.inTurn);
            }
        }

        private bool GridHasEmptyTiles()
        {
            bool hasEmptyTiles = false;
            for (int x = 0; x < Constants.gridXsize; x++)
            {
                for (int y = 0; y < Constants.gridYsize; y++)
                {
                    BaseTile baseTile = BaseTileAtPos(new Vector2(x, y));
                    if (baseTile.color >= Constants.AmountOfColors)
                        hasEmptyTiles = true;
                }
            }
            return hasEmptyTiles;
        }

        //Refill function
        private void Refill()
        {
            Debug.Log("---- Refilling ----");
            Grid dupli = DuplicateGrid();

            if (_curPlayer == 0)
            {
                dupli = ShiftUpGrid(dupli);
                dupli = RefillGridFromBelow(dupli);
            }
            else
            {
                dupli = ShiftDownGrid(dupli);
                dupli = RefillGridFromAbove(dupli);
            }

            ApplyGrid(dupli);

            photonView.RPC("RPCSendGridData", PhotonTargets.All, _grid);
        }

        private Grid DuplicateGrid()
        {
            Grid dupli = new Grid();
            dupli.data = new Tile[Constants.gridXsize, Constants.gridYsize];
            for (int x = 0; x < Constants.gridXsize; x++)
            {
                int topRow = Constants.gridYsize - 1;
                for (int y = topRow; y >= 0; y--)
                {
                    dupli.data[x, y] = new Tile();
                    dupli.data[x, y].color = _grid.data[x, y].color;
                    dupli.data[x, y].boosterLevel = _grid.data[x, y].boosterLevel;
                }
            }
            return dupli;
        }

        //Normal 'gravity' (player 0)
        private Grid ShiftDownGrid(Grid dupli)
        {
            for (int x = 0; x < Constants.gridXsize; x++)
            {
                int topRow = Constants.gridYsize - 1;
                for (int y = topRow; y >= 0; y--)
                {

                    Vector2 pos = new Vector2(x, y);
                    Tile tile = TileAtPos(pos);
                    int colorToAssume = tile.color;
                    int boosterToAssume = tile.boosterLevel;

                    //Checking only for non-destroyed tiles.
                    if (colorToAssume < Constants.AmountOfColors)
                    {
                        int emptyTilesBelow = EmptyTilesBelow(x, y);

                        //If there's empty spaces below
                        if (emptyTilesBelow > 0)
                        {
                            Vector2 dropIntoPos = new Vector2(x, y - emptyTilesBelow);
                            float dropDistance = emptyTilesBelow * Constants.tileHeight;

                            dupli.data[(int)dropIntoPos.x, (int)dropIntoPos.y].color = colorToAssume; //Set color of the target position to this tile.
                            if (dupli.data[(int)dropIntoPos.x, (int)dropIntoPos.y].boosterLevel < boosterToAssume)
                                dupli.data[(int)dropIntoPos.x, (int)dropIntoPos.y].boosterLevel = boosterToAssume; //Set booster of the target position to this tile.

                            if (y + emptyTilesBelow <= topRow)
                            { //Possible to take color from above?
                                dupli.data[x, y].color = _grid.data[x, y + emptyTilesBelow].color;
                                dupli.data[x, y].boosterLevel = _grid.data[x, y + emptyTilesBelow].boosterLevel;
                            }
                            else
                            {
                                dupli.data[x, y].color = Constants.AmountOfColors;
                                dupli.data[x, y].boosterLevel = 0;
                            }

                            AnimateTile anim = new AnimateTile(colorToAssume, dropDistance, (int)dropIntoPos.x, (int)dropIntoPos.y);
                            photonView.RPC("RPCAnimateTile", PhotonTargets.All, anim);
                        }
                    }
                }
            }
            return dupli;
        }

        private Grid RefillGridFromAbove(Grid dupli)
        {
            for (int x = 0; x < Constants.gridXsize; x++)
            {
                int topRow = Constants.gridYsize - 1;
                for (int y = topRow; y >= 0; y--)
                {
                    Tile tile = dupli.data[x, y];

                    //Replace destroyed tiles
                    if (tile.color >= Constants.AmountOfColors)
                    {
                        float dropDistance = y * Constants.tileHeight;
                        int color = UnityEngine.Random.Range(0, Constants.AmountOfColors);
                        dupli.data[x, y].color = color;
                        dupli.data[x, y].boosterLevel = 0;
                        AnimateTile anim = new AnimateTile(color, dropDistance, x, y);
                        photonView.RPC("RPCAnimateTile", PhotonTargets.All, anim);
                    }
                }
            }
            return dupli;
        }

        //Reverse 'gravity' (player 1)
        private Grid ShiftUpGrid(Grid dupli)
        {
            for (int x = 0; x < Constants.gridXsize; x++)
            {
                int topRow = Constants.gridYsize - 1;
                for (int y = 0; y <= topRow; y++)
                {

                    Vector2 pos = new Vector2(x, y);
                    Tile tile = TileAtPos(pos);
                    int colorToAssume = tile.color;
                    int boosterToAssume = tile.boosterLevel;

                    //Checking only for non-destroyed tiles.
                    if (colorToAssume < Constants.AmountOfColors)
                    {
                        int emptyTilesAbove = EmptyTilesAbove(x, y);

                        //If there's empty spaces below
                        if (emptyTilesAbove > 0)
                        {
                            Vector2 dropIntoPos = new Vector2(x, y + emptyTilesAbove);
                            float dropDistance = -1f * emptyTilesAbove * Constants.tileHeight;

                            dupli.data[(int)dropIntoPos.x, (int)dropIntoPos.y].color = colorToAssume; //Set color of the target position to this tile.
                            if (dupli.data[(int)dropIntoPos.x, (int)dropIntoPos.y].boosterLevel < boosterToAssume)
                                dupli.data[(int)dropIntoPos.x, (int)dropIntoPos.y].boosterLevel = boosterToAssume; //Set booster of the target position to this tile.

                            if (y - emptyTilesAbove >= 0)
                            {
                                dupli.data[x, y].color = _grid.data[x, y - emptyTilesAbove].color;
                                dupli.data[x, y].boosterLevel = _grid.data[x, y - emptyTilesAbove].boosterLevel;
                            }
                            else
                            {
                                dupli.data[x, y].color = Constants.AmountOfColors;
                                dupli.data[x, y].boosterLevel = 0;
                            }

                            AnimateTile anim = new AnimateTile(colorToAssume, dropDistance, (int)dropIntoPos.x, (int)dropIntoPos.y);
                            photonView.RPC("RPCAnimateTile", PhotonTargets.All, anim);
                        }
                    }
                }
            }
            return dupli;
        }

        private Grid RefillGridFromBelow(Grid dupli)
        {
            for (int x = 0; x < Constants.gridXsize; x++)
            {
                int topRow = Constants.gridYsize - 1;
                for (int y = 0; y <= topRow; y++)
                {
                    Tile tile = dupli.data[x, y];

                    //Replace destroyed tiles
                    if (tile.color >= Constants.AmountOfColors)
                    {
                        float dropDistance = -1f * topRow * Constants.tileHeight;
                        int color = UnityEngine.Random.Range(0, Constants.AmountOfColors);
                        dupli.data[x, y].color = color;
                        dupli.data[x, y].boosterLevel = 0;
                        AnimateTile anim = new AnimateTile(color, dropDistance, x, y);
                        photonView.RPC("RPCAnimateTile", PhotonTargets.All, anim);
                    }
                }
            }
            return dupli;
        }

        private void ApplyGrid(Grid gridToApply)
        {
            for (int x = 0; x < Constants.gridXsize; x++)
            {
                int topRow = Constants.gridYsize - 1;
                for (int y = topRow; y >= 0; y--)
                {
                    _grid.data[x, y].color = gridToApply.data[x, y].color;
                    _grid.data[x, y].boosterLevel = gridToApply.data[x, y].boosterLevel;
                }
            }
        }

        private int EmptyTilesBelow(int x, int y)
        {
            int startY = y - 1;
            int tileCount = 0;
            for (int i = startY; i >= 0; i--) //bottom row does not need shifting down. If there's 8 rows (0 to 7), then y should be 1 to 7, as row 0 is to be ignored.
            {
                Tile tile = TileAtPos(new Vector2(x, i));
                if (tile.color >= Constants.AmountOfColors) // Means it's destroyed.
                {
                    tileCount++;
                }
            }

            return tileCount;
        }

        private int EmptyTilesAbove(int x, int y)
        {
            int startY = y + 1;
            int tileCount = 0;
            int topRow = Constants.gridYsize - 1;
            for (int i = startY; i <= topRow; i++) //bottom row does not need shifting down. If there's 8 rows (0 to 7), then y should be 1 to 7, as row 0 is to be ignored.
            {
                Tile tile = TileAtPos(new Vector2(x, i));
                if (tile.color >= Constants.AmountOfColors) // Means it's destroyed.
                {
                    tileCount++;
                }
            }

            return tileCount;
        }

        public Tile TileAtPos(Vector2 position)
        {
            return _grid.data[(int)position.x, (int)position.y];
        }

        public BaseTile BaseTileAtPos(Vector2 position)
        {
            return _baseTiles.Find(item => item.position.x == position.x && item.position.y == position.y); ;
        }

        [PunRPC]
        public void RPCSendTile(Tile tile)
        {
            _grid.data[tile.x, tile.y].color = tile.color;
            _grid.data[tile.x, tile.y].boosterLevel = tile.boosterLevel;
            BaseTileAtPos(new Vector2(tile.x, tile.y)).boosterLevel = tile.boosterLevel;
            //GridUpdate();
        }

        [PunRPC]
        public void RPC_RequestGridData()
        {
            Debug.Log("Grid did not render properly, requesting update.");
            photonView.RPC("RPCSendGridData", PhotonTargets.Others, _grid);
        }

        [PunRPC]
        public void RPCSendGridData(Grid grid)
        {
            _gridReceived = true;
            _grid.data = grid.data;
            GridUpdate();
        }
        #endregion

        private void EndTurn() //both master-client and guest-client execute this.
        {
            turnTimer = 0f;
            gameState.State = GameStates.EGameState.interim;

            if (GameObject.FindGameObjectWithTag("ActiveFireball"))
            {
                Destroy(GameObject.FindGameObjectWithTag("ActiveFireball"));
            }
            if (GameObject.FindGameObjectWithTag("ActiveTrap"))
            {
                Destroy(GameObject.FindGameObjectWithTag("ActiveTrap"));
            }

            //Master client will sync this.
            if (PhotonNetwork.isMasterClient)
            {
                _MC_endTurnDelay = true;
                _MC_endTurnDelayTimer = 0f;
            }
        }

        public void ResetTimer ()
        {
            turnTimer = 0f;
            photonView.RPC("RPC_ResetTimer", PhotonTargets.Others);
        }

        [PunRPC]
        private void RPC_ResetTimer ()
        {
            turnTimer = 0f;
        }

        public void AddToSelection(Vector2 pos)
        {
            _selectedTiles.Add(pos);
            BaseTileAtPos(pos).SetSelected = true;

            RecalculateCollateral();
            RecalculateDamage();
        }

        public void RemoveFromSelection(Vector2 pos)
        {
            _selectedTiles.Remove(pos);
            BaseTileAtPos(pos).SetSelected = false;

            if (BaseTileAtPos(pos).boosterLevel > 0)
                RecalculateCollateral();
            RecalculateDamage();
        }

        public void RemoveSelections()
        {
            _selectedTiles.Clear();
            foreach (BaseTile tile in FindObjectsOfType<BaseTile>())
            {
                tile.SetSelected = false;
                tile.collateral = false;
            }

            RecalculateCollateral();
            //RecalculateDamage();
        }

        private void RecalculateCollateral ()
        {
            List<BaseTile> collateral = new List<BaseTile>();

            //Reset
            foreach (BaseTile tile in FindObjectsOfType<BaseTile>())
            {
                tile.collateral = false;
            }

            foreach (Vector2 pos in _selectedTiles) {
                foreach (BaseTile tile in BaseTileAtPos(pos).ListCollateralDamage(this, 1f))
                {
                    collateral.Add(tile);
                    tile.collateral = true;
                }
            }

            LoopCollateralDamage(collateral);
        }

        private void RecalculateDamage ()
        {
            foreach (Player player in FindObjectsOfType<Player>())
            {
                if (_curPlayer != player.localID) //Show potential damage on the target player.
                {
                    float targetPlayerHealth;
                    if (player.localID == 0)
                        targetPlayerHealth = healthPlayerOne;
                    else
                        targetPlayerHealth = healthPlayerTwo;

                    float damage = (_selectedTiles.Count * _selectedTiles.Count - 2) + _baseTiles.FindAll(item => item.collateral == true).Count * Constants.BoosterCollateralDamage;
                    float calculatedDamage = targetPlayerHealth - damage;
                    player.FindInterface().SetHitpoints(calculatedDamage);
                }
            }
        }

        private void LoopCollateralDamage(List<BaseTile> collateral)
        {
            List<BaseTile> copy = new List<BaseTile>(collateral);

            bool anyChanges = false;
            foreach (BaseTile collateralTile in copy)
            {
                if (collateralTile.boosterLevel > 0)
                {
                    foreach (BaseTile affectedTile in collateralTile.ListCollateralDamage(this, 1f))
                    {
                        if (!copy.Contains(affectedTile) && affectedTile.collateral == false) { 
                            collateral.Add(affectedTile);
                            affectedTile.collateral = true;
                            anyChanges = true;
                        }
                    }
                }
            }

            if (anyChanges)
                LoopCollateralDamage(collateral);
        }

        public List<BaseTile> FindAdjacentTiles(Vector2 position, float radius)
        {
            List<BaseTile> allTiles = _baseTiles;
            BaseTile centerTile = allTiles.Find(item => item.position.x == position.x && item.position.y == position.y);
            List<BaseTile> targetTiles = new List<BaseTile>();
            foreach (BaseTile tile in allTiles)
            {
                if (centerTile.DistanceToTile(tile) <= Constants.DistanceBetweenTiles * radius)
                    targetTiles.Add(tile);
            }

            return targetTiles;
        }

        public void InitiateCombo()
        {
            //Both host and client execute this command.
            bool trapped = false;

            Player targetPlayer = GetNextPlayer(_curPlayer);
            if (targetPlayer.localID == 0)
                targetPlayer.FindInterface().SetHitpoints(healthPlayerOne);
            else
                targetPlayer.FindInterface().SetHitpoints(healthPlayerTwo);


            trapped = false;
            foreach (Vector2 pos in _selectedTiles)
            {
                if (BaseTileAtPos(pos).boosterLevel >= 4)
                    trapped = true;
            }

            if (trapped)
                _gameContext.ShowLargeText("Trap was triggered!");

            int count = 0;
            foreach (Vector2 pos in _selectedTiles)
            {

                CreateTileAttackPlayerEffect(pos, count, trapped);
                count++;
            }

            foreach (BaseTile tile in _baseTiles.FindAll(item => item.collateral == true))
            {
                CreateTileAttackPlayerEffect(tile.position, count, true, trapped);
            }


            //Only master client will update the _grid and then sync it.
            if (PhotonNetwork.isMasterClient)
            {
                int color = BaseTileAtPos(_selectedTiles[_selectedTiles.Count - 1]).color;
                foreach (Vector2 pos in _selectedTiles)
                {
                    DestroyTileAtPosition(pos);
                    if (BaseTileAtPos(pos).boosterLevel >= 4)
                        trapped = true;
                }

                foreach (BaseTile tile in _baseTiles.FindAll(item => item.collateral == true))
                {
                    DestroyTileAtPosition(tile.position);
                }

                CreateBooster(_selectedTiles[_selectedTiles.Count - 1], _selectedTiles.Count, color);

                if ((_curPlayer == 0 && !trapped) || (_curPlayer == 1 && trapped))
                {
                    DamagePlayerWithCombo(1, _selectedTiles.Count, _baseTiles.FindAll(item => item.collateral == true).Count * Constants.BoosterCollateralDamage);
                    FillPowerBar(0, color, _selectedTiles.Count);
                }
                else
                {
                    DamagePlayerWithCombo(0, _selectedTiles.Count, _baseTiles.FindAll(item => item.collateral == true).Count * Constants.BoosterCollateralDamage);
                    FillPowerBar(1, color, _selectedTiles.Count);
                }
            }

            EndTurn();
            _selectedTiles.Clear();
        }

        private void CreateTileAttackPlayerEffect(Vector2 pos, int count, bool trapped)
        {
            CreateTileAttackPlayerEffect(pos, count, false, trapped);
        }

        private void CreateTileAttackPlayerEffect (Vector2 pos, int count, bool collateral, bool trapped)
        {
            GameObject go = Instantiate(Resources.Load("ParticleEffects/TileDebris")) as GameObject;
            Player[] players = FindObjectsOfType<Player>();
            Player targetPlayer = null;
            foreach (Player player in players)
            {
                if ((trapped && player.localID == _curPlayer) || (!trapped && player.localID != _curPlayer))
                {
                    targetPlayer = player;
                }
            }

            BaseTile baseTile = BaseTileAtPos(pos);
            go.transform.position = baseTile.transform.position;
            go.GetComponent<TileExplosion>().Init(targetPlayer, count, baseTile.HexSprite(TileTypes.EColor.yellow + baseTile.color));

            if (!collateral) { 
                GameObject boosterFiller = Instantiate(Resources.Load("ParticleEffects/BoosterFiller")) as GameObject;
                boosterFiller.transform.position = baseTile.transform.position;
                boosterFiller.GetComponent<BoosterFiller>().Init(targetPlayer, count, baseTile.color);
            }

            //Explosion effect
            GameObject expl = null;
            if (baseTile.boosterLevel == 1)
                expl = Instantiate(Resources.Load("ParticleEffects/Booster_One_Explosion")) as GameObject;
            else if (baseTile.boosterLevel == 2)
                expl = Instantiate(Resources.Load("ParticleEffects/Booster_Two_Explosion")) as GameObject;
            else if (baseTile.boosterLevel == 3)
                expl = Instantiate(Resources.Load("ParticleEffects/Booster_Three_Explosion")) as GameObject;
            else if (baseTile.boosterLevel >= 4)
                expl = Instantiate(Resources.Load("ParticleEffects/Booster_Trap_Explosion")) as GameObject;
            else { 
                if (!collateral) { 
                    expl = Instantiate(Resources.Load("ParticleEffects/TileDestroyedTimer")) as GameObject;
                    expl.GetComponent<TimedEffect>().createAfterTime = count * Constants.DelayAfterTileDestruction;
                    expl.GetComponent<TimedEffect>().basetileToHide = baseTile;
                }
            }
            if (expl != null)
            {
                expl.transform.position = baseTile.transform.position;
            }
        }

        private void DestroyTileAtPosition (Vector2 pos)
        {
            _grid.data[(int)pos.x, (int)pos.y].color = Constants.AmountOfColors; //Equals being 'destroyed'
            _grid.data[(int)pos.x, (int)pos.y].boosterLevel = 0;
        }
    
        private void CreateBooster (Vector2 pos, int comboCount, int color)
        {
            if (comboCount >= Constants.BoosterThreeThreshhold)
                _grid.data[(int)pos.x, (int)pos.y].boosterLevel = 3;
            else if (comboCount >= Constants.BoosterTwoThreshhold)
                _grid.data[(int)pos.x, (int)pos.y].boosterLevel = 2;
            else if (comboCount >= Constants.BoosterOneThreshhold)
                _grid.data[(int)pos.x, (int)pos.y].boosterLevel = 1;
            else
                _grid.data[(int)pos.x, (int)pos.y].boosterLevel = 0;

            if (comboCount >= Constants.BoosterOneThreshhold)
                _grid.data[(int)pos.x, (int)pos.y].color = color;

            Tile tile = _grid.data[(int)pos.x, (int)pos.y];
            photonView.RPC("RPCSendTile", PhotonTargets.All, tile);
        }

        public void DamagePlayerWithCombo(int playerNumber, float comboSize, float collateralDamage) //only master-client side
        {
            float damage = (comboSize * comboSize - 2) + collateralDamage;

            if (playerNumber == 0)
            {
                if (!P1_ShieldActive)
                {
                    healthPlayerOne -= damage;

                    photonView.RPC("RPC_DamageComboMessage", PhotonTargets.All, 0, (comboSize * comboSize - 2), collateralDamage);
                }
                else
                {
                    P1_ShieldActive = false;
                    photonView.RPC("RPC_ShieldEffect", PhotonTargets.All, 0);
                    photonView.RPC("RPC_ShieldMessage", PhotonTargets.All, 0, damage);
                }
            }
            else
            {
                if (!P2_ShieldActive)
                {
                    
                    healthPlayerTwo -= damage;//Mathf.Pow(comboSize, 2);

                    photonView.RPC("RPC_DamageComboMessage", PhotonTargets.All, 1, (comboSize * comboSize - 2), collateralDamage);
                }
                else
                {
                    P2_ShieldActive = false;
                    photonView.RPC("RPC_ShieldEffect", PhotonTargets.All, 1);
                    photonView.RPC("RPC_ShieldMessage", PhotonTargets.All, 1, damage);
                }
                
            }

            if (healthPlayerOne <= 0 || healthPlayerTwo <= 0)
                _gameDone = true;


        }

        public void DamagePlayer (int playerNumber, float damage) //only master-client side
        {
            if (playerNumber == 0)
            {
                if (!P1_ShieldActive)
                {
                    healthPlayerOne -= damage;
                    photonView.RPC("RPC_DamageMessage", PhotonTargets.All, 0, damage);

                }
                else
                {
                    P1_ShieldActive = false;
                    photonView.RPC("RPC_ShieldEffect", PhotonTargets.All, 0);
                    photonView.RPC("RPC_ShieldMessage", PhotonTargets.All, 0, damage);
                }

                GetPlayerByID(playerNumber).FindInterface().SetHitpoints(healthPlayerOne);
                GetPlayerByID(playerNumber).FindInterface().UpdateShadowhealth(healthPlayerOne);
                photonView.RPC("RPC_UpdateHealth", PhotonTargets.Others, playerNumber, healthPlayerOne);
            }
            else
            {
                if (!P2_ShieldActive)
                {
                    healthPlayerTwo -= damage;
                    photonView.RPC("RPC_DamageMessage", PhotonTargets.All, 1, damage);
                }
                else
                {
                    P2_ShieldActive = false;
                    photonView.RPC("RPC_ShieldEffect", PhotonTargets.All, 1);
                    photonView.RPC("RPC_ShieldMessage", PhotonTargets.All, 1, damage);
                }

                GetPlayerByID(playerNumber).FindInterface().SetHitpoints(healthPlayerTwo);
                GetPlayerByID(playerNumber).FindInterface().UpdateShadowhealth(healthPlayerTwo);
                photonView.RPC("RPC_UpdateHealth", PhotonTargets.Others, playerNumber, healthPlayerTwo);
            }

            if (healthPlayerOne <= 0 || healthPlayerTwo <= 0) { 
                _gameDone = true;
                EndTurn(); //Fireball should end the game.
            }
        }

        [PunRPC]
        private void RPC_UpdateHealth(int playerNumber, float hitpoints)
        {
            GetPlayerByID(playerNumber).FindInterface().SetHitpoints(hitpoints);
            GetPlayerByID(playerNumber).FindInterface().UpdateShadowhealth(hitpoints);
        }

        public bool IsMyTurn()
        {
            if (_myPlayer == null)
                return false;

            if (_myPlayer.localID == _curPlayer)
                return true;
            return false;
        }

        private void FillPowerBar (int playerNumber, int color, int increaseBy)
        {
            if (playerNumber == 0)
            {
                if (color == 1) // yellow, blue, green, red
                    P1_PowerBlue = Mathf.Min(P1_PowerBlue + increaseBy, Constants.BluePowerReq);
                else if (color == 2)
                    P1_PowerGreen = Mathf.Min(P1_PowerGreen + increaseBy, Constants.GreenPowerReq);
                else if (color == 3)
                    P1_PowerRed = Mathf.Min(P1_PowerRed + increaseBy, Constants.RedPowerReq);
                else if (color == 0)
                    P1_PowerYellow = Mathf.Min(P1_PowerYellow + increaseBy, Constants.YellowPowerReq);

                /*if (P1_PowerBlue == Constants.BluePowerReq)
                    _gameContext.ShowText("Shield ability available!");
                if (P1_PowerGreen == Constants.GreenPowerReq)
                    _gameContext.ShowText("Heal ability available!");
                if (P1_PowerRed == Constants.RedPowerReq)
                    _gameContext.ShowText("Trap ability available!");
                if (P1_PowerYellow == Constants.YellowPowerReq)
                    _gameContext.ShowText("Fireball ability available!");*/
            } else
            {
                if (color == 1) { 
                    P2_PowerBlue = Mathf.Min(P2_PowerBlue + increaseBy, Constants.BluePowerReq);
                    photonView.RPC("RPCFillPower", PhotonTargets.Others, color, P2_PowerBlue);
                } else if (color == 2) { 
                    P2_PowerGreen = Mathf.Min(P2_PowerGreen + increaseBy, Constants.GreenPowerReq);
                    photonView.RPC("RPCFillPower", PhotonTargets.Others, color, P2_PowerGreen);
                } else if (color == 3) { 
                    P2_PowerRed = Mathf.Min(P2_PowerRed + increaseBy, Constants.RedPowerReq);
                    photonView.RPC("RPCFillPower", PhotonTargets.Others, color, P2_PowerRed);
                } else if (color == 0) { 
                    P2_PowerYellow = Mathf.Min(P2_PowerYellow + increaseBy, Constants.YellowPowerReq);
                    photonView.RPC("RPCFillPower", PhotonTargets.Others, color, P2_PowerYellow);
                }
            }
        }

        public void PowerClicked (int color)
        {
            if (PhotonNetwork.isMasterClient) {
                if (_curPlayer == 0) { 
                    if (color == 1 && P1_PowerBlue >= Constants.BluePowerReq) //blue
                    {
                        P1_ShieldActive = true;
                        photonView.RPC("RPC_ShieldActivated", PhotonTargets.All, 0);
                        P1_PowerBlue = 0;
                    }
                    else if (color == 2 && P1_PowerGreen >= Constants.GreenPowerReq) //green
                    {
                        healthPlayerOne += Constants.HealPower;
                        photonView.RPC("RPC_HealEffect", PhotonTargets.All, 0);
                        P1_PowerGreen = 0;

                        GetPlayerByID(_curPlayer).FindInterface().SetHitpoints(healthPlayerOne);
                        GetPlayerByID(_curPlayer).FindInterface().UpdateShadowhealth(healthPlayerOne);
                        photonView.RPC("RPC_UpdateHealth", PhotonTargets.Others, _curPlayer, healthPlayerOne);
                    }
                    else if (color == 3 && P1_PowerRed >= Constants.RedPowerReq) //red
                    {
                        CreateTrap();
                        P1_PowerRed = 0;

                        _gameContext.ShowText("You placed a trap. Don't trigger it yourself!");
                        photonView.RPC("RPC_MessageTrap", PhotonTargets.Others);
                    }
                    else if (color == 0 && P1_PowerYellow >= Constants.YellowPowerReq) //yellow
                    {
                        CreateFireball();
                        P1_PowerYellow = 0;

                        _gameContext.ShowText("Touch the fireball and throw it!");
                        photonView.RPC("RPC_MessageFireball", PhotonTargets.Others); 
                    }
                } else
                {
                    if (color == 1 && P2_PowerBlue >= Constants.BluePowerReq) //blue
                    {
                        P2_ShieldActive = true;
                        photonView.RPC("RPC_ShieldActivated", PhotonTargets.All, 1);
                        P2_PowerBlue = 0;
                        photonView.RPC("RPC_EmptyPower", PhotonTargets.Others, color); //Sync with non-host player.
                    }
                    else if (color == 2 && P2_PowerGreen >= Constants.GreenPowerReq) //green
                    {
                        healthPlayerTwo += Constants.HealPower;
                        photonView.RPC("RPC_HealEffect", PhotonTargets.All, 1);
                        P2_PowerGreen = 0;
                        photonView.RPC("RPC_EmptyPower", PhotonTargets.Others, color);

                        GetPlayerByID(_curPlayer).FindInterface().SetHitpoints(healthPlayerTwo);
                        GetPlayerByID(_curPlayer).FindInterface().UpdateShadowhealth(healthPlayerTwo);
                        photonView.RPC("RPC_UpdateHealth", PhotonTargets.Others, _curPlayer, healthPlayerTwo);
                    }
                    else if (color == 3 && P2_PowerRed >= Constants.RedPowerReq) //red
                    {
                        photonView.RPC("RPC_P2_CreateTrap", PhotonTargets.Others);
                        P2_PowerRed = 0;
                        photonView.RPC("RPC_EmptyPower", PhotonTargets.Others, color);

                        _gameContext.ShowText("Opponent placed a trap. Be careful!");
                        _gameContext.ShowLargeText("Trap placed, careful!");
                    }
                    else if (color == 0 && P2_PowerYellow >= Constants.YellowPowerReq) //yellow
                    {
                        photonView.RPC("RPC_P2_Create_Fireball", PhotonTargets.Others);
                        P2_PowerYellow = 0;
                        photonView.RPC("RPC_EmptyPower", PhotonTargets.Others, color);

                        _gameContext.ShowText("Opponent summoned a fireball!");
                    }
                }
            }
        }

        private void CreateFireball ()
        {
            GameObject fireballGO = PhotonNetwork.Instantiate("Fireball", Vector3.zero, Quaternion.identity, 0);

            fireballGO.GetComponent<YellowPower>().ownerPlayer = MyPlayer;
            fireballGO.name = "Fireball" + MyPlayer.localID;
            fireballGO.transform.position = GameObject.Find("MyYellow").transform.position;
        }

        [PunRPC]
        private void RPC_P2_Create_Fireball()
        {
            CreateFireball();
            _gameContext.ShowText("Touch the fireball and throw it!");
        }

        [PunRPC]
        public void FireballHit () //should be both master-client side and guest-side
        {
            GameObject fireball = GameObject.FindGameObjectWithTag("ActiveFireball");

            if (fireball.GetComponent<YellowPower>().ownerPlayer == MyPlayer)
            {
                GameObject explosion = Instantiate(Resources.Load("ParticleEffects/FireballHit")) as GameObject;
                explosion.transform.position = GameObject.Find("OpponentAvatar").transform.position;

                if (PhotonNetwork.isMasterClient) { 
                    DamagePlayer(EnemyPlayer.localID, Constants.FireballPower);
                    ResetTimer();
                }
            }
            else
            {
                GameObject explosion = Instantiate(Resources.Load("ParticleEffects/FireballHit")) as GameObject;
                explosion.transform.position = GameObject.Find("MyAvatar").transform.position;

                if (PhotonNetwork.isMasterClient) { 
                    DamagePlayer(MyPlayer.localID, Constants.FireballPower);
                    ResetTimer();
                }
            }

            Destroy(fireball);
        }

        private void CreateTrap()
        {
            GameObject trapGO = Instantiate(Resources.Load("TrapPower")) as GameObject;

            trapGO.GetComponent<TrapPower>().ownerPlayer = MyPlayer;
            trapGO.name = "TrapPower" + MyPlayer.localID;
            trapGO.transform.position = GameObject.Find("MyRed").transform.position;
        }

        [PunRPC]
        private void RPC_P2_CreateTrap()
        {
            CreateTrap();
            _gameContext.ShowText("You placed a trap. Don't trigger it yourself!");
        }

        [PunRPC]
        public void RPC_CreateTrapBooster(Vector2 pos, int creatorPlayer)
        {
            if (PhotonNetwork.isMasterClient)
            {
                _grid.data[(int)pos.x, (int)pos.y].boosterLevel = 4 + creatorPlayer;
                Tile tile = _grid.data[(int)pos.x, (int)pos.y];
                photonView.RPC("RPCSendTile", PhotonTargets.All, tile);
            }
        }

        [PunRPC]
        private void RPC_TurnEnded(int curPlayer)
        {
            if (_myPlayer != null) { 
                if (curPlayer == _myPlayer.localID) //Reversed
                {
                   // _gameContext.ShowText("It is now your turn!");
                    _gameContext.ShowLargeText("Your turn!");
                }
                else
                {
                   // _gameContext.ShowText("Waiting for " + _enemyPlayer.GetName() + "'s turn...");
                    _gameContext.ShowLargeText(_enemyPlayer.GetName() + "'s turn!");
                }
            }

            if (MyPlayer.localID == 0)
            {
                MyPlayer.FindInterface().UpdateShadowhealth(healthPlayerOne);
                EnemyPlayer.FindInterface().UpdateShadowhealth(healthPlayerTwo);
            }
            else
            {
                MyPlayer.FindInterface().UpdateShadowhealth(healthPlayerTwo);
                EnemyPlayer.FindInterface().UpdateShadowhealth(healthPlayerOne);
            }
        }

        [PunRPC]
        private void RPCAnimateTile(AnimateTile tile)
        {
            BaseTile dropIntoTile = BaseTileAtPos(new Vector2 (tile.x, tile.y));

            //Debug.Log("Anim request: " + tile + " | " + tile.x + ", " + tile.y + " > " + tile.color);

            dropIntoTile.color = tile.color;
            dropIntoTile.Animate(tile.fallDistance);
        }

        [PunRPC]
        private void RPC_EndGame (int winnerPlayer)
        {
            GameObject.Find("PlayScreen").GetComponent<PlayGameCanvas>().EndGame(winnerPlayer);
            _gameDone = false;
            _isActive = false;
        }

        [PunRPC]
        private void RPCFillPower(int color, int value)
        {
            if (color == 1)
                P2_PowerBlue = value;
            else if (color == 2)
                P2_PowerGreen = value;
            else if (color == 3)
                P2_PowerRed = value;
            else if (color == 0)
                P2_PowerYellow = value;

            /*if (P2_PowerBlue == Constants.BluePowerReq)
                _gameContext.ShowText("Shield ability available!");
            if (P2_PowerGreen == Constants.GreenPowerReq)
                _gameContext.ShowText("Heal ability available!");
            if (P2_PowerRed == Constants.RedPowerReq)
                _gameContext.ShowText("Trap ability available!");
            if (P2_PowerYellow == Constants.YellowPowerReq)
                _gameContext.ShowText("Fireball ability available!");*/
        }

        [PunRPC]
        private void RPC_EmptyPower(int color)
        {
            if (color == 1)
                P2_PowerBlue = 0;
            else if (color == 2)
                P2_PowerGreen = 0;
            else if (color == 3)
                P2_PowerRed = 0;
            else if (color == 0)
                P2_PowerYellow = 0;
        }


        [PunRPC]
        public void SendRematchRequest()
        {
            Debug.Log("Rematch order gotten, reloading Scene.");
            _rematchRequested = true;
        }

        [PunRPC]
        private void RPC_ShieldEffect (int targetPlayer)
        {
            if (targetPlayer == MyPlayer.localID)
                foreach (GameObject activeShield in GameObject.FindGameObjectsWithTag("ActiveShield"))
                    Destroy(activeShield);

            GameObject effect = Instantiate(Resources.Load("ParticleEffects/ShieldEffect")) as GameObject;
            Vector2 effectPosition = new Vector2();
            if (_myPlayer.localID == targetPlayer)
                effectPosition = GameObject.Find("MyAvatar").GetComponent<RectTransform>().position;
            else
                effectPosition = GameObject.Find("OpponentAvatar").GetComponent<RectTransform>().position;
            effect.transform.position = effectPosition;
        }

        [PunRPC]
        private void RPC_ShieldActivated(int targetPlayer)
        {
            if (_myPlayer.localID == targetPlayer) { 
                GameObject effect = Instantiate(Resources.Load("ParticleEffects/ShieldActivated")) as GameObject;
                Vector2 effectPosition = new Vector2();
                effectPosition = GameObject.Find("MyAvatar").GetComponent<RectTransform>().position;
                effect.transform.position = effectPosition;

                _gameContext.ShowText("Shield is now active. Next damage is blocked.");
            }
        }

        [PunRPC]
        private void RPC_HealEffect(int targetPlayer)
        {
            GameObject effect = Instantiate(Resources.Load("ParticleEffects/HealEffect")) as GameObject;
            Vector2 effectPosition = new Vector2();

            float hitpoints = 0f;
            if (GetPlayerByID(targetPlayer).localID == 0)
                hitpoints = healthPlayerOne;
            else
                hitpoints = healthPlayerTwo;

            if (_myPlayer.localID == targetPlayer)
            {
                _gameContext.ShowText("You healed for " +  Constants.HealPower +  "!");

                effectPosition = GameObject.Find("MyAvatar").GetComponent<RectTransform>().position;
            }
            else
            {
                _gameContext.ShowText("Opponent healed for " + Constants.HealPower + "!");

                effectPosition = GameObject.Find("OpponentAvatar").GetComponent<RectTransform>().position;
            }
            effect.transform.position = effectPosition;
        }

        #region RPC Messages
        [PunRPC]
        private void RPC_DamageMessage(int targetPlayer, float damage)
        {
            /*if (_myPlayer.localID == targetPlayer)
            {
                _gameContext.ShowText("You received " + damage + " damage!");
            }
            else
            {
                _gameContext.ShowText("You dealt " + _enemyPlayer.GetName() + " " + damage + " damage!");
            }*/
        }

        [PunRPC]
        private void RPC_DamageComboMessage(int targetPlayer, float comboDamage, float collateralDamage)
        {
            /*if (_myPlayer.localID == targetPlayer)
            {
                _gameContext.ShowText("You received " + comboDamage + " + " + collateralDamage + " damage!");
            }
            else
            {
                _gameContext.ShowText("You dealt " + _enemyPlayer.GetName() + " " + comboDamage + " + " + collateralDamage + " damage!");
            }*/
        }

        [PunRPC]
        private void RPC_ShieldMessage (int targetPlayer, float damage)
        {
            if (_myPlayer.localID == targetPlayer)
            {
                _gameContext.ShowText("You blocked " + damage + " damage with your shield!");
                _gameContext.ShowMyText("Blocked!");
            } else
            {
                _gameContext.ShowText(_enemyPlayer.GetName() + " blocked " + damage + " damage with a shield!");
                _gameContext.ShowEnemyText("Blocked!");
            }
        }

        [PunRPC]
        private void RPC_MessageFireball ()
        {

            _gameContext.ShowText("Opponent summoned a fireball!");
        }

        [PunRPC]
        private void RPC_MessageTrap()
        {
            _gameContext.ShowText("Opponent placed a trap. Be careful!");
            _gameContext.ShowLargeText("Trap placed, careful!");
        }
        #endregion

        #region Serialization
        public static readonly byte[] memTile = new byte[4 * 4];
        private static short SerializeTile(StreamBuffer outStream, object customobject)
        {
            Tile tile = (Tile)customobject;
            lock (memTile)
            {
                byte[] bytes = memTile;
                int index = 0;
                Protocol.Serialize(tile.color, bytes, ref index);
                Protocol.Serialize(tile.boosterLevel, bytes, ref index);
                Protocol.Serialize(tile.x, bytes, ref index);
                Protocol.Serialize(tile.y, bytes, ref index);
                outStream.Write(bytes, 0, 4 * 4);
            }

            return 4 * 4;
        }

        private static object DeserializeTile(StreamBuffer inStream, short length)
        {
            Tile tile = new Tile();
            lock (memTile)
            {
                inStream.Read(memTile, 0, 4 * 4);
                int index = 0;
                Protocol.Deserialize(out tile.color, memTile, ref index);
                Protocol.Deserialize(out tile.boosterLevel, memTile, ref index);
                Protocol.Deserialize(out tile.x, memTile, ref index);
                Protocol.Deserialize(out tile.y, memTile, ref index);
            }

            return tile;
        }

        public static readonly byte[] memAnimTile = new byte[4 * 4];
        private static short SerializeAnimTile(StreamBuffer outStream, object customobject)
        {
            AnimateTile tile = (AnimateTile)customobject;
            lock (memAnimTile)
            {
                byte[] bytes = memAnimTile;
                int index = 0;
                Protocol.Serialize(tile.color, bytes, ref index);
                Protocol.Serialize(tile.fallDistance, bytes, ref index);
                Protocol.Serialize(tile.x, bytes, ref index);
                Protocol.Serialize(tile.y, bytes, ref index);
                outStream.Write(bytes, 0, 4 * 4);
            }

            return 4 * 4;
        }

        private static object DeserializeAnimTile(StreamBuffer inStream, short length)
        {
            AnimateTile tile = new AnimateTile();
            lock (memAnimTile)
            {
                inStream.Read(memAnimTile, 0, 4 * 4);
                int index = 0;
                Protocol.Deserialize(out tile.color, memAnimTile, ref index);
                Protocol.Deserialize(out tile.fallDistance, memAnimTile, ref index);
                Protocol.Deserialize(out tile.x, memAnimTile, ref index);
                Protocol.Deserialize(out tile.y, memAnimTile, ref index);
            }

            return tile;
        }

        public static readonly byte[] memGrid = new byte[Constants.gridXsize * Constants.gridYsize * 4 * 4];
        private static short SerializeGrid(StreamBuffer outStream, object customobject)
        {
            Grid sGrid = (Grid)customobject;
            if (sGrid != null)
            {
                lock (memGrid)
                {
                    byte[] bytes = memGrid;
                    int index = 0;
                    for (int x = 0; x < Constants.gridXsize; x++)
                    {
                        for (int y = 0; y < Constants.gridYsize; y++)
                        {
                            //Debug.Log("DEBUG: [" + sGrid.data[x, y].x + ", "+ sGrid.data[x, y].y + ", " + sGrid.data[x, y].color + "]");
                            Protocol.Serialize(sGrid.data[x, y].color, bytes, ref index);
                            Protocol.Serialize(sGrid.data[x, y].boosterLevel, bytes, ref index);
                            Protocol.Serialize(x, bytes, ref index);
                            Protocol.Serialize(y, bytes, ref index);
                        }
                    }

                    outStream.Write(bytes, 0, Constants.gridXsize * Constants.gridYsize * 4 * 4);
                }
            }

            return Constants.gridXsize * Constants.gridYsize * 4 * 4;
        }

        private static object DeserializeGrid(StreamBuffer inStream, short length)
        {
            Grid sGrid = new Grid();
            sGrid.data = new Tile[Constants.gridXsize, Constants.gridYsize];

            lock (memGrid)
            {
                inStream.Read(memGrid, 0, Constants.gridXsize * Constants.gridYsize * 4 * 4);
                int index = 0;
                for (int x = 0; x < Constants.gridXsize; x++)
                {
                    for (int y = 0; y < Constants.gridYsize; y++)
                    {
                        sGrid.data[x, y] = new Tile();
                        Protocol.Deserialize(out sGrid.data[x, y].color, memGrid, ref index);
                        Protocol.Deserialize(out sGrid.data[x, y].boosterLevel, memGrid, ref index);
                        Protocol.Deserialize(out x, memGrid, ref index);
                        Protocol.Deserialize(out y, memGrid, ref index);
                    }
                }
            }

            return sGrid;
        }
        #endregion
    }
}
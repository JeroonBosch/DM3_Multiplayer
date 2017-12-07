using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using System.Collections.Generic;

namespace Com.Hypester.DM3
{
    public class GameHandler : Photon.MonoBehaviour, IPunObservable
    {
        #region private variables
        private bool _isActive;
        public bool Active { get { return _isActive; } }
        
        private Grid _grid;
        private int _curPlayer;
        private Dictionary<Vector2, TileView> _baseTiles;
        private Player _myPlayer;
        private Player _enemyPlayer;
        private Dictionary<Vector2, TileView> _selectedTiles;
        public Dictionary<Vector2, TileView> _collateralTiles = new Dictionary<Vector2, TileView>();

        private bool _gridReceived = false;
        private bool _gridVisualized = false;
        private bool _gameDone = false;

        private GameContext _gameContext;

        private float _timer = 0f; //Generic timer.

        private bool _rematchRequested = false;
        #endregion

        #region public variables
        private int _gameID = 0; //Used for tournaments.
        public int GameID { get { return _gameID; } set { _gameID = value; GameIDSet(); } }

        public Player MyPlayer { get { return _myPlayer; } }
        public Player EnemyPlayer { get { return _enemyPlayer; } }

        public GameStates gameState;

        //Health
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

        #region prefabs
        [Header("Prefabs")]
        [SerializeField] GameObject tilePrefab;
        [SerializeField] GameObject tileDebrisPrefab;
        [SerializeField] GameObject tileDestroyedTimerPrefab;

        [SerializeField] GameObject boosterFillerPrefab;
        [SerializeField] GameObject boosterExplosion1Prefab;
        [SerializeField] GameObject boosterExplosion2Prefab;
        [SerializeField] GameObject boosterExplosion3Prefab;
        [SerializeField] GameObject boosterExplosionTrapPrefab;

        [SerializeField] GameObject fireballPrefab;
        #endregion

        public delegate byte[] SerializeMethod(object customObject);
        public delegate object DeserializeMethod(byte[] serializedCustomObject);

        private void GameIDSet()
        {
            Debug.Log("Game ID Set to " + _gameID + " while " + PhotonController.Instance.gameID_requested + " is requested.");
            if (_gameID == PhotonController.Instance.gameID_requested)
            {
                PhotonController.Instance.GameController = this;
            }
        }

        [PunRPC]
        void RPC_InitGameHandler(int id)
        {
            GameID = id;
            gameObject.name = "Grid" + id;
            gameObject.transform.SetParent(GameObject.Find("PlayScreen").transform);
            gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
            gameObject.transform.localPosition = new Vector3(0, 0, 0);
        }

        private void Start()
        {
            PhotonPeer.RegisterType(typeof(Tile), (byte)'L', SerializeTile, DeserializeTile);
            PhotonPeer.RegisterType(typeof(AnimateTile), (byte)'A', SerializeAnimTile, DeserializeAnimTile);
            PhotonPeer.RegisterType(typeof(Grid), (byte)'G', SerializeGrid, DeserializeGrid);

            _isActive = false;
            _gameDone = false;
            _selectedTiles = new Dictionary<Vector2, TileView>();
            _baseTiles = new Dictionary<Vector2, TileView>();
            _curPlayer = 0;

            turnTimer = 0;
            healthPlayerOne = Constants.PlayerStartHP;
            healthPlayerTwo = Constants.PlayerStartHP;

            _grid = new Grid();
            gameState = new GameStates();
            gameState.State = GameStates.EGameState.search;

            _gameContext = GameObject.Find("GameContext").GetComponent<GameContext>();

            if (IsGameMaster())
            {
                GenerateGrid();
            }
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
                }

                _enemyPlayer = _myPlayer.opponent;
                Debug.Log("Match started, " + _myPlayer.joinNumber + " vs " + EnemyPlayer.joinNumber);

                if (_gridReceived)
                    VisualizeGrid();

                PhotonView photonView = PhotonView.Get(this);
                if (IsGameMaster())
                {
                    photonView.RPC("RPC_SendGridData", PhotonTargets.All, _grid);
                }


                if (!(PhotonNetwork.isMasterClient || _myPlayer.joinNumber == 3))
                {
                    transform.rotation = new Quaternion(0f, 0f, 180f, transform.rotation.w);
                }
                else
                {
                    transform.rotation = new Quaternion(0f, 0f, 0f, transform.rotation.w);
                }
                PlayerInterface[] interfaces = FindObjectsOfType<PlayerInterface>();

                _myPlayer.Reset();
                _enemyPlayer.Reset();
            }
        }

        public void Hide()
        {
            transform.localScale = new Vector3(0, 0, 0);
        }

        private void Update()
        {
            if (_isActive)
            {
                if (_gridReceived && !_gridVisualized)
                    VisualizeGrid();

                if (IsGameMaster())
                {
                    if (_MC_endTurnDelay)
                        _MC_endTurnDelayTimer += Time.deltaTime;

                    if (_MC_endTurnDelayTimer > Constants.TimeBetweenTurns)
                    {
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
                        else
                        {
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
                }
                else
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
            if (_rematchRequested)
            {
                _timer += Time.deltaTime;

                if (_timer > 2f)
                {
                    _rematchRequested = false;
                    _timer = 0f;
                    PhotonNetwork.LoadLevel("NormalGame");
                    Debug.Log("Normal Game rematch. Needs work probably.");
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
                    _selectedTiles.Clear();
                    TileView.areaList.Clear();

                    if (GetPlayerByID(_curPlayer) == MyPlayer)
                        _gameContext.ShowText("You ran out of time! Turn skipped.");
                }
                else
                    turnTimer += Time.fixedDeltaTime;
            }
            else
                turnTimer = 0f;
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

                    if (IsGameRelevant())
                    {  //Only visualize relevant game data.
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
        }

        private Player GetPlayerByID(int number)
        {
            foreach (Player player in FindObjectsOfType<Player>())
            {
                if (GameID == 0)
                {
                    if (player.localID == number && (player.joinNumber == 1 || player.joinNumber == 2))
                        return player;
                }
                else if (GameID == 1)
                {
                    if (player.localID == number && (player.joinNumber == 3 || player.joinNumber == 4))
                        return player;
                }
            }
            return null;
        }

        private Player GetNextPlayer(int number)
        {
            foreach (Player player in PlayerManager.instance.GetAllPlayers().Values)
            {
                if (player.localID != number)
                    return player;

                if (GameID == 0)
                {
                    if (player.localID != number && (player.joinNumber == 1 || player.joinNumber == 2))
                        return player;
                }
                else if (GameID == 1)
                {
                    if (player.localID != number && (player.joinNumber == 3 || player.joinNumber == 4))
                        return player;
                }
            }
            return null;
        }

        [PunRPC]
        private void RPC_HealthChanged(int targetPlayer, string difference)
        {
            if (IsGameRelevant()) //Visuals only
            {
                if (targetPlayer == 0)
                {
                    if (MyPlayer.localID == 0)
                        _gameContext.ShowMyText(difference);
                    else
                        _gameContext.ShowEnemyText(difference);
                }
                else
                {
                    if (MyPlayer.localID == 1)
                        _gameContext.ShowMyText(difference);
                    else
                        _gameContext.ShowEnemyText(difference);
                }
            }
        }

        private void SetGameState(GameStates.EGameState state)
        {
            if (IsGameMaster())
            {
                gameState.State = state;
                photonView.RPC("RPC_SyncState", PhotonTargets.Others, (int)state);
            }
        }

        [PunRPC]
        private void RPC_SyncState(int stateNo)
        {
            gameState.State = GameStates.EGameState.search + stateNo;
        }

        #region Grid functions
        void GenerateGrid()
        {
            _gridReceived = true;
            Debug.Log("Generating grid");

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

            Debug.Log("Grid generated (by master client)");
        }

        void VisualizeGrid()
        {
            _gridVisualized = true;
            Debug.Log("Grid visualized. (done once)");

            foreach (Transform child in transform)
                Destroy(child.gameObject);

            _baseTiles.Clear();
            _collateralTiles.Clear();

            for (int x = 0; x < Constants.gridXsize; x++)
            {
                for (int y = 0; y < Constants.gridYsize; y++)
                {
                    GameObject tile = Instantiate(tilePrefab);
                    tile.name = "Tile (" + x + "," + y + ")";
                    tile.transform.SetParent(transform, false);
                    TileView tileView = tile.GetComponent<TileView>();
                    tileView.position = new Vector2(x, y);
                    _baseTiles.Add(tileView.position, tileView);
                    if (x % 2 == 0)
                        tile.transform.localPosition = new Vector3((-Constants.gridXsize / 2 + x) * Constants.tileWidth + Constants.tileWidth / 2, (-Constants.gridYsize / 2 + y) * Constants.tileHeight + (Constants.tileHeight * .75f), 0f);
                    else
                        tile.transform.localPosition = new Vector3((-Constants.gridXsize / 2 + x) * Constants.tileWidth + Constants.tileWidth / 2, (-Constants.gridYsize / 2 + y) * Constants.tileHeight + (Constants.tileHeight * 0.25f), 0f);

                    if (_grid.data[x, y].color < Constants.AmountOfColors)
                    {
                        tile.GetComponent<Image>().enabled = true;
                        tileView.color = _grid.data[x, y].color;
                        tileView.SetSelected = false;

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
                //Debug.Log("Grid updated.");

                for (int x = 0; x < Constants.gridXsize; x++)
                {
                    for (int y = 0; y < Constants.gridYsize; y++)
                    {
                        Vector2 pos = new Vector2(x, y);
                        TileView tile = TileViewAtPos(pos);
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
                            else
                            {
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
                    TileView baseTile = TileViewAtPos(new Vector2(x, y));
                    if (baseTile != null)
                    {
                        if (baseTile.color >= Constants.AmountOfColors)
                            hasEmptyTiles = true;
                    }
                    else
                        return false;
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

            photonView.RPC("RPC_SendGridData", PhotonTargets.All, _grid);
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
                            photonView.RPC("RPC_AnimateTile", PhotonTargets.All, anim);
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
                        photonView.RPC("RPC_AnimateTile", PhotonTargets.All, anim);
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
                            photonView.RPC("RPC_AnimateTile", PhotonTargets.All, anim);
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
                        photonView.RPC("RPC_AnimateTile", PhotonTargets.All, anim);
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

        public TileView TileViewAtPos(Vector2 position)
        {
            TileView tv = null;
            _baseTiles.TryGetValue(position, out tv);
            return tv;
        }

        public Dictionary<Vector2, TileView> GetAllBaseTiles() { return _baseTiles; }

        [PunRPC]
        public void RPC_SendTile(Tile tile)
        {
            _grid.data[tile.x, tile.y].color = tile.color;
            _grid.data[tile.x, tile.y].boosterLevel = tile.boosterLevel;
            TileViewAtPos(new Vector2(tile.x, tile.y)).boosterLevel = tile.boosterLevel;
        }

        [PunRPC]
        public void RPC_RequestGridData()
        {
            Debug.Log("Grid did not render properly, requesting update.");
            photonView.RPC("RPC_SendGridData", PhotonTargets.Others, _grid);
        }

        [PunRPC]
        public void RPC_SendGridData(Grid grid)
        {
            _gridReceived = true;
            _grid.data = grid.data;
            GridUpdate();
        }
        #endregion

        private void EndTurn() //both master-client and guest-client execute this.
        {
            turnTimer = 0f;
            SetGameState(GameStates.EGameState.interim);

            if (GameObject.FindGameObjectWithTag("ActiveTrap"))
            {
                Destroy(GameObject.FindGameObjectWithTag("ActiveTrap"));
            }

            //Master client will sync this.
            if (IsGameMaster())
            {
                _MC_endTurnDelay = true;
                _MC_endTurnDelayTimer = 0f;
            }
        }

        public void ResetTimer()
        {
            turnTimer = 0f;
            photonView.RPC("RPC_ResetTimer", PhotonTargets.Others);
        }

        [PunRPC]
        private void RPC_ResetTimer()
        {
            turnTimer = 0f;
        }

        #region selections
        [PunRPC]
        public void RPC_AddToSelection(Vector2 pos) //master-client and guest side, both.
        {
            //Debug.Log("Tile added to selection. My player subscribed to Game#" + MyPlayer.GetRequestedGameID() + " and this is " + GameID);
            TileView tile = TileViewAtPos(pos);
            if (tile == null) { return; }
            tile.SetSelected = true;

            TileView.areaList.Add(tile);
            tile.GetArea();
            if (TileView.areaList.Count > 2)
            {
                if (!_selectedTiles.ContainsKey(tile.position)) { _selectedTiles.Add(tile.position, tile); }
                foreach (TileView t in TileView.areaList)
                {
                    if (!_selectedTiles.ContainsKey(t.position)) { _selectedTiles.Add(t.position, t); }
                    t.SetSelected = true;
                }
                RecalculateCollateral();
                RecalculateDamage();
            }
            TileView.areaList.Clear();
        }

        [PunRPC]
        public void RPC_RemoveFromSelection(Vector2 pos) //master-client and guest side, both.
        {
            _selectedTiles.Remove(pos);
            TileViewAtPos(pos).SetSelected = false;

            if (TileViewAtPos(pos).boosterLevel > 0)
                RecalculateCollateral();
            RecalculateDamage();
        }

        [PunRPC]
        public void RPC_RemoveSelections() //master-client and guest side, both.
        {
            foreach (KeyValuePair<Vector2, TileView> kvp in _baseTiles)
            {
                kvp.Value.SetSelected = false;
                kvp.Value.collateral = false;
            }
            _selectedTiles.Clear();

            RecalculateCollateral();
        }
        #endregion

        private void RecalculateCollateral()
        {
            List<TileView> collateral = new List<TileView>();

            //Reset
            foreach (KeyValuePair<Vector2, TileView> kvp in _baseTiles)
            {
                kvp.Value.collateral = false;
            }

            foreach (KeyValuePair<Vector2, TileView> kvp in _selectedTiles)
            {
                foreach (TileView tile in TileViewAtPos(kvp.Key).ListCollateralDamage(this, 1f))
                {
                    collateral.Add(tile);
                    tile.collateral = true;
                }
            }

            LoopCollateralDamage(collateral);
        }

        private void LoopCollateralDamage(List<TileView> collateral)
        {
            List<TileView> copy = new List<TileView>(collateral);

            bool anyChanges = false;
            foreach (TileView collateralTile in copy)
            {
                if (collateralTile.boosterLevel > 0)
                {
                    foreach (TileView affectedTile in collateralTile.ListCollateralDamage(this, 1f))
                    {
                        if (!copy.Contains(affectedTile) && affectedTile.collateral == false)
                        {
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


        private void RecalculateDamage()
        {
            if (IsGameRelevant()) { 
                foreach (Player player in FindObjectsOfType<Player>())
                {
                    if (_curPlayer != player.localID) //Show potential damage on the target player.
                    {
                        float targetPlayerHealth;
                        if (player.localID == 0)
                            targetPlayerHealth = healthPlayerOne;
                        else
                            targetPlayerHealth = healthPlayerTwo;

                        float damage = (_selectedTiles.Count * 5) + _collateralTiles.Count * Constants.BoosterCollateralDamage;
                        float calculatedDamage = targetPlayerHealth - damage;
                        player.playerInterface.SetHitpoints(calculatedDamage);
                    }
                }
            }
        }

        public List<TileView> FindAdjacentTiles(Vector2 position, float radius)
        {
            TileView centerTile = _baseTiles[position];
            List<TileView> targetTiles = new List<TileView>();
            if (!centerTile) { return targetTiles; }

            foreach (KeyValuePair<Vector2, TileView> kvp in _baseTiles)
            {
                if (kvp.Key == position) { continue; }
                if (centerTile.DistanceToTile(kvp.Value) <= Constants.DistanceBetweenTiles * radius) { targetTiles.Add(kvp.Value); }
            }

            return targetTiles;
        }

        [PunRPC]
        public void RPC_InitiateCombo(Vector2 startingPos)
        {
            bool trapped = false;
            
            if (IsGameRelevant()) //Both host and client execute this if statement.
            {
                Player targetPlayer = GetNextPlayer(_curPlayer);
                if (targetPlayer.localID == 0)
                    targetPlayer.playerInterface.SetHitpoints(healthPlayerOne);
                else
                    targetPlayer.playerInterface.SetHitpoints(healthPlayerTwo);

                List<Vector2> trapPos = new List<Vector2>();
                foreach (KeyValuePair<Vector2, TileView> kvp in _selectedTiles)
                {
                    if (TileViewAtPos(kvp.Key).boosterLevel >= 5) {
                        trapped = true;
                        trapPos.Add(kvp.Value.transform.localPosition);
                    }
                }

                if (trapped && trapPos.Count > 0) // TODO: perhaps sending one event with the list of positions, instead of an event for every position, is faster
                {
                    if (EnemyPlayer == targetPlayer) {
                        foreach (Vector2 pos in trapPos)
                        {
                            UIEvent.LocalTrapTrigger(pos);
                        }
                    } else if (MyPlayer == targetPlayer)
                    {
                        foreach (Vector2 pos in trapPos)
                        {
                            UIEvent.OpponentTrapTrigger(pos);
                        }
                    }
                }

                // int boosterCount = 0;
                //Dictionary<Vector2, TileView> boostedTiles = new Dictionary<Vector2, TileView>();
                int count = 0;
                int highestCount = 0;
                foreach (KeyValuePair<Vector2, TileView> kvp in _selectedTiles)
                {
                    CreateTileAttackPlayerEffect(kvp.Key, count, false, trapped);
                    TileView baseTile = TileViewAtPos(kvp.Key);

                    if (baseTile.boosterLevel > 0)
                    {
                        // if (baseTile.boosterLevel < 4) { if (!boostedTiles.ContainsKey(baseTile.position)) { boosterCount++; Debug.Log("Adding to booster count"); } }
                        List<TileView> indivCollateral = baseTile.ListCollateralDamage(this, 1f);
                        foreach (TileView colTile in indivCollateral)
                        {
                            // if (colTile.boosterLevel > 0 & colTile.boosterLevel < 4) { if (!boostedTiles.ContainsKey(colTile.position)) { boosterCount++; Debug.Log("Adding to booster count"); } }
                            int hCount = count + Mathf.RoundToInt(colTile.DistanceToTile(baseTile) / Constants.DistanceBetweenTiles);
                            CreateTileAttackPlayerEffect(colTile.position, hCount, true, trapped);

                            if (hCount > highestCount)
                                highestCount = hCount;
                        }
                    }

                    count++;
                    if (count > highestCount)
                        highestCount = count;
                }

                /*
                Debug.Log("boosterCount: " + boosterCount.ToString());
                if (boosterCount > 0 && EnemyPlayer == targetPlayer)
                {
                    Debug.Log("Counter is bigger than 0 && EnemyPlayer == targetPlayer");
                    if (boosterCount == 2) { UIEvent.BoosterTriggerDouble(); }
                    else if (boosterCount == 3) { UIEvent.BoosterTriggerTriple(); }
                    else if (boosterCount >= 4) { UIEvent.BoosterTriggerMulti(); }
                }
                */

                foreach (KeyValuePair<Vector2, TileView> kvp in _baseTiles)
                {
                    if (kvp.Value != null && kvp.Value.collateral && !kvp.Value.isBeingDestroyed) { CreateTileAttackPlayerEffect(kvp.Key, highestCount, true, trapped); }
                }
            }

            if (IsGameMaster())
            {
                Debug.LogError("IsGameMaster()");
                trapped = false;
                //Only master client will update the _grid and then sync it.
                int color = _selectedTiles[startingPos].color;
                foreach (KeyValuePair<Vector2, TileView> kvp in _selectedTiles)
                {
                    DestroyTileAtPosition(kvp.Key);
                    if (TileViewAtPos(kvp.Key).boosterLevel >= 5)
                        trapped = true;
                }

                foreach (KeyValuePair<Vector2, TileView> kvp in _collateralTiles)
                {
                    DestroyTileAtPosition(kvp.Key);
                }
                Debug.LogError("DestroyedAll()");

                CreateBooster(startingPos, _selectedTiles.Count, color);

                if ((_curPlayer == 0 && !trapped) || (_curPlayer == 1 && trapped))
                {
                    DamagePlayerWithCombo(1, _selectedTiles.Count, _collateralTiles.Count * Constants.BoosterCollateralDamage);
                    FillPowerBar(0, color, _selectedTiles.Count);
                }
                else
                {
                    DamagePlayerWithCombo(0, _selectedTiles.Count, _collateralTiles.Count * Constants.BoosterCollateralDamage);
                    FillPowerBar(1, color, _selectedTiles.Count);
                }
            }
            
            EndTurn();
            _selectedTiles.Clear();
            TileView.areaList.Clear();
        }

        private void CreateTileAttackPlayerEffect(Vector2 pos, int count, bool collateral, bool trapped)
        {
            
            Player targetPlayer = null;
            foreach (Player player in PlayerManager.instance.GetAllPlayers().Values)
            {
                if ((trapped && player.localID == _curPlayer) || (!trapped && player.localID != _curPlayer))
                {
                    targetPlayer = player;
                }
            }

            TileView baseTile = TileViewAtPos(pos);

            if (targetPlayer == null || baseTile == null) { Debug.LogWarning("Could not find player(" + (targetPlayer == null).ToString() + ") or tile(" + (baseTile == null).ToString() + ")"); }

            GameObject go = Instantiate(tileDebrisPrefab);
            go.transform.position = baseTile.transform.position;
            go.GetComponent<TileExplosion>().Init(targetPlayer, count, baseTile.HexSprite(TileTypes.EColor.yellow + baseTile.color));

            baseTile.isBeingDestroyed = true;

            if (!collateral)
            {
                GameObject boosterFiller = Instantiate(boosterFillerPrefab);
                boosterFiller.transform.position = baseTile.transform.position;
                boosterFiller.GetComponent<BoosterFiller>().Init(targetPlayer.opponent, count, baseTile.color);
            }

            //Explosion effect
            
            GameObject expl = null;
            if (baseTile.boosterLevel == 1 || baseTile.boosterLevel == 2) {
                expl = Instantiate(boosterExplosion1Prefab);
            }
            else if (baseTile.boosterLevel == 3)
            {
                expl = Instantiate(boosterExplosion2Prefab);
            }
            else if (baseTile.boosterLevel == 4)
            {
                expl = Instantiate(boosterExplosion3Prefab);
            }
            else if (baseTile.boosterLevel >= 5)
                expl = Instantiate(boosterExplosionTrapPrefab);

            
            TimedEffect timedEff = Instantiate(tileDestroyedTimerPrefab).GetComponent<TimedEffect>();
            timedEff.createAfterTime = count * Constants.DelayAfterTileDestruction;
            timedEff.basetileToHide = baseTile;

            /*
            if (expl != null)
            {
                if (baseTile.boosterLevel < 4 && baseTile.boosterLevel > 0)
                {
                    UIEvent.BoosterTrigger(baseTile.transform.localPosition, baseTile.boosterLevel);
                }
                expl.transform.position = baseTile.transform.position;
            }
            */
            timedEff.transform.position = baseTile.transform.position;
        }

        private void DestroyTileAtPosition(Vector2 pos)
        {
            _grid.data[(int)pos.x, (int)pos.y].color = Constants.AmountOfColors; //Equals being 'destroyed'
            _grid.data[(int)pos.x, (int)pos.y].boosterLevel = 0;
        }

        private void CreateBooster(Vector2 pos, int comboCount, int color)
        {
            if (comboCount >= Constants.BoosterThreeThreshhold)
                _grid.data[(int)pos.x, (int)pos.y].boosterLevel = 4;
            else if (comboCount >= Constants.BoosterTwoThreshhold)
                _grid.data[(int)pos.x, (int)pos.y].boosterLevel = 3;
            else if (comboCount >= Constants.BoosterDiagonalThreshhold)
                _grid.data[(int)pos.x, (int)pos.y].boosterLevel = 2;
            else if (comboCount >= Constants.BoosterOneThreshhold)
                _grid.data[(int)pos.x, (int)pos.y].boosterLevel = 1;
            else
                _grid.data[(int)pos.x, (int)pos.y].boosterLevel = 0;

            if (comboCount >= Constants.BoosterOneThreshhold)
                _grid.data[(int)pos.x, (int)pos.y].color = color;

            Tile tile = _grid.data[(int)pos.x, (int)pos.y];
            photonView.RPC("RPC_SendTile", PhotonTargets.All, tile);
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
                }

            }

            if (healthPlayerOne <= 0 || healthPlayerTwo <= 0)
                _gameDone = true;


        }

        public void DamagePlayer(int playerNumber, float damage) //only master-client side
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
                }

                GetPlayerByID(playerNumber).playerInterface.SetHitpoints(healthPlayerOne);
                GetPlayerByID(playerNumber).playerInterface.UpdateShadowhealth(healthPlayerOne);
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
                }

                GetPlayerByID(playerNumber).playerInterface.SetHitpoints(healthPlayerTwo);
                GetPlayerByID(playerNumber).playerInterface.UpdateShadowhealth(healthPlayerTwo);
                photonView.RPC("RPC_UpdateHealth", PhotonTargets.Others, playerNumber, healthPlayerTwo);
            }

            if (healthPlayerOne <= 0 || healthPlayerTwo <= 0)
            {
                _gameDone = true;
                EndTurn(); //Fireball should end the game.
                _selectedTiles.Clear();
                TileView.areaList.Clear();
            }
        }

        [PunRPC]
        private void RPC_UpdateHealth(int playerNumber, float hitpoints)
        {
            if (IsGameRelevant())
            {
                GetPlayerByID(playerNumber).playerInterface.SetHitpoints(hitpoints);
                GetPlayerByID(playerNumber).playerInterface.UpdateShadowhealth(hitpoints);
            }
        }

        public bool IsMyTurn()
        {
            if (_myPlayer == null)
                return false;

            if (_myPlayer.localID == _curPlayer && gameState.State != GameStates.EGameState.interim)
                return true;
            return false;
        }

        private void FillPowerBar(int playerNumber, int color, int increaseBy)
        {
            //Executed Master Client side only.
            if (playerNumber == 0)
            {
                if (color == 1)
                {
                    P1_PowerBlue = Mathf.Min(P1_PowerBlue + increaseBy, Constants.BluePowerReq);
                    photonView.RPC("RPC_FillPower", PhotonTargets.All, playerNumber, color, P1_PowerBlue);
                }
                else if (color == 2)
                {
                    P1_PowerGreen = Mathf.Min(P1_PowerGreen + increaseBy, Constants.GreenPowerReq);
                    photonView.RPC("RPC_FillPower", PhotonTargets.All, playerNumber, color, P1_PowerGreen);
                }
                else if (color == 3)
                {
                    P1_PowerRed = Mathf.Min(P1_PowerRed + increaseBy, Constants.RedPowerReq);
                    photonView.RPC("RPC_FillPower", PhotonTargets.All, playerNumber, color, P1_PowerRed);
                }
                else if (color == 0)
                {
                    P1_PowerYellow = Mathf.Min(P1_PowerYellow + increaseBy, Constants.YellowPowerReq);
                    photonView.RPC("RPC_FillPower", PhotonTargets.All, playerNumber, color, P1_PowerYellow);
                }
            }
            else
            {
                if (color == 1)
                {
                    P2_PowerBlue = Mathf.Min(P2_PowerBlue + increaseBy, Constants.BluePowerReq);
                    photonView.RPC("RPC_FillPower", PhotonTargets.All, playerNumber, color, P2_PowerBlue);
                }
                else if (color == 2)
                {
                    P2_PowerGreen = Mathf.Min(P2_PowerGreen + increaseBy, Constants.GreenPowerReq);
                    photonView.RPC("RPC_FillPower", PhotonTargets.All, playerNumber, color, P2_PowerGreen);
                }
                else if (color == 3)
                {
                    P2_PowerRed = Mathf.Min(P2_PowerRed + increaseBy, Constants.RedPowerReq);
                    photonView.RPC("RPC_FillPower", PhotonTargets.All, playerNumber, color, P2_PowerRed);
                }
                else if (color == 0)
                {
                    P2_PowerYellow = Mathf.Min(P2_PowerYellow + increaseBy, Constants.YellowPowerReq);
                    photonView.RPC("RPC_FillPower", PhotonTargets.All, playerNumber, color, P2_PowerYellow);
                }
            }
        }

        [PunRPC]
        public void RPC_PowerClicked(SkillColor color, PhotonMessageInfo info)
        {
            if (IsGameMaster()) // Local player is master
            {
                int skillRequirement = Constants.GetSkillActivationRequirement(color);

                if (info.sender == PhotonNetwork.player) // Local player sent this
                {
                    if (_curPlayer == 0)
                    {
                        if (color == SkillColor.Blue) //blue
                        {
                            if (P1_PowerBlue >= skillRequirement)
                            {
                                P1_ShieldActive = true;
                                photonView.RPC("RPC_ShieldActivated", PhotonTargets.All, 0);
                                P1_PowerBlue = 0;
                                photonView.RPC("RPC_EmptyPower", PhotonTargets.All, _curPlayer, color);
                            }
                            else { UIEvent.SkillNotFull(color); }
                        }
                        else if (color == SkillColor.Green) //green
                        {
                            if (P1_PowerGreen >= skillRequirement)
                            {
                                healthPlayerOne += Constants.HealPower;
                                photonView.RPC("RPC_HealEffect", PhotonTargets.All, 0);
                                P1_PowerGreen = 0;
                                photonView.RPC("RPC_EmptyPower", PhotonTargets.All, _curPlayer, color);

                                photonView.RPC("RPC_UpdateHealth", PhotonTargets.All, _curPlayer, healthPlayerOne);
                            }
                            else { UIEvent.SkillNotFull(color); }
                        }
                        else if (color == SkillColor.Red) //red
                        {
                            if (P1_PowerRed >= skillRequirement)
                            {
                                photonView.RPC("RPC_CreateTrap", PhotonTargets.All, _curPlayer);
                                P1_PowerRed = 0;
                                photonView.RPC("RPC_EmptyPower", PhotonTargets.All, _curPlayer, color);

                                if (IsGameRelevant())
                                {
                                    _gameContext.ShowText("You placed a trap. Don't trigger it yourself!");
                                    photonView.RPC("RPC_MessageTrap", PhotonTargets.Others);
                                }
                            }
                            else { UIEvent.SkillNotFull(color); }
                        }
                        else if (color == SkillColor.Yellow) //yellow
                        {
                            if (P1_PowerYellow >= skillRequirement)
                            {
                                photonView.RPC("RPC_Create_Fireball", PhotonTargets.All, info.sender.ID);
                                P1_PowerYellow = 0;
                                photonView.RPC("RPC_EmptyPower", PhotonTargets.All, _curPlayer, color);
                            }
                            else { UIEvent.SkillNotFull(color); }
                        }
                    }
                }
                else
                {
                    if (_curPlayer != 0)
                    {
                        if (color == SkillColor.Blue && P2_PowerBlue >= Constants.BluePowerReq) //blue
                        {
                            P2_ShieldActive = true;
                            photonView.RPC("RPC_ShieldActivated", PhotonTargets.All, 1);
                            P2_PowerBlue = 0;
                            photonView.RPC("RPC_EmptyPower", PhotonTargets.All, _curPlayer, color);
                        }
                        else if (color == SkillColor.Green && P2_PowerGreen >= Constants.GreenPowerReq) //green
                        {
                            healthPlayerTwo += Constants.HealPower;
                            photonView.RPC("RPC_HealEffect", PhotonTargets.All, 1);
                            P2_PowerGreen = 0;
                            photonView.RPC("RPC_EmptyPower", PhotonTargets.All, _curPlayer, color);

                            photonView.RPC("RPC_UpdateHealth", PhotonTargets.Others, _curPlayer, healthPlayerTwo);
                        }
                        else if (color == SkillColor.Red && P2_PowerRed >= Constants.RedPowerReq) //red
                        {
                            photonView.RPC("RPC_CreateTrap", PhotonTargets.All, _curPlayer);
                            P2_PowerRed = 0;
                            photonView.RPC("RPC_EmptyPower", PhotonTargets.All, _curPlayer, color);

                            if (IsGameRelevant())
                            {
                                UIEvent.OpponentTrapPlaced();
                                // _gameContext.ShowText("Opponent placed a trap. Be careful!");
                                // _gameContext.ShowLargeText("Trap placed, careful!");
                                iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.Warning);
                            }
                        }
                        else if (color == SkillColor.Yellow && P2_PowerYellow >= Constants.YellowPowerReq) //yellow
                        {
                            photonView.RPC("RPC_Create_Fireball", PhotonTargets.All, info.sender.ID);
                            P2_PowerYellow = 0;
                            photonView.RPC("RPC_EmptyPower", PhotonTargets.All, _curPlayer, color);
                        }
                    }
                }
            }
        }

        private void CreateFireball(int fireballOwnerId)
        {
            Player fireballOwner = PlayerManager.instance.GetPlayerById(fireballOwnerId);
            if (fireballOwner == null) { return; }
            Vector3 startingPos = fireballOwner.playerInterface.GetSkillButtonBySkillColor(SkillColor.Yellow).transform.position;
            Transform enemyAvatarTransform = fireballOwner.opponent.playerInterface.avatarGameObject.transform;
            YellowPower fireball = Instantiate(fireballPrefab, startingPos, Quaternion.identity).GetComponent<YellowPower>();

            fireball.ownerPlayer = fireballOwner;
            fireball.name = "Fireball" + fireballOwner.localID;
            fireball.transform.position = startingPos;
            fireball.target = enemyAvatarTransform;

            fireball.Init();
        }

        [PunRPC]
        private void RPC_Create_Fireball(int fireballOwner)
        {
            CreateFireball(fireballOwner);
        }

        [PunRPC]
        public void RPC_FireballHit(int fireballOwnerId) //should be both master-client side and guest-side
        {
            Debug.Log("Fireball Hit!");
            Player fireballOwnerPlayer = PlayerManager.instance.GetPlayerById(fireballOwnerId);

            if (fireballOwnerPlayer == null) { Debug.Log("Fireball owner left. Never mind"); return; }

            if (IsGameRelevant())
            {
                Debug.Log("Game is relevant!");
                GameObject explosion = Instantiate(Resources.Load("ParticleEffects/FireballHit")) as GameObject;
                explosion.transform.position = fireballOwnerPlayer.opponent.playerInterface.avatarGameObject.transform.position;
            }

            if (IsGameMaster())
            {
                DamagePlayer(fireballOwnerPlayer.opponent.localID, Constants.FireballPower);
                ResetTimer();
            }

            iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.ImpactHeavy);
        }

        private void CreateTrap()
        {
            GameObject trapGO = Instantiate(Resources.Load("TrapPower")) as GameObject;

            trapGO.GetComponent<TrapPower>().ownerPlayer = MyPlayer;
            trapGO.GetComponent<TrapPower>().gameID = GameID;
            trapGO.name = "TrapPower" + MyPlayer.localID;
            trapGO.transform.position = PlayerManager.instance.GetPlayerById(PhotonNetwork.player.ID).playerInterface.GetSkillButtonBySkillColor(SkillColor.Red).transform.position;
        }

        [PunRPC]
        private void RPC_CreateTrap(int playerNo)
        {
            if (GetPlayerByID(playerNo) == MyPlayer) {
                CreateTrap();
                // _gameContext.ShowText("You placed a trap. Don't trigger it yourself!");
            }
        }

        [PunRPC]
        public void RPC_CreateTrapBooster(Vector2 pos, int creatorPlayer)
        {
            if (IsGameMaster())
            {
                _grid.data[(int)pos.x, (int)pos.y].boosterLevel = 5 + creatorPlayer;
                Tile tile = _grid.data[(int)pos.x, (int)pos.y];
                photonView.RPC("RPC_SendTile", PhotonTargets.All, tile);
            }
        }

        [PunRPC]
        private void RPC_TurnEnded(int curPlayer)
        {
            if (IsGameRelevant())
            {
                if (_myPlayer != null)
                {
                    UIEvent.TurnChange(curPlayer == _myPlayer.localID);
                }

                if (MyPlayer.localID == 0)
                {
                    MyPlayer.playerInterface.UpdateShadowhealth(healthPlayerOne);
                    EnemyPlayer.playerInterface.UpdateShadowhealth(healthPlayerTwo);
                }
                else
                {
                    MyPlayer.playerInterface.UpdateShadowhealth(healthPlayerTwo);
                    EnemyPlayer.playerInterface.UpdateShadowhealth(healthPlayerOne);
                }
            }
        }

        [PunRPC]
        private void RPC_AnimateTile(AnimateTile tile)
        {
            if (IsGameRelevant())
            {
                TileView dropIntoTile = TileViewAtPos(new Vector2(tile.x, tile.y));

                //Debug.Log("Anim request: " + tile + " | " + tile.x + ", " + tile.y + " > " + tile.color);

                dropIntoTile.color = tile.color;
                dropIntoTile.Animate(tile.fallDistance);
            }
        }

        [PunRPC]
        private void RPC_EndGame(int winnerPlayer)
        {
            if (IsGameRelevant())
            {
                MyPlayer.playerInterface.shieldEffect.SetActive(false);
                GameObject.Find("PlayScreen").GetComponent<PlayGameCanvas>().EndGame(winnerPlayer);
                _gameDone = false;
                _isActive = false;
            }
        }

        [PunRPC]
        private void RPC_FillPower(int playerNo, int color, int value)
        {
            if (playerNo == 0)
            {
                if (color == 1)
                    P1_PowerBlue = value;
                else if (color == 2)
                    P1_PowerGreen = value;
                else if (color == 3)
                    P1_PowerRed = value;
                else if (color == 0)
                    P1_PowerYellow = value;
            } else
            {
                if (color == 1)
                    P2_PowerBlue = value;
                else if (color == 2)
                    P2_PowerGreen = value;
                else if (color == 3)
                    P2_PowerRed = value;
                else if (color == 0)
                    P2_PowerYellow = value;
            }
        }

        [PunRPC]
        private void RPC_EmptyPower(int playerNo, SkillColor color)
        {
            if (playerNo == 0)
            {
                if (color == SkillColor.Blue)
                    P1_PowerBlue = 0;
                else if (color == SkillColor.Green)
                    P1_PowerGreen = 0;
                else if (color == SkillColor.Red)
                    P1_PowerRed = 0;
                else if (color == SkillColor.Yellow)
                    P1_PowerYellow = 0;
            } else
            {
                if (color == SkillColor.Blue)
                    P2_PowerBlue = 0;
                else if (color == SkillColor.Green)
                    P2_PowerGreen = 0;
                else if (color == SkillColor.Red)
                    P2_PowerRed = 0;
                else if (color == SkillColor.Yellow)
                    P2_PowerYellow = 0;
            }
        }


        [PunRPC]
        public void RPC_SendRematchRequest()
        {
            if (IsGameRelevant())
            {
                Debug.Log("Rematch order gotten, reloading Scene.");
                _rematchRequested = true;
            }
        }

        [PunRPC]
        private void RPC_ShieldEffect(int targetPlayer)
        {
            if (IsGameRelevant())
            {
                if (targetPlayer == MyPlayer.localID)
                    MyPlayer.playerInterface.shieldEffect.SetActive(false);

                GameObject effect = Instantiate(Resources.Load("ParticleEffects/ShieldEffect")) as GameObject;
                Vector2 effectPosition = new Vector2();
                if (_myPlayer.localID == targetPlayer)
                    effectPosition = MyPlayer.playerInterface.avatarGameObject.transform.position;
                else
                    effectPosition = EnemyPlayer.playerInterface.avatarGameObject.transform.position;
                effect.transform.position = effectPosition;

                UIEvent.ShieldHit(targetPlayer == MyPlayer.localID);
            }
        }

        [PunRPC]
        private void RPC_ShieldActivated(int targetPlayer)
        {
            if (IsGameRelevant())
            {
                if (_myPlayer.localID == targetPlayer)
                {
                    MyPlayer.playerInterface.shieldEffect.SetActive(true);
                    // _gameContext.ShowText("Shield is now active. Next damage is blocked.");
                    UIEvent.ShieldActivate(true);
                }
            }
        }

        [PunRPC]
        private void RPC_HealEffect(int targetPlayer)
        {
            if (IsGameRelevant())
            {
                GameObject effect = Instantiate(Resources.Load("ParticleEffects/HealEffect")) as GameObject;
                Vector2 effectPosition = new Vector2();

                bool isLocal = _myPlayer.localID == targetPlayer;

                UIEvent.Heal(isLocal);

                if (isLocal)
                {
                    // _gameContext.ShowText("You healed for " + Constants.HealPower + "!");

                    effectPosition = MyPlayer.playerInterface.avatarGameObject.transform.position;
                }
                else
                {
                    // _gameContext.ShowText("Opponent healed for " + Constants.HealPower + "!");

                    effectPosition = GameObject.Find("OpponentAvatar").GetComponent<RectTransform>().position;
                }
                effect.transform.position = effectPosition;
            }
        }

        private bool IsGameMaster()
        {
            if (_myPlayer != null)
            {
                //if (PhotonNetwork.isMasterClient || _myPlayer.joinNumber == 3) //okay
                if (PhotonNetwork.isMasterClient) //okay
                    return true;
                else
                    return false;
            }
            else
            {
                if (PhotonNetwork.isMasterClient) //okay
                    return true;
                else
                    return false;
            }

            //photonView.isMine eventually, if player 3 creates his own Grid. This is the way to go I think.
        }

        private bool IsGameRelevant()
        {
            if (_myPlayer != null)
            {
                return _myPlayer.IsRelevantGame(_gameID);
            }
            return false;
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
            if (IsGameRelevant())
            {
                if (comboDamage > 300)
                    iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.ImpactHeavy);
            }
        }

        [PunRPC]
        private void RPC_MessageTrap()
        {
            if (IsGameRelevant())
            {
                UIEvent.OpponentTrapPlaced();
                //_gameContext.ShowText("Opponent placed a trap. Be careful!");
                //_gameContext.ShowLargeText("Trap placed, careful!");
                iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.Warning);
            }
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

        public int GetSkillChargeAmount(SkillColor skillColor, int localId)
        {
            int chargeAmount = 0;
            switch (skillColor)
            {
                case SkillColor.Red:
                    chargeAmount = localId == 0 ? P1_PowerRed : P2_PowerRed;
                    break;
                case SkillColor.Green:
                    chargeAmount = localId == 0 ? P1_PowerGreen : P2_PowerGreen;
                    break;
                case SkillColor.Blue:
                    chargeAmount = localId == 0 ? P1_PowerBlue : P2_PowerBlue;
                    break;
                case SkillColor.Yellow:
                    chargeAmount = localId == 0 ? P1_PowerYellow : P2_PowerYellow;
                    break;
            }
            return chargeAmount;
        }

        public void UpdateCollateral(TileView tile)
        {
            if (tile.collateral) { if (!_collateralTiles.ContainsKey(tile.position)) { _collateralTiles.Add(tile.position, tile); } }
            else { if (_collateralTiles.ContainsKey(tile.position)) { _collateralTiles.Remove(tile.position); } }
        }
    }
}
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
        private Grid _grid;
        private int _curPlayer;
        private List<BaseTile> _baseTiles;
        private Player _myPlayer;
        private Player _enemyPlayer;
        private List<Vector2> _selectedTiles;
        private List<Vector2> _collateralTiles;

        private bool _gridReceived = false;
        private bool _gridVisualized = false;
        #endregion

        #region public game logic
        public Player MyPlayer { get { return _myPlayer; } }

        public float healthPlayerOne;
        public float healthPlayerTwo;
        public float turnTimer;
        #endregion

        public delegate byte[] SerializeMethod(object customObject);
        public delegate object DeserializeMethod(byte[] serializedCustomObject);

        private void Start()
        {
            PhotonPeer.RegisterType(typeof(Tile), (byte)'L', SerializeTile, DeserializeTile);
            PhotonPeer.RegisterType(typeof(AnimateTile), (byte)'A', SerializeAnimTile, DeserializeAnimTile);
            PhotonPeer.RegisterType(typeof(Grid), (byte)'G', SerializeGrid, DeserializeGrid);

            _isActive = false;
            _selectedTiles = new List<Vector2>();
            _curPlayer = 0;

            turnTimer = 0;
            healthPlayerOne = Constants.PlayerStartHP;
            healthPlayerTwo = Constants.PlayerStartHP;

            _grid = new Grid();
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

                // TODO more efficiency ? 
                /*foreach (BaseTile tile in FindObjectsOfType<BaseTile>())
                {
                    tile.SetSelected = false;
                    if (_selectedTiles.Contains(tile.position))
                    {
                        tile.SetSelected = true;
                    }
                }*/

                if (IsMyTurn())
                    GameObject.Find("CurPlayer").GetComponent<Text>().text = "My turn";
                else
                    GameObject.Find("CurPlayer").GetComponent<Text>().text = "Wait...";
            }
        }

        private void FixedUpdate()
        {
            if (_isActive)
            {
                if (turnTimer > Constants.TurnTime)
                {
                    EndTurn();
                }
                else
                    turnTimer += Time.fixedDeltaTime;
            }
        }

        public void Show()
        {
            if (!_isActive)
            {
                _isActive = true;
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


                Debug.Log(_myPlayer.localID);
                if (_myPlayer.localID == 1 && !PhotonNetwork.isMasterClient)
                {
                    transform.rotation = new Quaternion(0f, 0f, 180f, transform.rotation.w);
                } else
                {
                    transform.rotation = new Quaternion(0f, 0f, 0f, transform.rotation.w);
                }

                photonView.RPC("RPCTurnWarning", PhotonTargets.All, _curPlayer);
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
                    healthPlayerOne = (float)stream.ReceiveNext();
                    healthPlayerTwo = (float)stream.ReceiveNext();
                }
            }
        }

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
                                Debug.Log("booster at GridUpdate: " + _grid.data[x, y].boosterLevel);
                                tile.SetSelected = false;
                            }
                            else { 
                                tile.GetComponent<Image>().enabled = false;
                            }
                        }
                    }
                }
            }
        }

        private void EndTurn()
        {
            turnTimer = 0f;

            //Master client will sync this.
            if (PhotonNetwork.isMasterClient)
            {
                if (_curPlayer == 0)
                    _curPlayer = 1;
                else
                    _curPlayer = 0;

                photonView.RPC("RPCTurnWarning", PhotonTargets.All, _curPlayer);

                /*if (_refill == null)
                {
                    _refillChanges = false;
                    _refillTime = Time.time + _refillInterval;
                    _refill = StartCoroutine(RefillCoroutine());
                }*/

                Refill();
            }
        }

        //Refill function
        private void Refill()
        {
            Debug.Log("---- Refill v2 ----");
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

        private Grid DuplicateGrid ()
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
        private Grid ShiftDownGrid (Grid dupli)
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
                            dupli.data[(int)dropIntoPos.x, (int)dropIntoPos.y].boosterLevel = boosterToAssume; //Set booster of the target position to this tile.

                            if (y + emptyTilesBelow <= topRow) { //Possible to take color from above?
                                dupli.data[x, y].color = _grid.data[x, y + emptyTilesBelow].color;
                                if (_grid.data[x, y + emptyTilesBelow].boosterLevel > 0)
                                    dupli.data[x, y].boosterLevel = _grid.data[x, y + emptyTilesBelow].boosterLevel;
                            }
                            else { 
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
                        //dupli.data[x, y].boosterLevel = 0;
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
                            dupli.data[(int)dropIntoPos.x, (int)dropIntoPos.y].boosterLevel = boosterToAssume; //Set booster of the target position to this tile.

                            if (y - emptyTilesAbove >= 0)
                            {
                                dupli.data[x, y].color = _grid.data[x, y - emptyTilesAbove].color;
                                if (_grid.data[x, y - emptyTilesAbove].boosterLevel > 0)
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
                        //dupli.data[x, y].boosterLevel = 0;
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
                for (int y = topRow; y >= 0; y--) { 
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

        public void AddToSelection(Vector2 pos)
        {
            _selectedTiles.Add(pos);
            BaseTileAtPos(pos).SetSelected = true;

            RecalculateCollateral();
        }

        public void RemoveFromSelection(Vector2 pos)
        {
            _selectedTiles.Remove(pos);
            BaseTileAtPos(pos).SetSelected = false;

            if (BaseTileAtPos(pos).boosterLevel > 0)
                RecalculateCollateral();
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

        public void RemoveSelections()
        {
            _selectedTiles.Clear();
            foreach (BaseTile tile in FindObjectsOfType<BaseTile>())
            {
                tile.SetSelected = false;
                tile.collateral = false;
            }
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

            //Only master client will update the _grid and then sync it.
            if (PhotonNetwork.isMasterClient)
            {
                foreach (Vector2 pos in _selectedTiles)
                {
                    DestroyTileAtPosition(pos);
                }

                foreach (BaseTile tile in _baseTiles.FindAll(item => item.collateral == true))
                {
                    DestroyTileAtPosition(tile.position);
                }

                CreateBooster(_selectedTiles[_selectedTiles.Count - 1], _selectedTiles.Count);

                if (_curPlayer == 0)
                {
                    DamagePlayerWithCombo(1, _selectedTiles.Count);
                    DamagePlayer(1, _baseTiles.FindAll(item => item.collateral == true).Count);
                } else {
                    DamagePlayerWithCombo(0, _selectedTiles.Count);
                    DamagePlayer(0, _baseTiles.FindAll(item => item.collateral == true).Count);
                }
            }

            foreach (Vector2 pos in _selectedTiles)
            {
                CreateTileAttackPlayerEffect(pos);
            }

            foreach (BaseTile tile in _baseTiles.FindAll(item => item.collateral == true))
            {
                CreateTileAttackPlayerEffect(tile.position);
            }


            EndTurn();
            _selectedTiles.Clear();
        }

        private void CreateTileAttackPlayerEffect (Vector2 pos)
        {
            GameObject go = Instantiate(Resources.Load("Explosion")) as GameObject;
            Player[] players = GameObject.FindObjectsOfType<Player>();
            Player targetPlayer = null;
            foreach (Player player in players)
            {
                if (player.localID != _curPlayer)
                {
                    targetPlayer = player;
                }
            }

            BaseTile baseTile = BaseTileAtPos(pos);
            go.transform.SetParent(transform, false); //or transform.parent? TODO
            go.transform.position = baseTile.transform.position;
            go.GetComponent<TileExplosion>().Init(targetPlayer, 1, baseTile.HexSprite(TileTypes.EColor.yellow + baseTile.color));
            baseTile.color = Constants.AmountOfColors;
        }

        private void DestroyTileAtPosition (Vector2 pos)
        {
            _grid.data[(int)pos.x, (int)pos.y].color = Constants.AmountOfColors; //Equals being 'destroyed'
            _grid.data[(int)pos.x, (int)pos.y].boosterLevel = 0;
        }
    
        private void CreateBooster (Vector2 pos, int comboCount)
        {
            if (comboCount >= Constants.BoosterThreeThreshhold)
                _grid.data[(int)pos.x, (int)pos.y].boosterLevel = 3;
            else if (comboCount >= Constants.BoosterTwoThreshhold)
                _grid.data[(int)pos.x, (int)pos.y].boosterLevel = 2;
            else if (comboCount >= Constants.BoosterOneThreshhold)
                _grid.data[(int)pos.x, (int)pos.y].boosterLevel = 1;
            else
                _grid.data[(int)pos.x, (int)pos.y].boosterLevel = 0;

            Tile tile = _grid.data[(int)pos.x, (int)pos.y];
            photonView.RPC("RPCSendTile", PhotonTargets.All, tile);
        }

        public void DamagePlayerWithCombo(int playerNumber, float comboSize)
        {
            if (playerNumber == 0)
                healthPlayerOne -= Mathf.Pow(comboSize, 2);
            else
                healthPlayerTwo -= Mathf.Pow(comboSize, 2);


            if (healthPlayerOne < 0 || healthPlayerTwo < 0)
                photonView.RPC("RPCEndGame", PhotonTargets.All);
        }

        public void DamagePlayer (int playerNumber, float damage)
        {
            if (playerNumber == 0)
                healthPlayerOne -= damage;
            else
                healthPlayerTwo -= damage;


            if (healthPlayerOne < 0 || healthPlayerTwo < 0)
                photonView.RPC("RPCEndGame", PhotonTargets.All);
        }

        public bool IsMyTurn()
        {
            if (_myPlayer == null)
                return false;

            if (_myPlayer.localID == _curPlayer)
                return true;
            return false;
        }

        public bool InTurnDelay()
        {
            /*if (_myPlayer.localID == _curPlayer)
                return true;*/
            return false;
        }


        [PunRPC]
        public void RPCSendTile(Tile tile)
        {
            _grid.data[tile.x, tile.y].color = tile.color;
            _grid.data[tile.x, tile.y].boosterLevel = tile.boosterLevel;
            //GridUpdate();
        }

        [PunRPC]
        public void RPCSendGridData(Grid grid)
        {
            _gridReceived = true;
            _grid.data = grid.data;
            GridUpdate();
        }

        [PunRPC]
        private void RPCTurnWarning(int curPlayer)
        {
            if (_myPlayer != null) { 
                if (curPlayer == _myPlayer.localID) //Reversed
                {
                    if (GameObject.FindGameObjectWithTag("TurnText") == null)
                    {
                        GameObject go = Instantiate(Resources.Load("UI/MyTurn")) as GameObject;
                        go.transform.SetParent(transform.parent, false);
                    }
                }
                else
                {
                    if (GameObject.FindGameObjectWithTag("TurnText") == null)
                    {
                        GameObject go = Instantiate(Resources.Load("UI/OpponentsTurn")) as GameObject;
                        go.transform.SetParent(transform.parent, false);
                        go.GetComponent<Text>().text = _enemyPlayer.GetName() + "'s turn!";
                    }
                }
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
        private void RPCEndGame ()
        {
            GameObject.Find("PlayScreen").GetComponent<PlayGameCanvas>().EndGame();
        }

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
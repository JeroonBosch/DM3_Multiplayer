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
        private List<Vector2> _selectedTiles;
        private float _turnTimer;
        private Coroutine _refill;
        private float _refillInterval;
        private float _refillTime;
        private bool _refillChanges;
        #endregion

        public Player MyPlayer { get { return _myPlayer; } }

        public delegate byte[] SerializeMethod(object customObject);
        public delegate object DeserializeMethod(byte[] serializedCustomObject);

        private void Start()
        {
            PhotonPeer.RegisterType(typeof(Tile), (byte)'L', SerializeTile, DeserializeTile);
            PhotonPeer.RegisterType(typeof(Grid), (byte)'G', SerializeGrid, DeserializeGrid);

            _isActive = false;
            _selectedTiles = new List<Vector2>();
            _curPlayer = 0;
            _turnTimer = 0;
            _refillInterval = .2f;
            _refillTime = float.PositiveInfinity;
        }

        private void Update()
        {
            foreach (BaseTile tile in FindObjectsOfType<BaseTile>())
            {
                int color = TileAtPos(new Vector2(tile.position.x, tile.position.y)).color;
                tile.GetComponent<Image>().sprite = HexSprite(TileTypes.EColor.yellow + color);
                if (_selectedTiles.Contains(tile.position))
                {
                    tile.GetComponent<Image>().sprite = HexSpriteSelected(TileTypes.EColor.yellow + color);
                }
            }

            GameObject.Find("CurPlayer").GetComponent<Text>().text = "Current player: " + _curPlayer; 
        }

        private void FixedUpdate()
        {
            if (_turnTimer > Constants.TurnTime)
            {
                EndTurn();
            } else
                _turnTimer += Time.fixedDeltaTime;
        }

        public void Show ()
        {
            Player[] players = FindObjectsOfType<Player>();
            foreach (Player player in players)
            {
                if (player.GetComponent<PhotonView>().isMine)
                    _myPlayer = player;
            }
            

            GenerateGrid();
            VisualizeGrid();

            _isActive = true;

            if (_myPlayer.localID == 1) { 
                transform.rotation = new Quaternion(0f, 0f, 180f, transform.rotation.w);
                GameObject go = Instantiate(Resources.Load("OpponentsTurn")) as GameObject;
                go.transform.SetParent(transform.parent, false);
            } else {
                GameObject go = Instantiate(Resources.Load("MyTurn")) as GameObject;
                go.transform.SetParent(transform.parent, false);
            }
        }

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (_isActive) { 
                if (stream.isWriting)
                {
                    stream.SendNext(_grid);
                    stream.SendNext(_curPlayer);
                }
                else
                {
                    _grid = (Grid)stream.ReceiveNext();
                    _curPlayer = (int)stream.ReceiveNext();
                    Debug.Log("Data received.");
                    GridUpdate();
                }
            }
        }

        void GenerateGrid ()
        {
            Debug.Log("GenerateGrid");

            _grid = new Grid();
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
            Debug.Log("VisualizeGrid");

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
                        tile.transform.localPosition = new Vector3((-Constants.gridXsize / 2 + x) * Constants.tileWidth + Constants.tileWidth/2, (-Constants.gridYsize / 2 + y) * Constants.tileHeight + (Constants.tileHeight * .75f), 0f);
                    else
                        tile.transform.localPosition = new Vector3((-Constants.gridXsize / 2 + x) * Constants.tileWidth + Constants.tileWidth/2, (-Constants.gridYsize / 2 + y) * Constants.tileHeight + (Constants.tileHeight * 0.25f), 0f);

                    if (_grid.data[x, y].color < Constants.AmountOfColors)
                    {
                        tile.GetComponent<Image>().enabled = true;
                        tile.GetComponent<Image>().sprite = HexSprite(TileTypes.EColor.yellow + _grid.data[x, y].color);
                    }
                    else
                        tile.GetComponent<Image>().enabled = false;
                }
            }
        }

        void GridUpdate ()
        {
            Debug.Log("GridUpdate");

            for (int x = 0; x < Constants.gridXsize; x++)
            {
                for (int y = 0; y < Constants.gridYsize; y++)
                {
                    Vector2 pos = new Vector2(x, y);
                    BaseTile tile = BaseTileAtPos(pos);
                    if (tile != null) { 
                        tile.gameObject.name = "Tile (" + x + "," + y + ")";
                        tile.transform.SetParent(transform, false);
                        tile.position = pos;
                        _baseTiles.Add(tile.GetComponent<BaseTile>());
                        if (x % 2 == 0)
                            tile.transform.localPosition = new Vector3((-Constants.gridXsize / 2 + x) * Constants.tileWidth + Constants.tileWidth / 2, (-Constants.gridYsize / 2 + y) * Constants.tileHeight + (Constants.tileHeight * .75f), 0f);
                        else
                            tile.transform.localPosition = new Vector3((-Constants.gridXsize / 2 + x) * Constants.tileWidth + Constants.tileWidth / 2, (-Constants.gridYsize / 2 + y) * Constants.tileHeight + (Constants.tileHeight * 0.25f), 0f);


                        if (_grid.data[x, y].color < Constants.AmountOfColors)
                        {
                            tile.GetComponent<Image>().enabled = true;
                            tile.GetComponent<Image>().sprite = HexSprite(TileTypes.EColor.yellow + _grid.data[x, y].color);
                        }
                        else
                            tile.GetComponent<Image>().enabled = false;
                    }
                }
            }
        }

        private void EndTurn ()
        {
            _turnTimer = 0f;
            
            if (_curPlayer == 0)
                _curPlayer = 1;
            else
                _curPlayer = 0;

            if (IsMyTurn()) {
                GameObject go = Instantiate(Resources.Load("MyTurn")) as GameObject;
                go.transform.SetParent(transform.parent, false);
            } else { 
                GameObject go = Instantiate(Resources.Load("OpponentsTurn")) as GameObject;
                go.transform.SetParent(transform.parent, false);
            }

            if (_refill == null)
            {
                _refillChanges = false;
                _refillTime = Time.time + _refillInterval;
                _refill = StartCoroutine(RefillCoroutine());
            }
        }

        private IEnumerator RefillCoroutine()
        {
            Debug.Log("RefillCoroutine() started.");
            while (Time.time < _refillTime) yield return null;
            Debug.Log("RefillCoroutine() done.");

            if (_curPlayer == 1) //since the player number already changed.
            {
                ShiftTilesDown();
            }
            else
            {
                ShiftTilesUp();
            }

            _refillTime = float.PositiveInfinity;
            _refill = null;

            if (_refillChanges)
            {
                Grid newGrid = new Grid();
                newGrid.data = _grid.data;
                _grid = newGrid; //TODO: This is a hack to trigger a server-side call

                _refillChanges = false;
                _refillTime = Time.time + _refillInterval;
                _refill = StartCoroutine(RefillCoroutine());
            }
        }

        private void ShiftTilesDown()
        { //0,0 is bottom left
            for (int x = 0; x < Constants.gridXsize; x++)
            {
                for (int y = 1; y < Constants.gridYsize; y++) //bottom row does not need shifting down. If there's 8 rows (0 to 7), then y should be 1 to 7, as row 0 is to be ignored.
                {
                    Tile tile = TileAtPos(new Vector2(x, y));

                    if (tile.color >= Constants.AmountOfColors) // Means it's destroyed.
                    {
                        _refillChanges = true;

                        if (y == Constants.gridYsize - 1) //Top row, create new ones.
                        {
                            _grid.data[x, y].color = UnityEngine.Random.Range(0, Constants.AmountOfColors); 
                        } else {
                            Debug.Log("Taking color from " + x + ", " + (y + 1));
                            _grid.data[x, y].color = _grid.data[x, y + 1].color; //Take color from above.
                            _grid.data[x, y + 1].color = Constants.AmountOfColors; //Above tile is set to invisible.
                        }
                    }
                }
            }
        }

        private void ShiftTilesUp()
        {
            for (int x = 0; x < Constants.gridXsize; x++)
            {
                for (int y = Constants.gridYsize - 2; y >= 0; y--) //top row does not need shifting up. If there's 8 rows (7 to 0), then y should be 6 to 0, as row 7 is to be ignored.
                {
                    Tile tile = TileAtPos(new Vector2(x, y));

                    if (tile.color >= Constants.AmountOfColors) // Means it's destroyed.
                    {
                        _refillChanges = true;

                        if (y == 0) //Bottom row, create new ones.
                        {
                            _grid.data[x, y].color = UnityEngine.Random.Range(0, Constants.AmountOfColors);
                        }
                        else
                        {
                            Debug.Log("Taking color from " + x + ", " + (y - 1));
                            _grid.data[x, y].color = _grid.data[x, y - 1].color; //Take color from below.
                            _grid.data[x, y - 1].color = Constants.AmountOfColors; //Below tile is set to invisible.
                        }
                    }
                }
            }
        }

        public Tile TileAtPos (Vector2 position)
        {
            return _grid.data[(int)position.x, (int)position.y];
        }

        public BaseTile BaseTileAtPos (Vector2 position)
        {
            return _baseTiles.Find(item => item.position.x == position.x && item.position.y == position.y); ;
        }

        public void AddToSelection(Vector2 pos)
        {
            _selectedTiles.Add(pos);
        }

        public void RemoveFromSelection (Vector2 pos)
        {
            _selectedTiles.Remove(pos);
        }

        public void RemoveSelections()
        {
            _selectedTiles.Clear() ;
        }

        public void InitiateCombo ()
        {
            foreach (Vector2 pos in _selectedTiles) {
                _grid.data[(int)pos.x, (int)pos.y].color = Constants.AmountOfColors;
            }

            EndTurn();
            _selectedTiles.Clear();
        }

        public bool IsMyTurn()
        {
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


        #region SpriteRendering
        public Sprite HexSprite (TileTypes.EColor color)
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

        public static readonly byte[] memGrid = new byte[Constants.gridXsize * Constants.gridYsize * 4 * 4];
        private static short SerializeGrid(StreamBuffer outStream, object customobject)
        {
            Grid sGrid = (Grid)customobject;
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
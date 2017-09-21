using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class TournamentScreenCanvas : BaseMenuCanvas
    {
        //Shows the tournament match-making tree. Replaces 'FindMatchCanvas'
        private bool _fullTournament;

        private GameObject _searchObject2;
        private GameObject _opponentAvatar2;

        private GameObject _searchObject3;
        private GameObject _opponentAvatar3;

        private GameObject _searchObject4;
        private GameObject _opponentAvatar4;

        private float _timer = 0f;
        private float _timeUntilStart = 3f;

        protected override void Start ()
        {
            base.Start();

            _fullTournament = false;

            _searchObject2 = GameObject.Find("FindingOpponent2");
            _opponentAvatar2 = GameObject.Find("player2");
            _searchObject3 = GameObject.Find("FindingOpponent3");
            _opponentAvatar3 = GameObject.Find("player3");
            _searchObject4 = GameObject.Find("FindingOpponent4");
            _opponentAvatar4 = GameObject.Find("player4");
            GameObject.Find("LookingForMatch").GetComponent<Text>().text = "Setting up Tournament..."; 

            if (GameObject.FindGameObjectsWithTag("Player").Length >= 2)
            {
                foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
                {
                    int joinNumber = player.GetComponent<Player>().joinNumber;
                    GameObject.Find("Player"+ joinNumber + "Name").GetComponent<Text>().text = player.GetComponent<Player>().profileName;
                }
            }

            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player"))
            {
                go.transform.Find("FingerTracker").GetComponent<Image>().enabled = false;
            }

            _opponentAvatar2.SetActive(false);
            _opponentAvatar3.SetActive(false);
            _opponentAvatar4.SetActive(false);
        }

        protected override void Update()
        {
            base.Update();

            Player[] players = FindObjectsOfType<Player>();
            foreach (Player player in players)
                player.UpdateLabels();

            if (players.Length == 2)
            {
                _opponentAvatar2.SetActive(true);
                _searchObject2.SetActive(false);
            }
            else if (players.Length == 3)
            {
                _opponentAvatar2.SetActive(true);
                _searchObject2.SetActive(false);
                _opponentAvatar3.SetActive(true);
                _searchObject3.SetActive(false);
            }
            else if (players.Length == 4)
            {
                if (!_fullTournament) //Do once.
                    TournamentFilled();

                _timer += Time.deltaTime;

                if (_timer > _timeUntilStart)
                {
                    AssignOpponent();
                    
                    PhotonController.Instance.SetTournamentOpponent(GetMyJoinNumber()); //set correct interest groups
                    PhotonNetwork.LoadLevel("Match");
                    Debug.Log("Match loaded from Tournament.");
                    enabled = false;
                }
            }
        }

        private void TournamentFilled ()
        {
            _fullTournament = true;
            GameObject.Find("LookingForMatch").GetComponent<Text>().text = "Players found!";

            _opponentAvatar2.SetActive(true);
            _searchObject2.SetActive(false);
            _opponentAvatar3.SetActive(true);
            _searchObject3.SetActive(false);
            _opponentAvatar4.SetActive(true);
            _searchObject4.SetActive(false);

            //TODO Placeholder avatars
            if (!PhotonNetwork.isMasterClient)
            {
                GameObject.Find("player1").GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/AvatarB");
                GameObject.Find("player2").GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/AvatarA");
                GameObject.Find("player3").GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/AvatarB");
                GameObject.Find("player4").GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/AvatarA");
            }
        }

        private int GetMyJoinNumber()
        {
            foreach (Player player in FindObjectsOfType<Player>())
            {
                if (player.photonView.isMine)
                {
                    return player.joinNumber;
                }
            }
            return -1;
        }

        private void AssignOpponent ()
        {
            foreach (Player player in FindObjectsOfType<Player>())
            {
                if (player.photonView.isMine)
                {
                    if (player.joinNumber == 1)
                    {
                        player.opponent = GetPlayerWithJoinNumber(2);
                    }
                    else if (player.joinNumber == 2)
                    {
                        player.opponent = GetPlayerWithJoinNumber(1);
                    }
                    else if (player.joinNumber == 3)
                    {
                        player.opponent = GetPlayerWithJoinNumber(4);
                    }
                    else if (player.joinNumber == 4)
                    {
                        player.opponent = GetPlayerWithJoinNumber(3);
                    }
                }
            }
        }

        private Player GetPlayerWithJoinNumber(int joinNum)
        {
            foreach (Player player in FindObjectsOfType<Player>())
            {
                if (player.joinNumber == joinNum)
                {
                    return player;
                }
            }
            return null;
        }
    }
}
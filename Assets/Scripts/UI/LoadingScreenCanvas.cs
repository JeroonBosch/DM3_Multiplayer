using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class LoadingScreenCanvas : BaseMenuCanvas
    {
        private bool _playersReady;

        private float _timer = 0f;
        private float _timeUntilStart = 2f;

        protected override void Start ()
        {
            base.Start();

            _playersReady = false;

            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player"))
            {
                go.transform.Find("FingerTracker").GetComponent<Image>().enabled = false;
            }

            if (PhotonNetwork.isMasterClient && PhotonConnect.Instance.tournamentMode)
            { // PhotonConnect.Instance.allGroups[1]
                GameObject newGrid = PhotonNetwork.Instantiate("Grid", Vector3.zero, Quaternion.identity, 0);
                newGrid.name = "Grid2";
                newGrid.transform.SetParent(GameObject.Find("PlayScreen").transform);
                newGrid.transform.localScale = new Vector3(0, 0, 0);
                newGrid.transform.localPosition = new Vector3(0, 0, 0);
            }
        }

        protected override void Update()
        {
            base.Update();

            Player myPlayer = null;
            Player[] players = FindObjectsOfType<Player>();
            foreach (Player player in players) {
                player.UpdateLabels();
                if (player.photonView.isMine)
                    myPlayer = player;
            }

            if (players.Length == 2)
            {
                if (!_playersReady) //Do once.
                    PlayersReady();

                _timer += Time.deltaTime;

                if (_timer > _timeUntilStart) {
                    GoToScreen(GameObject.Find("PlayScreen").GetComponent<BaseMenuCanvas>());
                    enabled = false;
                }
            } if (players.Length > 2)
            {
                foreach (Player player in players)
                {
                    if (myPlayer.joinNumber == 3 || myPlayer.joinNumber == 4)
                        if (player.joinNumber == 1 || player.joinNumber == 2)
                            player.gameObject.SetActive(false);

                    if (myPlayer.joinNumber == 1 || myPlayer.joinNumber == 2)
                        if (player.joinNumber == 3 || player.joinNumber == 4)
                            player.gameObject.SetActive(false);
                }
            }
        }

        private void PlayersReady ()
        {
            _playersReady = true;
        }
    }
}
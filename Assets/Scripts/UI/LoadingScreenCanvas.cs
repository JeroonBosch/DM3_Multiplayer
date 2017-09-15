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
        }

        protected override void Update()
        {
            base.Update();

            Player[] players = FindObjectsOfType<Player>();
            foreach (Player player in players)
                player.UpdateLabels();

            if (players.Length >= 2)
            {
                if (!_playersReady) //Do once.
                    PlayersReady();

                _timer += Time.deltaTime;

                if (_timer > _timeUntilStart) {
                    GoToScreen(GameObject.Find("PlayScreen").GetComponent<BaseMenuCanvas>());
                    enabled = false;
                }
            }
        }

        private void PlayersReady ()
        {
            _playersReady = true;
        }
    }
}
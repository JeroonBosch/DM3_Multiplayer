using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Com.Hypester.DM3
{
    public class PlayerInterface : MonoBehaviour
    {
        public int playerNumber;
        //public string avatarString;

        private GameHandler _game;
        private GameObject _health;
        private Sprite[] _healthSprites;
        private GameObject _timer;
        private Sprite[] _timerSprites;
        

        private void Start()
        {
            _game = GameObject.Find("Grid").GetComponent<GameHandler>();
            _healthSprites = Resources.LoadAll<Sprite>("Spritesheets/AvatarHealth256x256");
            _timerSprites = Resources.LoadAll<Sprite>("Spritesheets/AvatarTimer256x256");
            _health = transform.Find("Health").gameObject;
            _timer = transform.Find("Timer").gameObject;
        }

        private void Update()
        {
            if (_game != null) { 

                if (_game.MyPlayer != null) { 

                    if ((_game.MyPlayer.localID == 0 && playerNumber == 0) || (_game.MyPlayer.localID == 1 && playerNumber == 1))
                        SetHitpoints(_game.healthPlayerOne, Constants.PlayerStartHP);
                    else
                        SetHitpoints(_game.healthPlayerTwo, Constants.PlayerStartHP);

                    if ((_game.IsMyTurn() && playerNumber == 0) || (!_game.IsMyTurn() && playerNumber == 1))
                        SetTimer(_game.turnTimer);
                    else
                        SetTimer(0f);
                }

                
            } else
            {
                _game = GameObject.Find("Grid").GetComponent<GameHandler>();
            }
        }

        public void SetHitpoints(float hitpoints, float maxHealth)
        {
            float ratio = hitpoints / maxHealth;
            int select = Mathf.FloorToInt(ratio * _healthSprites.Length);
            select = _healthSprites.Length - select;
            select = Mathf.Clamp(select, 0, _healthSprites.Length - 1);

            Sprite assignedSprite = _healthSprites[select];
            _health.GetComponent<Image>().sprite = assignedSprite;
        }

        public GameObject GetTimer()
        {
            return _timer;
        }

        public void SetTimer(float timer)
        {
            float ratio = timer / Constants.MoveTimeInSeconds;
            int select = Mathf.FloorToInt(ratio * _timerSprites.Length);
            select = Mathf.Clamp(select, 0, _timerSprites.Length - 1);

            Sprite assignedSprite = _timerSprites[select];
            _timer.GetComponent<Image>().sprite = assignedSprite;
        }

        public void SetTimerActive(bool active)
        {
            _timer.SetActive(active);
        }
    }
}
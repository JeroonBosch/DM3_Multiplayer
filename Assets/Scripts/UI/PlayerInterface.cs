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
        private GameObject _healthText;
        //private Sprite[] _healthSprites;
        private GameObject _timer;
        //private Sprite[] _timerSprites;

        private Text _bluePowerText;
        private Image _bluePowerImage;
        private Text _greenPowerText;
        private Image _greenPowerImage;
        private Text _redPowerText;
        private Image _redPowerImage;
        private Text _yellowPowerText;
        private Image _yellowPowerImage;

        private void Start()
        {
            _game = GameObject.Find("Grid").GetComponent<GameHandler>();
            //_healthSprites = Resources.LoadAll<Sprite>("Spritesheets/AvatarHealth256x256");
            //_timerSprites = Resources.LoadAll<Sprite>("Spritesheets/AvatarTimer256x256");
            _health = transform.Find("Health").gameObject;
            _healthText = _health.transform.Find("HealthText").gameObject;
            _timer = transform.Find("Timer").gameObject;

            _bluePowerText = GameObject.Find("MyBluePower").gameObject.GetComponent<Text>();
            _bluePowerImage = GameObject.Find("MyBlue").gameObject.GetComponent<Image>();
            _greenPowerText = GameObject.Find("MyGreenPower").gameObject.GetComponent<Text>();
            _greenPowerImage = GameObject.Find("MyGreen").gameObject.GetComponent<Image>();
            _redPowerText = GameObject.Find("MyRedPower").gameObject.GetComponent<Text>();
            _redPowerImage = GameObject.Find("MyRed").gameObject.GetComponent<Image>();
            _yellowPowerText = GameObject.Find("MyYellowPower").gameObject.GetComponent<Text>();
            _yellowPowerImage = GameObject.Find("MyYellow").gameObject.GetComponent<Image>();

            SetAvatars();
        }

        private void Update()
        {
            if (_game != null) { 

                if (_game.MyPlayer != null) {

                    //health
                    if ((_game.MyPlayer.localID == 0 && playerNumber == 0) || (_game.MyPlayer.localID == 1 && playerNumber == 1)) {
                        SetHitpoints(_game.healthPlayerOne, Constants.PlayerStartHP);
                    }
                    else { 
                        SetHitpoints(_game.healthPlayerTwo, Constants.PlayerStartHP);
                    }

                    //timer
                    if ((_game.IsMyTurn() && playerNumber == 0) || (!_game.IsMyTurn() && playerNumber == 1)) { 
                        SetTimer(_game.turnTimer);
                    }
                    else { 
                        SetTimer(Constants.TurnTime); //invisible.
                    }

                    if (_game.MyPlayer.localID == 0 && playerNumber == 0)
                    {
                        //powers
                        _bluePowerText.text = _game.P1_PowerBlue + "/" + Constants.BluePowerReq;
                        _greenPowerText.text = _game.P1_PowerGreen + "/" + Constants.GreenPowerReq;
                        _redPowerText.text = _game.P1_PowerRed + "/" + Constants.RedPowerReq;
                        _yellowPowerText.text = _game.P1_PowerYellow + "/" + Constants.YellowPowerReq;

                        if (_game.P1_PowerBlue >= Constants.BluePowerReq)
                            _bluePowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupBlueActive");
                        else
                            _bluePowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupBlue");

                        if (_game.P1_PowerGreen >= Constants.GreenPowerReq)
                            _greenPowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupGreenActive");
                        else
                            _greenPowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupGreen");

                        if (_game.P1_PowerRed >= Constants.RedPowerReq)
                            _redPowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupRedActive");
                        else
                            _redPowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupRed");

                        if (_game.P1_PowerYellow >= Constants.YellowPowerReq)
                            _yellowPowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupYellowActive");
                        else
                            _yellowPowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupYellow");
                    } else if (_game.MyPlayer.localID == 1 && playerNumber == 0)
                    {
                        //powers
                        _bluePowerText.text = _game.P2_PowerBlue + "/" + Constants.BluePowerReq;
                        _greenPowerText.text = _game.P2_PowerGreen + "/" + Constants.GreenPowerReq;
                        _redPowerText.text = _game.P2_PowerRed + "/" + Constants.RedPowerReq;
                        _yellowPowerText.text = _game.P2_PowerYellow + "/" + Constants.YellowPowerReq;

                        if (_game.P2_PowerBlue >= Constants.BluePowerReq)
                            _bluePowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupBlueActive");
                        else
                            _bluePowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupBlue");

                        if (_game.P2_PowerGreen >= Constants.GreenPowerReq)
                            _greenPowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupGreenActive");
                        else
                            _greenPowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupGreen");

                        if (_game.P2_PowerRed >= Constants.RedPowerReq)
                            _redPowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupRedActive");
                        else
                            _redPowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupRed");

                        if (_game.P2_PowerYellow >= Constants.YellowPowerReq)
                            _yellowPowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupYellowActive");
                        else
                            _yellowPowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupYellow");
                    }
                }
            } else
            {
                _game = GameObject.Find("Grid").GetComponent<GameHandler>();
            }
        }

        public void SetAvatars ()
        {
            if (!PhotonNetwork.isMasterClient)
            {
                GameObject.Find("MyAvatar").GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/AvatarB");
                GameObject.Find("OpponentAvatar").GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/AvatarA");
            }
        }

        public void SetHitpoints(float hitpoints, float maxHealth)
        {
            float ratio = hitpoints / maxHealth;
           // int select = Mathf.FloorToInt(ratio * _healthSprites.Length);
           // select = _healthSprites.Length - select;
           // select -= 5; //needed for the radial thing. Still keeping the color change.
            //select = Mathf.Clamp(select, 0, _healthSprites.Length - 1);

            //Sprite assignedSprite = _healthSprites[select];
           // _health.GetComponent<Image>().sprite = assignedSprite; //for color change
            _health.GetComponent<Image>().fillAmount = ratio;
            _healthText.GetComponent<Text>().text = hitpoints + "/" + maxHealth;
        }

        public GameObject GetTimer()
        {
            return _timer;
        }

        public void SetTimer(float timer)
        {
            float ratio = timer / Constants.TurnTime;
            ratio = 1 - ratio;
            //int select = Mathf.FloorToInt(ratio * _timerSprites.Length);
            //select = Mathf.Clamp(select, 0, _timerSprites.Length - 1);

            //Sprite assignedSprite = _timerSprites[select];
            //_timer.GetComponent<Image>().sprite = assignedSprite;
            _timer.GetComponent<Image>().fillAmount = ratio;
        }

        public void SetTimerActive(bool active)
        {
            _timer.SetActive(active);
        }
    }
}
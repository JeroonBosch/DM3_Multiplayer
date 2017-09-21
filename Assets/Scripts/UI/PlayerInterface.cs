using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Com.Hypester.DM3
{
    public class PlayerInterface : MonoBehaviour
    {
        public int playerNumber; //0 for MyAvatar and 1 for Opponent

        //public string avatarString;

        private GameObject _health;
        private GameObject _shadowHealth;
        private GameObject _timer;

        private bool _animateHealth;

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
            _health = transform.Find("Health").gameObject;
            _shadowHealth = transform.Find("ShadowHealth").gameObject;
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
            if (PhotonController.Instance.GameController != null) { 

                if (PhotonController.Instance.GameController.MyPlayer != null) {

                    if (_animateHealth) {
                        //health
                        if ((PhotonController.Instance.GameController.MyPlayer.localID == 0 && IsMyInterface()) || (PhotonController.Instance.GameController.MyPlayer.localID == 1 && !IsMyInterface())) {
                            if (GetShownHitpoints() >= PhotonController.Instance.GameController.healthPlayerOne)
                                SetHitpoints(Mathf.Max(GetShownHitpoints() - Time.deltaTime * Constants.HealthDroppingSpeed, PhotonController.Instance.GameController.healthPlayerOne));
                            else
                                _animateHealth = false;
                        }
                        else {
                            if (GetShownHitpoints() >= PhotonController.Instance.GameController.healthPlayerTwo)
                                SetHitpoints(Mathf.Max(GetShownHitpoints() - Time.deltaTime * Constants.HealthDroppingSpeed, PhotonController.Instance.GameController.healthPlayerTwo));
                            else
                               _animateHealth = false;
                        }
                    }

                    //timer
                    if ((PhotonController.Instance.GameController.IsMyTurn() && IsMyInterface()) || (!PhotonController.Instance.GameController.IsMyTurn() && !IsMyInterface())) { 
                        SetTimer(PhotonController.Instance.GameController.turnTimer);
                    }
                    else { 
                        SetTimer(Constants.TurnTime); //invisible.
                    }

                    if (PhotonController.Instance.GameController.MyPlayer.localID == 0 && IsMyInterface())
                    {
                        //powers
                        _bluePowerText.text = PhotonController.Instance.GameController.P1_PowerBlue + "/" + Constants.BluePowerReq;
                        _greenPowerText.text = PhotonController.Instance.GameController.P1_PowerGreen + "/" + Constants.GreenPowerReq;
                        _redPowerText.text = PhotonController.Instance.GameController.P1_PowerRed + "/" + Constants.RedPowerReq;
                        _yellowPowerText.text = PhotonController.Instance.GameController.P1_PowerYellow + "/" + Constants.YellowPowerReq;

                        if (PhotonController.Instance.GameController.P1_PowerBlue >= Constants.BluePowerReq)
                        {
                            _bluePowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupBlueActive");
                            _bluePowerImage.gameObject.GetComponent<Wiggle>().StartWiggle();
                        }
                        else
                        {
                            _bluePowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupBlue");
                            _bluePowerImage.gameObject.GetComponent<Wiggle>().StopWiggle();
                        }

                        if (PhotonController.Instance.GameController.P1_PowerGreen >= Constants.GreenPowerReq)
                        {
                            _greenPowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupGreenActive");
                            _greenPowerImage.gameObject.GetComponent<Wiggle>().StartWiggle();
                        }
                        else
                        {
                            _greenPowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupGreen");
                            _greenPowerImage.gameObject.GetComponent<Wiggle>().StopWiggle();
                        }

                        if (PhotonController.Instance.GameController.P1_PowerRed >= Constants.RedPowerReq)
                        {
                            _redPowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupRedActive");
                            _redPowerImage.gameObject.GetComponent<Wiggle>().StartWiggle();
                        }
                        else
                        {
                            _redPowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupRed");
                            _redPowerImage.gameObject.GetComponent<Wiggle>().StopWiggle();
                        }

                        if (PhotonController.Instance.GameController.P1_PowerYellow >= Constants.YellowPowerReq)
                        {
                            _yellowPowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupYellowActive");
                            _yellowPowerImage.gameObject.GetComponent<Wiggle>().StartWiggle();
                        }
                        else
                        {
                            _yellowPowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupYellow");
                            _yellowPowerImage.gameObject.GetComponent<Wiggle>().StopWiggle();
                        }

                    } else if (PhotonController.Instance.GameController.MyPlayer.localID == 1 && playerNumber == 0)
                    {
                        //powers
                        _bluePowerText.text = PhotonController.Instance.GameController.P2_PowerBlue + "/" + Constants.BluePowerReq;
                        _greenPowerText.text = PhotonController.Instance.GameController.P2_PowerGreen + "/" + Constants.GreenPowerReq;
                        _redPowerText.text = PhotonController.Instance.GameController.P2_PowerRed + "/" + Constants.RedPowerReq;
                        _yellowPowerText.text = PhotonController.Instance.GameController.P2_PowerYellow + "/" + Constants.YellowPowerReq;

                        if (PhotonController.Instance.GameController.P2_PowerBlue >= Constants.BluePowerReq)
                        {
                            _bluePowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupBlueActive");
                            _bluePowerImage.gameObject.GetComponent<Wiggle>().StartWiggle();
                        }
                        else
                        {
                            _bluePowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupBlue");
                            _bluePowerImage.gameObject.GetComponent<Wiggle>().StopWiggle();
                        }

                        if (PhotonController.Instance.GameController.P2_PowerGreen >= Constants.GreenPowerReq)
                        {
                            _greenPowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupGreenActive");
                            _greenPowerImage.gameObject.GetComponent<Wiggle>().StartWiggle();
                        }
                        else
                        {
                            _greenPowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupGreen");
                            _greenPowerImage.gameObject.GetComponent<Wiggle>().StopWiggle();
                        }

                        if (PhotonController.Instance.GameController.P2_PowerRed >= Constants.RedPowerReq) { 
                            _redPowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupRedActive");
                            _redPowerImage.gameObject.GetComponent<Wiggle>().StartWiggle();
                        }
                        else { 
                            _redPowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupRed");
                            _redPowerImage.gameObject.GetComponent<Wiggle>().StopWiggle();
                        }

                        if (PhotonController.Instance.GameController.P2_PowerYellow >= Constants.YellowPowerReq) { 
                            _yellowPowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupYellowActive");
                            _yellowPowerImage.gameObject.GetComponent<Wiggle>().StartWiggle();
                        }
                        else { 
                            _yellowPowerImage.sprite = Resources.Load<Sprite>("UI/ActivePower/IconPowerupYellow");
                            _yellowPowerImage.gameObject.GetComponent<Wiggle>().StopWiggle();
                        }
                    }
                }
            }
        }

        public void SetAvatars ()
        {
            //TODO Placeholders.
            if (!PhotonNetwork.isMasterClient)
            {
                GameObject.Find("MyAvatar").GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/AvatarB");
                GameObject.Find("OpponentAvatar").GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/AvatarA");
            }
        }

        public void SetHitpoints(float hitpoints)
        {
            float ratio = hitpoints / Constants.PlayerStartHP;
            _health.GetComponent<Image>().fillAmount = ratio;
        }

        public float GetShownHitpoints ()
        {
            return _health.GetComponent<Image>().fillAmount * Constants.PlayerStartHP;
        }

        public void UpdateShadowhealth (float hitpoints)
        {
            float ratio = hitpoints / Constants.PlayerStartHP;
            _shadowHealth.GetComponent<Image>().fillAmount = ratio;
        }

        public void AnimateHealth ()
        {
            if ((PhotonController.Instance.GameController.MyPlayer.localID == 0 && IsMyInterface()) || (PhotonController.Instance.GameController.MyPlayer.localID == 1 && !IsMyInterface()))
            {
                if (GetShownHitpoints() >= PhotonController.Instance.GameController.healthPlayerOne)
                    _animateHealth = true;
            } else
            {
                if (GetShownHitpoints() >= PhotonController.Instance.GameController.healthPlayerTwo)
                    _animateHealth = true;
            }
                
        }

        public GameObject GetTimer()
        {
            return _timer;
        }

        public void SetTimer(float timer)
        {
            float ratio = timer / Constants.TurnTime;
            ratio = 1 - ratio; //inverse.
            _timer.GetComponent<Image>().fillAmount = ratio;
        }

        public void SetTimerActive(bool active)
        {
            _timer.SetActive(active);
        }

        private bool IsMyInterface ()
        {
            if (playerNumber == 0)
                return true;
            return false;
        }
    }
}
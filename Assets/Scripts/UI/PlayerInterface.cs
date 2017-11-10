using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Com.Hypester.DM3
{
    public class PlayerInterface : MonoBehaviour
    {
        public enum Owner { Local, Remote }
        public Owner owner;

        //public string avatarString;

        public GameObject avatarGameObject;
        [SerializeField] GameObject _health;
        [SerializeField] GameObject _shadowHealth;
        [SerializeField] GameObject _timer;

        private bool _animateHealth;

        public List<SkillButton> skillButtons = new List<SkillButton>();
        private Dictionary<SkillColor, SkillButton> skillButtonsDict = new Dictionary<SkillColor, SkillButton>();

        private void Start()
        {
            InitSkillButtonsDict();
            SetAvatars();

            Player localPlayer = PlayerManager.instance.GetPlayerById(PhotonNetwork.player.ID);
            if (owner == Owner.Local) { localPlayer.playerInterface = this; }
            else { localPlayer.opponent.playerInterface = this; }
        }

        private void Update()
        {
            GameHandler gameHandler = PhotonController.Instance.GameController;
            if (gameHandler != null) { 

                if (gameHandler.MyPlayer != null) {

                    if (_animateHealth) {
                        //health
                        if ((gameHandler.MyPlayer.localID == 0 && IsMyInterface()) || (gameHandler.MyPlayer.localID == 1 && !IsMyInterface())) {
                            if (GetShownHitpoints() >= gameHandler.healthPlayerOne)
                                SetHitpoints(Mathf.Max(GetShownHitpoints() - Time.deltaTime * Constants.HealthDroppingSpeed, gameHandler.healthPlayerOne));
                            else
                                _animateHealth = false;
                        }
                        else {
                            if (GetShownHitpoints() >= gameHandler.healthPlayerTwo)
                                SetHitpoints(Mathf.Max(GetShownHitpoints() - Time.deltaTime * Constants.HealthDroppingSpeed, gameHandler.healthPlayerTwo));
                            else
                               _animateHealth = false;
                        }
                    }

                    //timer
                    if ((gameHandler.IsMyTurn() && IsMyInterface()) || (!gameHandler.IsMyTurn() && !IsMyInterface())) { 
                        SetTimer(gameHandler.turnTimer);
                    }
                    else { 
                        SetTimer(Constants.TurnTime); //invisible.
                    }

                    // Powers. TODO: Optimize this. Should not be done every Update()
                    if (IsMyInterface())
                    {
                        foreach (SkillButton skillButton in skillButtons)
                        {
                            int powerChargeAmount = gameHandler.GetSkillChargeAmount(skillButton.skillColor, gameHandler.MyPlayer.localID);
                            int powerActivationRequirementAmount = Constants.GetSkillActivationRequirement(skillButton.skillColor);

                            skillButton.SetSkillChargeText((Mathf.Min(powerChargeAmount, powerActivationRequirementAmount)).ToString() + "/" + powerActivationRequirementAmount);

                            bool fullyCharged = powerChargeAmount >= powerActivationRequirementAmount;
                            skillButton.ToggleState(fullyCharged);
                            skillButton.wiggle.ToggleWiggle(fullyCharged);
                        }
                    }
                }
            }
        }

        private void InitSkillButtonsDict()
        {
            foreach (SkillButton skillButton in skillButtons)
            {
                skillButtonsDict.Add(skillButton.skillColor, skillButton);
            }
        }
        public SkillButton GetSkillButtonBySkillColor(SkillColor skillColor)
        {
            return skillButtonsDict[skillColor];
        }

        public void SetAvatars ()
        {
            //TODO Placeholders.
            if (!PhotonNetwork.isMasterClient)
            {
                GameObject.Find("MyAvatar").GetComponent<Image>().sprite = MainController.Data.sprites.guestAvatar;
                GameObject.Find("OpponentAvatar").GetComponent<Image>().sprite = MainController.Data.sprites.guestAvatar; ;
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
            if (owner == Owner.Local)
                return true;
            return false;
        }
    }
}
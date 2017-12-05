using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class SkillButton : MonoBehaviour {

        public Animator animator;
        public Transform textPopupTransform;
        public SkillColor skillColor;
        public Wiggle wiggle;

        [SerializeField] Button button;
        [SerializeField] Image buttonBackground;
        [SerializeField] Image buttonSkillIcon;
        [SerializeField] Text skillChargeText;

        [SerializeField] Sprite chargingSprite;
        [SerializeField] Sprite chargingSpritePressed;
        [SerializeField] Sprite readySprite;
        [SerializeField] Sprite readySpritePressed;

        public void ActivateSkill()
        {
            PhotonController.Instance.GameController.photonView.RPC("RPC_PowerClicked", PhotonTargets.All, skillColor);
        }

        public void SetSkillIcon(Sprite value)
        {
            if (value != null) { buttonSkillIcon.sprite = value; }
        }
        public void SetSkillChargeText(string value)
        {
            skillChargeText.text = value;
        }
        public void ToggleState(bool fullyCharged)
        {
            SpriteState spriteState = new SpriteState();
            spriteState = button.spriteState;
            buttonBackground.sprite = fullyCharged ? readySprite : chargingSprite;
            spriteState.pressedSprite = fullyCharged ? readySpritePressed : chargingSpritePressed;
            button.spriteState = spriteState;
        }
    }
}
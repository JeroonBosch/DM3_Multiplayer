using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class PreferencesCanvas : BaseMenuCanvas
    {
        [SerializeField] GameObject hapticFeedbackActiveButton;
        [SerializeField] GameObject hapticFeedbackDeactiveButton;

        public override void Show()
        {
            base.Show();

            // TODO: If logged in with facebook, active SignOut button. Also ,get current sound and haptic values.
        }

        public void OnSoundFXValueChanged(Slider slider)
        {
            // TODO: Change sound volume.
        }

        public void ToggleHapticFeedback(bool yes)
        {
            if (yes)
            {
                // TODO: Activate haptic feedback.
                hapticFeedbackActiveButton.SetActive(true);
                hapticFeedbackDeactiveButton.SetActive(false);
            }
            else
            {
                // TODO: Disable haptic feedback.
                hapticFeedbackActiveButton.SetActive(false);
                hapticFeedbackDeactiveButton.SetActive(true);
            }
        }

        public void SignOutFromFacebook()
        {
            // TODO: Sign out from facebook
        }

        public void PreviousScreen()
        {
            GoToScreen(PrevScreen());
        }
    }
}
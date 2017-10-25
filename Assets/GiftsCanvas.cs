using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class GiftsCanvas : BaseMenuCanvas
    {
        [SerializeField] Text inboxCountText;
        [SerializeField] Image inboxButton;
        [SerializeField] Sprite greenButtonSprite;
        [SerializeField] Sprite greenButtonPressedSprite;

        protected override void OnEnable()
        {
            base.OnEnable();
            int msgCount = 2; // TODO: get this info from server. Probably hook to an event.
            if (msgCount > 0)
            {
                Button button = inboxButton.GetComponent<Button>();
                inboxButton.sprite = greenButtonSprite;
                SpriteState spriteState = new SpriteState();
                spriteState = button.spriteState;
                spriteState.pressedSprite = greenButtonPressedSprite;
                button.spriteState = spriteState;
                inboxCountText.text = msgCount.ToString();
            }
        }

        public void PreviousScreen()
        {
            GoToScreen(PrevScreen());
        }

        public void SendCoins()
        {
            GoToScreen(GameObject.Find("SendCoinsScreen").GetComponent<SendCoinsCanvas>());
        }

        public void AskCoins()
        {
            GoToScreen(GameObject.Find("AskCoinsScreen").GetComponent<AskCoinsCanvas>());
        }

        public void Inbox()
        {
            GoToScreen(GameObject.Find("InboxScreen").GetComponent<InboxCanvas>());
        }
    }
}
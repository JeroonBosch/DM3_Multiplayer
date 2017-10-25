using UnityEngine;

namespace Com.Hypester.DM3
{
    public class FreeCoinsCanvas : BaseMenuCanvas
    {
        public void AskCoins()
        {
            GoToScreen(GameObject.Find("AskCoinsScreen").GetComponent<AskCoinsCanvas>());
        }

        public void InviteFriends()
        {
            GoToScreen(GameObject.Find("InviteFriendsScreen").GetComponent<InviteFriendsCanvas>());
        }

        public void DailyWheel()
        {
            // GoToScreen(GameObject.Find("DailyWheelScreen").GetComponent<InboxCanvas>());
        }

        public void PreviousScreen()
        {
            GoToScreen(PrevScreen());
        }
    }
}
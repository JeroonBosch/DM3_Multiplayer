using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class InboxCanvas : BaseMenuCanvas, IEntryList
    {
        [SerializeField] Text emptyInboxAlert;
        [SerializeField] Text emptyInboxAlertAdditional;
        [SerializeField] Button askCoinsButton;

        [SerializeField] GameObject messageList;
        [SerializeField] Transform contentTransform;
        [SerializeField] GameObject messageEntryPrefab;
        List<InboxEntry> allInboxEntries = new List<InboxEntry>();

        public void AskCoins()
        {
            GoToScreen(GameObject.Find("AskCoinsScreen").GetComponent<AskCoinsCanvas>());
        }

        void ClearInboxEntries()
        {
            int listCount = allInboxEntries.Count;
            for (int i = 0; i < listCount; i++)
            {
                Destroy(allInboxEntries[i].gameObject);
            }
            allInboxEntries.Clear();
        }

        public override void Hide()
        {
            base.Hide();

            ClearInboxEntries();
            emptyInboxAlert.gameObject.SetActive(true);
            emptyInboxAlertAdditional.gameObject.SetActive(true);
            askCoinsButton.gameObject.SetActive(true);
            messageList.SetActive(false);
        }

        public override void Show()
        {
            base.Show();

            int messages = 2; // TODO: retrieve this value from database

            if (messages > 0)
            {
                messageList.SetActive(true);
                for (int i = 0; i < messages; i++)
                {
                    InboxEntry entry = Instantiate(messageEntryPrefab, contentTransform, false).GetComponent<InboxEntry>();
                    entry.SetIFLObject(this);
                    entry.SetName("Friend (" + UnityEngine.Random.Range(0, 9999).ToString() + ")");
                    int coinAmount = UnityEngine.Random.Range(10, 100); // TODO: retrieve this info from server
                    entry.SetInfoText1("Sent you");
                    entry.SetInfoText2(coinAmount.ToString());
                    entry.coins = coinAmount;
                    allInboxEntries.Add(entry);
                }
                if (allInboxEntries.Count > 0)
                {
                    emptyInboxAlert.gameObject.SetActive(false);
                    emptyInboxAlertAdditional.gameObject.SetActive(false);
                    askCoinsButton.gameObject.SetActive(false);
                }
            }
        }

        public void PreviousScreen()
        {
            GoToScreen(PrevScreen());
        }

        public void Selected(UIEntry entry, bool yes)
        {
            throw new NotImplementedException();
        }

        public void EntryPrimaryAction(UIEntry entry)
        {
            MainController.Instance.playerData.AddCoins(((InboxEntry)entry).coins); // TODO: this must be done server-side
            allInboxEntries.Remove((InboxEntry) entry);
            Destroy(entry.gameObject);

            if (allInboxEntries.Count <= 0)
            {
                emptyInboxAlert.gameObject.SetActive(true);
                emptyInboxAlertAdditional.gameObject.SetActive(true);
                askCoinsButton.gameObject.SetActive(true);
                messageList.SetActive(false);
            }
        }
    }
}
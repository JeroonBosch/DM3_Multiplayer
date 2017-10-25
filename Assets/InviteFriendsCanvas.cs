using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class InviteFriendsCanvas : BaseMenuCanvas, IEntryList
    {
        [SerializeField] Button inviteFriendsButton;

        [SerializeField] GameObject selectAllDeactiveButton;
        [SerializeField] GameObject selectAllActiveButton;

        [SerializeField] Transform contentTransform;
        [SerializeField] GameObject friendEntryPrefab;
        List<FriendEntry> allFriendEntries = new List<FriendEntry>();
        List<FriendEntry> selectedFriendEntries = new List<FriendEntry>();
        List<FriendEntry> nonSelectedFriendEntries = new List<FriendEntry>();

        public void InviteFriends()
        {
            if (selectedFriendEntries.Count <= 0) { return; }
            foreach (FriendEntry entry in selectedFriendEntries)
            {
                // TODO: Send coins here
            }
        }

        public void SelectAll(bool yes)
        {
            selectAllDeactiveButton.SetActive(!yes);
            selectAllActiveButton.SetActive(yes);

            foreach (FriendEntry entry in allFriendEntries)
            {
                entry.Selected(yes);
            }
        }

        public void Selected(UIEntry entry, bool yes)
        {
            if (yes)
            {
                if (nonSelectedFriendEntries.Contains((FriendEntry)entry)) { nonSelectedFriendEntries.Remove((FriendEntry)entry); }
                if (!selectedFriendEntries.Contains((FriendEntry)entry)) { selectedFriendEntries.Add((FriendEntry)entry); }
            }
            else
            {
                if (selectedFriendEntries.Contains((FriendEntry)entry)) { selectedFriendEntries.Remove((FriendEntry)entry); }
                if (!nonSelectedFriendEntries.Contains((FriendEntry)entry)) { nonSelectedFriendEntries.Add((FriendEntry)entry); }
            }
            
            inviteFriendsButton.interactable = selectedFriendEntries.Count > 0 ? true : false;

            if (selectedFriendEntries.Count >= allFriendEntries.Count)
            {
                selectAllDeactiveButton.SetActive(false);
                selectAllActiveButton.SetActive(true);
            }
            else
            {
                selectAllDeactiveButton.SetActive(true);
                selectAllActiveButton.SetActive(false);
            }
        }

        public void EntryPrimaryAction(UIEntry entry) { throw new NotImplementedException(); }

        void ClearFriendList()
        {
            int listCount = allFriendEntries.Count;
            for (int i = 0; i < listCount; i++)
            {
                Destroy(allFriendEntries[i].gameObject);
            }
            allFriendEntries.Clear();
            selectedFriendEntries.Clear();
            nonSelectedFriendEntries.Clear();
        }

        public override void Hide()
        {
            base.Hide();

            ClearFriendList();
            inviteFriendsButton.interactable = false;
            selectAllDeactiveButton.SetActive(true);
            selectAllActiveButton.SetActive(false);
        }

        public override void Show()
        {
            base.Show();

            for (int i = 0; i < 15; i++)
            {
                FriendEntry entry = Instantiate(friendEntryPrefab, contentTransform, false).GetComponent<FriendEntry>();
                entry.SetIFLObject(this);
                entry.SetName("Friend (" + UnityEngine.Random.Range(0, 9999).ToString() + ")");
                allFriendEntries.Add(entry);
            }
        }

        public void PreviousScreen()
        {
            GoToScreen(PrevScreen());
        }
    }
}
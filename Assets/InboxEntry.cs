using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class InboxEntry : UIEntry
    {
        public int coins;

        [SerializeField] Image avatarIcon;
        [SerializeField] Image avatarBorder;
        [SerializeField] Text senderName;
        [SerializeField] Text infoText1;
        [SerializeField] Text infoText2;
        [SerializeField] Image messageType;


        public void MessageAction()
        {
            ifl.EntryPrimaryAction(this);
        }

        public void SetIFLObject(IEntryList ifl)
        {
            this.ifl = ifl;
        }
        public void SetAvatarImage(Sprite newImage)
        {
            avatarIcon.sprite = newImage;
        }
        public void SetAvatarBorder(Sprite newImage)
        {
            avatarBorder.sprite = newImage;
        }
        public void SetName(string newName)
        {
            senderName.text = newName;
        }
        public void SetInfoText1(string newText)
        {
            infoText1.text = newText;
        }
        public void SetInfoText2(string newText)
        {
            infoText2.text = newText;
        }
    }
}

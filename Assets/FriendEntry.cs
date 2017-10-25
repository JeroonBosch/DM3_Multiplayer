using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class FriendEntry : UIEntry {


        [SerializeField] Image avatarIcon;
        [SerializeField] Image avatarBorder;
        [SerializeField] Text friendNameText;
        [SerializeField] GameObject selectDeactive;
        [SerializeField] GameObject selectActive;

        public void Selected(bool yes)
        {
            selectDeactive.SetActive(!yes);
            selectActive.SetActive(yes);
            ifl.Selected(this, yes);
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
            friendNameText.text = newName;
        }
    }
}

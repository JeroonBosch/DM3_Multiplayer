using System.Collections.Generic;
using UnityEngine;

namespace Com.Hypester.DM3
{
    public class SpriteDatabase : MonoBehaviour
    {
        [Header("Avatars")]
        public Sprite guestAvatar;
        public Sprite randomAvatarSheet;

        [Header("AvatarBorders")]
        [SerializeField] List<AvatarBorderEntry> avatarBorders;
        public Sprite defaultNormalAvatarBorder;
        public Sprite defaultWinnerAvatarBorder;

        [Header("Icons")]
        public Sprite thumbsUp;
        public Sprite thumbsDown;
        public Sprite lockIcon;
        public Sprite skillIcon;
        public Sprite coinIcon;
        [SerializeField] List<Sprite> coinPiles = new List<Sprite>();

        [Header("Skills")]
        [SerializeField]
        List<Sprite> blueSprites = new List<Sprite>();
        [SerializeField] List<Sprite> greenSprites = new List<Sprite>();
        [SerializeField] List<Sprite> redSprites = new List<Sprite>();
        [SerializeField] List<Sprite> yellowSprites = new List<Sprite>();

        [Header("Stages")]
        [SerializeField] List<StageArtEntry> stages;

        [Header("Popups")]
        [SerializeField] List<PopupArtEntry> popups;

        public Sprite GetSkillSprite(string syscode, int level)
        {
            Sprite skillSprite = null;
            switch (syscode)
            {
                case "blue":
                    skillSprite = blueSprites[level];
                    break;
                case "green":
                    skillSprite = greenSprites[level];
                    break;
                case "red":
                    skillSprite = redSprites[level];
                    break;
                case "yellow":
                    skillSprite = yellowSprites[level];
                    break;
            }
            return skillSprite;
        }

        public Sprite GetCoinPileSprite(int level)
        {
            return (coinPiles.Count == 0 ? null : coinPiles[level]);
        }

        public AvatarBorderEntry GetAvatarBorderEntry(string syscode)
        {
            AvatarBorderEntry avatarBorderEntry = new AvatarBorderEntry();
            foreach (AvatarBorderEntry abe in avatarBorders)
            {
                if (string.Equals(abe.syscode, syscode)) { avatarBorderEntry = abe; break; }
            }
            return avatarBorderEntry;
        }

        public StageArt GetStageArt(string syscode)
        {
            StageArt stageArt = new StageArt();
            foreach (StageArtEntry sae in stages)
            {
                if (string.Equals(sae.syscode, syscode)) { stageArt = sae.stageArt; break; }
            }
            return stageArt;
        }

        public Sprite GetPopupBackground(PopupType type)
        {
            Sprite popupBg = null;
            foreach (PopupArtEntry pae in popups)
            {
                if (pae.type == type) { popupBg = pae.bg; break; }
            }
            return popupBg;
        }
    }

    [System.Serializable]
    public struct StageArtEntry { public string syscode; public StageArt stageArt; }
    [System.Serializable]
    public struct PopupArtEntry { public PopupType type; public Sprite bg; }
    [System.Serializable]
    public struct AvatarBorderEntry { public string syscode; public Sprite normal; public Sprite winner; }

}
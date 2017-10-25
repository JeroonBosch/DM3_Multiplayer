using System.Collections.Generic;
using UnityEngine;

public class SpriteDatabase : MonoBehaviour {

    [Header("Avatars")]
    public Sprite guestAvatar;

    [Header("Icons")]
    public Sprite thumbsUp;
    public Sprite thumbsDown;
    public Sprite lockIcon;
    public Sprite coinIcon;
    public Sprite coinIcon7;
    public Sprite skillIcon;

    [Header("Skills")]
    [SerializeField] List<Sprite> blueSprites = new List<Sprite>();
    [SerializeField] List<Sprite> greenSprites = new List<Sprite>();
    [SerializeField] List<Sprite> redSprites = new List<Sprite>();
    [SerializeField] List<Sprite> yellowSprites = new List<Sprite>();

    public Sprite GetSkillSprite(string skillColor, int level)
    {
        Sprite skillSprite = null;
        switch (skillColor)
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
}

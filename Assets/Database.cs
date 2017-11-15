using UnityEngine;

namespace Com.Hypester.DM3
{
    public class Database : MonoBehaviour
    {
        public SoundDatabase sounds;
        public SpriteDatabase sprites;
        public TemporaryDatabase temporary;

        public void Init()
        {
            sounds = GetComponent<SoundDatabase>();
            sprites = GetComponent<SpriteDatabase>();
            temporary = GetComponent<TemporaryDatabase>();

            sprites.Init();
        }
    }
}
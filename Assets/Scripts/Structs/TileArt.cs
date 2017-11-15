using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Com.Hypester.DM3
{
    [System.Serializable]
    public class TileArt
    {
        [SerializeField] Sprite redSprite;
        [SerializeField] Sprite greenSprite;
        [SerializeField] Sprite blueSprite;
        [SerializeField] Sprite yellowSprite;

        Dictionary<TileTypes.EColor, Sprite> colorCodedSprites = new Dictionary<TileTypes.EColor, Sprite>();

        public void Init()
        {
            colorCodedSprites.Add(TileTypes.EColor.red, redSprite);
            colorCodedSprites.Add(TileTypes.EColor.green, greenSprite);
            colorCodedSprites.Add(TileTypes.EColor.blue, blueSprite);
            colorCodedSprites.Add(TileTypes.EColor.yellow, yellowSprite);
        }

        public Sprite GetSprite(TileTypes.EColor color) {
            return (colorCodedSprites.ContainsKey(color)) ? colorCodedSprites[color] : null;
        }
    }
}
using UnityEngine;
using System.Collections;

namespace Com.Hypester.DM3
{
    public class TileTypes
    {
        public enum EColor { yellow, blue, green, red };
        protected EColor m_color;

        public enum EBooster { level0, level1, level2, level3 };
        protected EBooster m_booster;

        public EColor Color
        {
            get
            {
                return m_color;
            }
            set
            {
                m_color = value;
            }
        }

        public EBooster BoosterLevel
        {
            get
            {
                return m_booster;
            }
            set
            {
                m_booster = value;
            }
        }

        //Hex
        public Sprite HexSprite
        {
            get
            {
                if (m_color == EColor.yellow)
                    return Resources.LoadAll<Sprite>("Tiles/TilesHexa128x128")[0];
                else if (m_color == EColor.blue)
                    return Resources.LoadAll<Sprite>("Tiles/TilesHexa128x128")[1];
                else if (m_color == EColor.green)
                    return Resources.LoadAll<Sprite>("Tiles/TilesHexa128x128")[2];
                else if (m_color == EColor.red)
                    return Resources.LoadAll<Sprite>("Tiles/TilesHexa128x128")[3];
                return Resources.LoadAll<Sprite>("Tiles/TilesHexa128x128")[0];
            }
        }
        public Sprite HexSpriteSelected
        {
            get
            {
                if (m_color == EColor.yellow)
                    return Resources.LoadAll<Sprite>("Tiles/TilesHexaSelected128x128")[0];
                else if (m_color == EColor.blue)
                    return Resources.LoadAll<Sprite>("Tiles/TilesHexaSelected128x128")[1];
                else if (m_color == EColor.green)
                    return Resources.LoadAll<Sprite>("Tiles/TilesHexaSelected128x128")[2];
                else if (m_color == EColor.red)
                    return Resources.LoadAll<Sprite>("Tiles/TilesHexaSelected128x128")[3];
                return Resources.LoadAll<Sprite>("Tiles/TilesHexaSelected128x128")[0];
            }
        }
    }
}
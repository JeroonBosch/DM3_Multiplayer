using UnityEngine;
using System.Collections;

namespace Com.Hypester.DM3
{
    public class TileTypes
    {
        public enum EColor { yellow, blue, green, red };
        protected EColor m_color;

        public enum EBooster { level0, level1, level2, level3, trap };
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
    }
}
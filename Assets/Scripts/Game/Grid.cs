using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Com.Hypester.DM3
{
    [Serializable]
    public class Grid
    {
        public Tile[,] data;
    }

    [Serializable]
    public class Tile
    {
        public int color;
        public int boosterLevel;
        public int x;
        public int y;
    }

    [Serializable]
    public class AnimateTile
    {
        public int color;
        public float fallDistance;
        public int x;
        public int y;

        public AnimateTile(int color, float fallDistance, int x, int y)
        {
            this.color = color;
            this.fallDistance = fallDistance;
            this.x = x;
            this.y = y;
        }
    }
}
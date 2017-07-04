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
}
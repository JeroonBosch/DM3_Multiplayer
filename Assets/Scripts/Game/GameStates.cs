﻿using UnityEngine;
using System.Collections;

namespace Com.Hypester.DM3
{
    public class GameStates
    {
        public enum EGameState { search, matchFound, inTurn, interim, gameEnd };
        protected EGameState m_state;

        public EGameState State
        {
            get
            {
                return m_state;
            }
            set
            {
                Debug.Log("Now in state: " + value.ToString());
                m_state = value;
            }
        }
    }
}
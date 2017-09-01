﻿using UnityEngine;
using System.Collections;

namespace Com.Hypester.DM3
{
    public class MainmenuCanvas : BaseMenuCanvas
    {
        public void PlayNormalGame()
        {
            GoToScreen(GameObject.Find("NormalGameScreen").GetComponent<BaseMenuCanvas>());
            PhotonConnect.Instance.ConnectNormalLobby();
        }
    }
}
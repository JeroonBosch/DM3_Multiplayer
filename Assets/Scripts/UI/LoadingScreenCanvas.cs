using UnityEngine;
using UnityEngine.UI;
using Lean.Touch;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Com.Hypester.DM3
{
    public class LoadingScreenCanvas : BaseMenuCanvas
    {
        protected override void Start ()
        {
            base.Start();

        }

        protected override void Update()
        {
            base.Update();

            Player[] players = FindObjectsOfType<Player>();
            if (players.Length == 2)
            {
                GoToScreen(GameObject.Find("PlayScreen").GetComponent<BaseMenuCanvas>());
                enabled = false;
            }
        }
    }
}
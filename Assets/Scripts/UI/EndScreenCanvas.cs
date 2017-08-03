using UnityEngine;
using UnityEngine.UI;
using Lean.Touch;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Com.Hypester.DM3
{
    public class EndScreenCanvas : BaseMenuCanvas
    {
        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            base.Update();
        }

        public void BackToMenu ()
        {
            if (PhotonNetwork.inRoom)
            {
                PhotonNetwork.LeaveRoom();
                PhotonNetwork.LoadLevel("Menu");
            }
        }
    }
}
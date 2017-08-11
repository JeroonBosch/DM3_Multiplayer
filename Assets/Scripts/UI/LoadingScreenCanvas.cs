using UnityEngine;
using UnityEngine.UI;
using Lean.Touch;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Com.Hypester.DM3
{
    public class LoadingScreenCanvas : BaseMenuCanvas
    {
        float timer = 0f;
        float timeUntilStart = 3f;

        protected override void Start ()
        {
            base.Start();
            //GameObject.FindGameObjectsWithTag("Player_1_Name");
            //GameObject.Find("Player1Name").GetComponent<Text>().text = PhotonNetwork.playerName;
        }

        protected override void Update()
        {
            base.Update();

            timer += Time.deltaTime;

            Player[] players = FindObjectsOfType<Player>();
            if (players.Length == 2 && timer > timeUntilStart)
            {
                GoToScreen(GameObject.Find("PlayScreen").GetComponent<BaseMenuCanvas>());
                enabled = false;
            }
        }
    }
}
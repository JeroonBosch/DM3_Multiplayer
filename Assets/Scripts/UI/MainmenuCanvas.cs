using UnityEngine;
using System.Collections;

namespace Com.Hypester.DM3
{
    public class MainmenuCanvas : BaseMenuCanvas
    {

        // Use this for initialization
        protected override void Start()
        {
            base.Start();
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
        }

        public void PlayNormalGame()
        {
            GoToScreen(GameObject.Find("NormalGameScreen").GetComponent<BaseMenuCanvas>());
        }
    }
}
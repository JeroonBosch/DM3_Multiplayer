using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Hypester.DM3
{
    public class LoginCanvas : BaseMenuCanvas
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

        public void Login()
        {
            GoToScreen(NextScreen());
        }
    }
}
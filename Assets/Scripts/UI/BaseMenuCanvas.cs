using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Com.Hypester.DM3
{
    public class BaseMenuCanvas : MonoBehaviour
    {
        protected bool isShown;

        // Use this for initialization
        protected virtual void Start()
        {
            isShown = false;
        }

        // Update is called once per frame
        protected virtual void Update()
        {

        }

        protected virtual BaseMenuCanvas NextScreen()
        {
            BaseMenuCanvas nextScreen = null;

            List<BaseMenuCanvas> menus = new List<BaseMenuCanvas>();
            menus = GetAllScreens();
            int index = menus.IndexOf(this);
            if (menus.Count > (index + 1))
                nextScreen = menus[index + 1];

            return nextScreen;
        }

        protected virtual BaseMenuCanvas PrevScreen()
        {
            BaseMenuCanvas prevScreen = null;

            List<BaseMenuCanvas> menus = new List<BaseMenuCanvas>();
            menus = GetAllScreens();
            int index = menus.IndexOf(this);
            if (menus.Count > (index - 1))
                prevScreen = menus[index - 1];

            return prevScreen;
        }

        protected virtual List<BaseMenuCanvas> GetAllScreens()
        {
            return FindObjectsOfType<BaseMenuCanvas>().ToList();
        }

        public virtual void GoToScreen(BaseMenuCanvas screen)
        {
            foreach (BaseMenuCanvas menu in GetAllScreens())
                menu.Hide();
            screen.Show();
        }

        public virtual void Hide()
        {
            GetComponent<Canvas>().enabled = false;
            isShown = false;
        }

        public virtual void Show()
        {
            GetComponent<Canvas>().enabled = true;
            isShown = true;
        }
    }
}
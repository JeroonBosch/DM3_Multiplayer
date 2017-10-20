using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Com.Hypester.DM3
{
    public class BaseMenuCanvas : MonoBehaviour
    {
        [SerializeField] protected BaseMenuCanvas previousScreen;
        [SerializeField] protected BaseMenuCanvas nextScreen;
        [SerializeField] protected Header header;

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

        protected virtual void OnEnable()
        {

        }

        protected virtual void OnDisable()
        {

        }

        protected virtual BaseMenuCanvas NextScreen()
        {
            return nextScreen;
        }

        protected virtual BaseMenuCanvas PrevScreen()
        {
            return previousScreen;
        }

        protected virtual List<BaseMenuCanvas> GetAllScreens()
        {
            return FindObjectsOfType<BaseMenuCanvas>().ToList();
        }

        public virtual void GoToScreen(BaseMenuCanvas screen)
        {
            if (!screen.isShown) { 
                foreach (BaseMenuCanvas menu in GetAllScreens())
                    menu.Hide();
                screen.Show();

                MainController.Instance.currentScreen = screen;
            }
        }

        public virtual void Hide()
        {
            if (header != null) { header.headerGraphics.SetActive(false); }
            GetComponent<Canvas>().enabled = false;
            isShown = false;
        }

        public virtual void Show()
        {
            if (header != null) { header.headerGraphics.SetActive(true); }
            GetComponent<Canvas>().enabled = true;
            isShown = true;
        }
    }
}
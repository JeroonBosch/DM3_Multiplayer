using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Com.Hypester.DM3
{
    public class MainController : MonoBehaviour
    {
        public AudioSource sound;
        public BaseMenuCanvas currentScreen;

        private static MainController instance;
        public static MainController Instance
        {
            get { return instance ?? (instance = new GameObject("MainController").AddComponent<MainController>()); }
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            BaseMenuCanvas firstScreen = GameObject.Find("LoginScreen").GetComponent<BaseMenuCanvas>();
            firstScreen.GoToScreen(firstScreen);
            currentScreen = firstScreen;
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}
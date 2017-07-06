using Photon;
using UnityEngine;

namespace Com.Hypester.DM3
{
    public class Player : Photon.MonoBehaviour
    {
        int profileID;
        int coins;
        string profileName;
        string portraitURL;

        // Use this for initialization
        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
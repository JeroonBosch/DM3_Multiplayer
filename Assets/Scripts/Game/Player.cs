using Photon;
using UnityEngine;
using UnityEngine.UI;

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

            if (gameObject.GetComponent<PhotonView>().isMine)
                transform.Find("FingerTracker").GetComponent<Image>().enabled = false;
        }

        void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                stream.SendNext(profileName);
            }
            else
            {
                profileName = (string)stream.ReceiveNext();
            }
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}
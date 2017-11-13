using System;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class SurrenderPopup : MonoBehaviour, IPopup
    {
        public void Accept()
        {
            if (PhotonNetwork.inRoom)
            {
                PhotonNetwork.LeaveRoom();
                PhotonNetwork.LoadLevel("Menu");
            }
            else
            {
                PhotonNetwork.LoadLevel("Menu");
            }
        }

        public void Refuse()
        {
            UIEvent.PopupClose(this);
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }
    }
}
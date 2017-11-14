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
                Debug.Log("PhotonNetwork.inRoom");
                PhotonNetwork.LeaveRoom();
                PhotonNetwork.LoadLevel("Menu");
            }
            else
            {
                Debug.Log("!PhotonNetwork.inRoom");
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
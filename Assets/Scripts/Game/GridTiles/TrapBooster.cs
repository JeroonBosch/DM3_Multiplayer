using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Com.Hypester.DM3
{
    public class TrapBooster : MonoBehaviour
    {
        private int _ownerPlayer;
        public int ownerPlayer { set { SetOwner(value); } } //Hide if not owned.

        private void SetOwner(int owner)
        {
            _ownerPlayer = owner;
            if (PhotonConnect.Instance.GameController.MyPlayer.localID != _ownerPlayer)
            {
                GetComponent<Animator>().enabled = false;
                GetComponent<Image>().enabled = false;
            }
        }
    }
}

using UnityEngine;

namespace Com.Hypester.DM3
{
    public class BaseTile : MonoBehaviour
    {
        public Vector2 position { get; set; }
        /*
        bool isSelected;
        if (stream.isWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(isSelected);
        }else{
            // Network player, receive data
            this.isSelected = (bool)stream.ReceiveNext();
        }
        */
    }
}
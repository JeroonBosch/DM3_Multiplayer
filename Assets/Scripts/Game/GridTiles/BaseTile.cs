using UnityEngine;

namespace Com.Hypester.DM3
{
    public class BaseTile : MonoBehaviour
    {
        /*
         * bool isSelected;
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                Vector3 pos = transform.localPosition;
                stream.Serialize(ref pos);
            }
            else
            {
                Vector3 pos = Vector3.zero;
                stream.Serialize(ref pos);  // pos gets filled-in. must be used somewhere
            }
        }*/
    }
}
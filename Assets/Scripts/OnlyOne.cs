using UnityEngine;

namespace Com.Hypester.DM3
{
    public class OnlyOne : MonoBehaviour
    {
        //Class to give when there's only supposed to be one instance. Requires an unique tag.
        void OnEnable()
        {
            string tag = gameObject.tag;
            GameObject[] checkArray = GameObject.FindGameObjectsWithTag(tag);

            if (checkArray.Length > 1)
            {
                int count = 0;
                foreach (GameObject one in checkArray)
                {
                    if (count > 0)
                        Destroy(one.gameObject);
                    count++;
                }
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Hypester.DM3
{
    //Simple "don't destroy" class. For things that require to stay between scene-switching. Best to be used along with OnlyOne.
    public class DontDestroy : MonoBehaviour
    {
        void OnEnable()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
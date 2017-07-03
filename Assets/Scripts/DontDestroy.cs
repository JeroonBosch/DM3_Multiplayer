using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Hypester.DM3
{
    public class DontDestroy : MonoBehaviour
    {
        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
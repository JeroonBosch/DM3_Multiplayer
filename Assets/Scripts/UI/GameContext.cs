using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Com.Hypester.DM3
{
    public class GameContext : MonoBehaviour
    {
        private Text _text;

        private float _showTime;
        private float _timer;

        private BufferedText[] bufferedText;

        // Use this for initialization
        void Start()
        {
            _text = GetComponent<Text>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            _timer += Time.fixedDeltaTime;
        }
    }

    public class BufferedText
    {
        public string text;
        public float showTime;
    }
}
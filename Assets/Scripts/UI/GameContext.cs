using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Com.Hypester.DM3
{
    public class GameContext : MonoBehaviour
    {
        private Text _text;

        private float _showTime;
        private float _timer;

        private List<BufferedText> _bufferedText;

        void Start()
        {
            _text = GetComponent<Text>();

            _bufferedText = new List<BufferedText>();
        }

        void FixedUpdate()
        {
            _timer += Time.fixedDeltaTime;

            if (_bufferedText.Count > 1)
            {
                if (_timer - _showTime > _bufferedText[0].showTime)
                {
                    _showTime = _timer;
                    _text.text = _bufferedText[0].text;
                    _bufferedText.RemoveAt(0);
                }
            } else
            {
                if (_timer - _showTime > Constants.MinimumTextTime)
                {
                    _text.text = "";
                }
            }
        }

        public void ShowText(string text)
        {
            if (_text.text == "") {
                _text.text = text;
                _showTime = _timer;
            }
            else {
                BufferedText newBuffer = new BufferedText(text, Constants.MinimumTextTime);
                _bufferedText.Add(newBuffer);
            }
        }

        public void ShowLargeText(string text)
        {
            GameObject go = Instantiate(Resources.Load("UI/LargeText")) as GameObject;
            go.transform.SetParent(transform.parent, false);
            go.GetComponent<Text>().text = text;
        }

        public void ShowMyText(string text)
        {
            GameObject go = Instantiate(Resources.Load("UI/MyPlayerText")) as GameObject;
            go.transform.SetParent(transform.parent, false);
            go.GetComponent<Text>().text = text;
        }

        public void ShowEnemyText(string text)
        {
            GameObject go = Instantiate(Resources.Load("UI/EnemyPlayerText")) as GameObject;
            go.transform.SetParent(transform.parent, false);
            go.GetComponent<Text>().text = text;
        }
    }

    public class BufferedText
    {
        public string text;
        public float showTime;

        public BufferedText(string text, float showTime)
        {
            this.text = text;
            this.showTime = showTime;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lean.Touch;

namespace Com.Hypester.DM3
{
    public class StageCarousel : MonoBehaviour
    {
        public enum Type { Normal, Tournament }
        public Type type;

        [SerializeField] GameObject stageEntryPrefab;
        private int _selectedIndex = 0;
        private int _highestIndex;
        private Coroutine _moveCoroutine;

        [SerializeField] SelectStageCanvas canvas;
        [SerializeField] private Image buttonPrevious;
        [SerializeField] private Image buttonNext;

        Color fullColor = new Color(1f, 1f, 1f, 1f);
        Color fadedColor = new Color(1f, 1f, 1f, .3f);

        List<GameObject> stageEntries = new List<GameObject>();

        private void OnEnable()
        {
            LeanTouch.OnFingerSwipe += OnFingerSwipe;
        }

        private void OnDisable()
        {
            LeanTouch.OnFingerSwipe -= OnFingerSwipe;
        }

        IEnumerator SmoothMove(Vector2 startpos , Vector2 endpos, float seconds)
        {
            float t = 0.0f;
            while (t <= 1.0)
            {
                t += Time.deltaTime / seconds;
                transform.localPosition = Vector2.Lerp(startpos, endpos, Mathf.SmoothStep(0.0f, 1.0f, t));
                yield return null;
            }
            _moveCoroutine = null;
        }

        private void OnFingerSwipe(LeanFinger finger)
        {
            var swipe = finger.SwipeScreenDelta;

            if (swipe.x < -Mathf.Abs(swipe.y))
            {
                SelectNext();
            }

            if (swipe.x > Mathf.Abs(swipe.y))
            {
                SelectPrevious();
            }
        }
        
        public void SelectNext ()
        {
            if (_moveCoroutine != null) {
                StopAllCoroutines();
            }

            if (_highestIndex >= (_selectedIndex + 1))
                _selectedIndex++;

            Vector2 wantedPosition = new Vector2(-1000f * _selectedIndex, 0f);
            Vector2 startspot = transform.localPosition;
            _moveCoroutine = StartCoroutine(SmoothMove(startspot, wantedPosition, 1.0f));

            ButtonColorCheck();
        }

        public void SelectPrevious()
        {
            if (_moveCoroutine != null) {
                StopAllCoroutines();
            }

            if (0 <= (_selectedIndex - 1))
                _selectedIndex--;

            Vector2 wantedPosition = new Vector2(-1000f * _selectedIndex, 0f);
            Vector2 startspot = transform.localPosition;
            _moveCoroutine = StartCoroutine(SmoothMove(startspot, wantedPosition, 1.0f));

            ButtonColorCheck();
        }

        void ButtonColorCheck()
        {
            if (_highestIndex >= (_selectedIndex + 1)) {
                buttonNext.color = fullColor;
                buttonNext.GetComponent<Button>().interactable = true;
            }
            else {
                buttonNext.color = fadedColor;
                buttonNext.GetComponent<Button>().interactable = false;
            }
            if (0 <= (_selectedIndex - 1)) {
                buttonPrevious.color = fullColor;
                buttonPrevious.GetComponent<Button>().interactable = true;
            }
            else {
                buttonPrevious.color = fadedColor;
                buttonPrevious.GetComponent<Button>().interactable = false;
            }
        }

        public void InitializeCarousel()
        {
            List<StageEntryInfoDB> entries = new List<StageEntryInfoDB>();
            if (type == Type.Normal)
            {
                StageEntryInfoDB entry1 = new StageEntryInfoDB(1000, 100, "Join three same color tiles", "normal");
                StageEntryInfoDB entry2 = new StageEntryInfoDB(4000, 1000, "", "normal");
                entries.Add(entry1);
                entries.Add(entry2);
            } else if (type == Type.Tournament)
            {
                StageEntryInfoDB entry1 = new StageEntryInfoDB(2000, 200, "Join three same color tiles", "tournament");
                StageEntryInfoDB entry2 = new StageEntryInfoDB(10000, 2000, "", "tournament");
                entries.Add(entry1);
                entries.Add(entry2);
            }

            int positionCounter = 0;
            foreach (StageEntryInfoDB entryInfo in entries)
            {
                StageEntry entry = Instantiate(stageEntryPrefab, transform, false).GetComponent<StageEntry>();

                entry.transform.localPosition = new Vector2(1000f * positionCounter, 0f); ;
                entry.playButton.onClick.RemoveAllListeners();
                entry.playButton.onClick.AddListener(()=> canvas.SetReady(entry));

                entry.SetCoinPrize(entryInfo.prizeCoinAmount);
                entry.SetCoinCost(entryInfo.entryCoinFee);
                entry.SetSpecialRule(entryInfo.specialRule);

                stageEntries.Add(entry.gameObject);
                positionCounter++;
            }

            _highestIndex = entries.Count - 1;
            ButtonColorCheck();
        }

        public void ClearCarousel()
        {
            for (int i=0; i<stageEntries.Count; i++)
            {
                Destroy(stageEntries[i]);
            }
            stageEntries.Clear();
        }
    }

    [System.Serializable]
    public class StageEntryInfoDB
    {
        public int prizeCoinAmount;
        public int entryCoinFee;
        public string specialRule;
        public string type;

        public StageEntryInfoDB(int prizeCoinAmount, int entryCoinFee, string specialRule, string type)
        {
            this.prizeCoinAmount = prizeCoinAmount;
            this.entryCoinFee = entryCoinFee;
            this.specialRule = specialRule;
            this.type = type;
        }
    }
}
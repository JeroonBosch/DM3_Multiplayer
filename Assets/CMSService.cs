using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMSService : MonoBehaviour {

    [SerializeField] KeyCode PingRequestKey;

    private void Update()
    {
        KeyControls();
    }

    void KeyControls()
    {
        if (Input.GetKeyDown(PingRequestKey)) {
            StartCoroutine(PingServer());
        }
    }

    IEnumerator PingServer()
    {

        Debug.Log("Starting ping");
        string pingUrl = "https://clash.hypester.com/api/ping";
        WWW request = new WWW(pingUrl);

        Debug.Log("Sending");
        yield return request;
        Debug.Log("Finished");

        if (!string.IsNullOrEmpty(request.error))
        {
            print("Error downloading: " + request.error);
        }
        else
        {
            // show the highscores
            Debug.Log(request.text);
            PingObject pingObject = new PingObject();
            pingObject = JsonUtility.FromJson<PingObject>(request.text);
            Debug.Log("Ping object: " + pingObject);
            Debug.Log(pingObject.chk);
        }

        Debug.Log("Unity Test");
        MyClass myObject = new MyClass();
        myObject.level = 1;
        myObject.timeElapsed = 47.5f;
        myObject.playerName = "Dr Charles Francis";
        string json = JsonUtility.ToJson(myObject);
        myObject = JsonUtility.FromJson<MyClass>(json);
        Debug.Log(json);
    }

    [Serializable]
    public class MyClass
    {
        public int level;
        public float timeElapsed;
        public string playerName;
    }

    [Serializable]
    public class PingObject
    {
        public int pong { get; set; }
        public int hourBonus { get; set; }
        public int experience { get; set; }
        public int XPlevel { get; set; }
        public int XPlevelGain { get; set; }
        public int XPlevelGainCurrent { get; set; }
        public int skillPoints { get; set; }
        public int coins { get; set; }
        public string user_id { get; set; }
        public int wheelEnabled { get; set; }
        public string chk { get; set; }
    }
}

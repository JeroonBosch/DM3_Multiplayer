using UnityEngine;

public class OnlyOne : MonoBehaviour {

	// Use this for initialization
	void OnEnable () {
        string tag = gameObject.tag;
        GameObject[] checkArray = GameObject.FindGameObjectsWithTag(tag);

        if (checkArray.Length > 1)
        {
            int count = 0;
            foreach (GameObject one in checkArray) { 
                if (count > 0)
                    Destroy(one.gameObject);
                count++;
            }
        }
	}
}

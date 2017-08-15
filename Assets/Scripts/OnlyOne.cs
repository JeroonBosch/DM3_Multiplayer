using UnityEngine;

public class OnlyOne : MonoBehaviour {

	// Use this for initialization
	void Start () {
        OnlyOne[] checkArray = FindObjectsOfType<OnlyOne>();

        if (checkArray.Length > 1)
        {
            int count = 0;
            foreach (OnlyOne one in checkArray) { 
                if (count > 0)
                    Destroy(one.gameObject);
                count++;
            }
        }
	}
}

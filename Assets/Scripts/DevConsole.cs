using UnityEngine;
using UnityEngine.UI;

public class DevConsole : MonoBehaviour {

    public static string gameId;
    public static string userId;

    [SerializeField] Text gameIdText;
    [SerializeField] Text userIdText;

    private void Update()
    {
        if (!string.IsNullOrEmpty(gameId)) { gameIdText.text = "Game id: " + gameId; } else { gameIdText.text = ""; }
        if (!string.IsNullOrEmpty(userId)) { userIdText.text = "User id: " + userId; } else { userIdText.text = ""; }
    }
}

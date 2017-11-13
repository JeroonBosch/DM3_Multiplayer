using System;
using System.Collections;
using UnityEngine;

public class AssetService : MonoBehaviour
{
    internal IEnumerator ImageFromURL(int playerId, string pictureURL, Action<Sprite, int> onLoadLocalPlayerProfileImage)
    {
        WWW www = new WWW(pictureURL);
        yield return www;

        if (!string.IsNullOrEmpty(www.error)) { Debug.LogError("Could not load image (" + www.error + ")"); }
        else
        {
            Debug.Log("Image loaded without errors.");
            Sprite resultImageSprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));
            if (onLoadLocalPlayerProfileImage != null && resultImageSprite != null)
            {
                onLoadLocalPlayerProfileImage(resultImageSprite, playerId);
            }
        }
    }
}
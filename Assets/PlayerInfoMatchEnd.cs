using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoMatchEnd : MonoBehaviour {

    public bool wantsRematch = false;

    [SerializeField] Text winnerText;
    [SerializeField] Image rematchImage;
    [SerializeField] Image avatarImage;
    [SerializeField] Image avatarBorder;
    [SerializeField] Image flagImage;
    [SerializeField] Text playerNameText;

    [SerializeField] Text currentXpLevelText;
    [SerializeField] Image currentXpLevelImage;

    [SerializeField] Text gainedXpText;
    [SerializeField] Image gainedXpImage;

    [SerializeField] Text gainedSkillText;
    [SerializeField] Image gainedSkillImage;

    [SerializeField] Button rematchButton;

    [SerializeField] Text gainedCoinsText;
    [SerializeField] Image gainedCoinsImage;

    public void ToggleWinnerText(bool value) { winnerText.enabled = value; }
    public void ToggleRematchImage(bool value) { rematchImage.enabled = value; wantsRematch = value; }
    public void ToggleRematchButton(bool value) { rematchButton.enabled = value; }
    public void SetAvatarImage(Sprite value) { if (value != null) { avatarImage.sprite = value; } }
    public void SetBorderImage(Sprite value) { if (value != null) { avatarBorder.sprite = value; } }
    public void SetFlagImage(Sprite value) { if (value != null) { flagImage.sprite = value; } }
    public void SetPlayerNameText(string value) { playerNameText.text = value; }

    public void SetCurrentXpLevelText(int value) { currentXpLevelText.text = value.ToString(); }
    public void SetGainedXpText(int value) { gainedXpText.text = "+" + value.ToString(); }
    public void SetGainedSkillText(int value) { gainedSkillText.text = "+" + value.ToString(); }
    public void SetGainedCoinsText(int value) { gainedCoinsText.text = "+" + value.ToString(); }
}

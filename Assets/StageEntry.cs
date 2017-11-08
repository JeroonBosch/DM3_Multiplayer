using UnityEngine;
using UnityEngine.UI;

public class StageEntry : MonoBehaviour {

	private string id;
    private string syscode;

    public int coinPrize { get; private set; }
    public int coinCost { get; private set; }

    private string specialRule;

    [SerializeField] Image splashArt;
    [SerializeField] Text coinPrizeText;
    [SerializeField] Text coinCostText;
    [SerializeField] Text amountOfPlayersOnlineText;
    [SerializeField] Text specialRuleTitleText;
    [SerializeField] Text specialRuleText;
    public Button playButton;

	void Update () {
        int playerCount = PhotonNetwork.connected ? PhotonNetwork.countOfPlayers : 0;
        amountOfPlayersOnlineText.text = "Players online: " + playerCount;
    }

	public void SetId(string id) {
		this.id = id;
	}
    public void SetSyscode(string syscode)
    {
        this.syscode = syscode;
    }
    public void SetCoinPrize(int value)
    {
        coinPrize = value;
        coinPrizeText.text = coinPrize.ToString();
    }
    public void SetCoinCost(int value)
    {
        coinCost = value;
        coinCostText.text = coinCost.ToString();
    }
    public void SetSpecialRule(string value)
    {
        specialRuleTitleText.enabled = value == "" ? false : true;
        specialRuleText.text = value;
    }
    public void SetBackground(Sprite newImage)
    {
        if (newImage == null) { Debug.LogWarning("No splash image to set."); return; }
        splashArt.sprite = newImage;
    }

	public string GetStageId() {
		return id;
	}
    public string GetStageSyscode()
    {
        return syscode;
    }
}

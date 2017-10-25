using UnityEngine;
using UnityEngine.UI;

public class StageEntry : MonoBehaviour {

    public int coinPrize { get; private set; }
    public int coinCost { get; private set; }

    string specialRule;

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
}

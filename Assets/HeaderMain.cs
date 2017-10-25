using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class HeaderMain : Header
    {
        [SerializeField] Text coinText;
        [SerializeField] Text unspentSkillPointText;

        private void OnEnable()
        {
            PlayerEvent.OnCoinAmountChange += CoinAmountChange;
            PlayerEvent.OnUnspentSkillPointAmountChange += UnspentSkillPointAmountChange;
        }

        private void OnDisable()
        {
            PlayerEvent.OnCoinAmountChange -= CoinAmountChange;
            PlayerEvent.OnUnspentSkillPointAmountChange -= UnspentSkillPointAmountChange;
        }

        void CoinAmountChange(int amount)
        {
            coinText.text = amount.ToString();
        }
        void UnspentSkillPointAmountChange(int amount)
        {
            unspentSkillPointText.text = amount.ToString();
        }

        public void FreeCoins()
        {
            FreeCoinsCanvas fcc = GameObject.Find("FreeCoinsScreen").GetComponent<FreeCoinsCanvas>();
            fcc.GoToScreen(fcc);
        }
        public void CoinShop()
        {
            CoinShopCanvas csc = GameObject.Find("CoinShopScreen").GetComponent<CoinShopCanvas>();
            csc.GoToScreen(csc);
        }
        public void SkillUpgrades()
        {
            UpgradeSkillCanvas usc = GameObject.Find("UpgradeSkillsScreen").GetComponent<UpgradeSkillCanvas>();
            usc.GoToScreen(usc);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Hypester.DM3
{
    public class SelectStageCanvas : BaseMenuCanvas
    {
        //Currently used for both the Normal 1v1 game and tournament.
        [SerializeField] GameObject coverForeground;
        [SerializeField] StageCarousel stageCarousel;

        public Canvas canvas;
        public bool isInitialized = false;

        protected override void Start()
        {
            base.Start();

            canvas = GetComponent<Canvas>();
            PhotonNetwork.autoJoinLobby = false;
            PhotonNetwork.automaticallySyncScene = true;
            coverForeground.SetActive(false);
        }

        public override void Show()
        {
            isInitialized = false;
            base.Show();
            stageCarousel.InitializeCarousel();
            Invoke("InitFinal", 0.1f);
        }

        void InitFinal()
        {
            isInitialized = true;
        }
        
        public override void Hide()
        {
            base.Hide();
            stageCarousel.ClearCarousel();
        }

        public void SetReady (StageEntry se)
        {
            // TODO: Server side check
            if (MainController.Instance.playerData.coins >= se.coinCost)
            {
                coverForeground.SetActive(true);
                MainController.Instance.playerData.AddCoins(-se.coinCost);
                PhotonController.Instance.MatchPlayers(se); //This should load the next scene.
            }
            else
            {
                UIEvent.InsufficientCurrency(SkillLevelEntry.Currency.Coins);
            }
        }

        public void PreviousScreen()
        {
            GoToScreen(PrevScreen());
        }
    }
}
using UnityEngine;

namespace Com.Hypester.DM3
{
    public class MainmenuCanvas : BaseMenuCanvas
    {
        public void PlayNormalGame()
        {
            GoToScreen(GameObject.Find("NormalGameScreen").GetComponent<SelectStageCanvas>());
            PhotonController.Instance.ConnectNormalLobby();
        }

        public void PlayTournament()
        {
            GoToScreen(GameObject.Find("TournamentGameScreen").GetComponent<SelectStageCanvas>());
            PhotonController.Instance.ConnectTournamentLobby();
        }
    }
}
namespace Com.Hypester.DM3
{
    public class FindMatchStateFactory
    {
        public FindMatchCanvas.FindMatchState.SearchingOpponent searchingOpponent { get; protected set; }
        public FindMatchCanvas.FindMatchState.GatheringOpponentData gatheringOpponentData { get; protected set; }
        public FindMatchCanvas.FindMatchState.GettingGameInfo gettingGameInfo { get; protected set; }
        public FindMatchCanvas.FindMatchState.StartingGame startingGame { get; protected set; }

        public FindMatchStateFactory(FindMatchCanvas context)
        {
            searchingOpponent = new FindMatchCanvas.FindMatchState.SearchingOpponent();
            gatheringOpponentData = new FindMatchCanvas.FindMatchState.GatheringOpponentData();
            gettingGameInfo = new FindMatchCanvas.FindMatchState.GettingGameInfo();
            startingGame = new FindMatchCanvas.FindMatchState.StartingGame();

            searchingOpponent.SetContextVariables(this, context);
            gatheringOpponentData.SetContextVariables(this, context);
            gettingGameInfo.SetContextVariables(this, context);
            startingGame.SetContextVariables(this, context);
        }
    }
}

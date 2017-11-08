
namespace Com.Hypester.DM3
{
    public partial class FindMatchCanvas
    {
        abstract public class FindMatchState
        {
            FindMatchCanvas context;
            public FindMatchStateFactory factory;

            virtual public void OnEnter() { }
            virtual public void Update() { }

            public void SetContextVariables(FindMatchStateFactory factory, FindMatchCanvas context)
            {
                this.factory = factory;
                this.context = context;
            }

            public class SearchingOpponent : FindMatchState
            {
                public override void OnEnter()
                {
                    base.OnEnter();
                    context.SearchOpponentStart();
                }
                public override void Update()
                {
                    base.Update();
                    context.SearchOpponentUpdate();
                }
            }
            public class GatheringOpponentData : FindMatchState
            {
                public override void OnEnter()
                {
                    base.OnEnter();
                    context.GatherOpponentDataStart();
                }
                public override void Update()
                {
                    base.Update();
                    context.GatherOpponentDataUpdate();
                }
            }
            public class GettingGameInfo : FindMatchState
            {
                public string remotePlayerUserId;

                public override void OnEnter()
                {
                    base.OnEnter();
                    context.GettingGameInfoStart();
                }
                public override void Update()
                {
                    base.Update();
                    context.GettingGameInfoUpdate();
                }
            }
            public class StartingGame : FindMatchState
            {
                public int gameId;

                public override void OnEnter()
                {
                    base.OnEnter();
                    context.StartingGameStart();
                }
                public override void Update()
                {
                    base.Update();
                    context.StartingGameUpdate();
                }
            }
        }
    }
}
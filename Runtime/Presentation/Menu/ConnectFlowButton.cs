namespace _Project.Presentation.Menu
{
    public class ConnectFlowButton : NetworkFlowButton
    {
        protected override void RequestMatch()
        {
            Matchmaking.JoinQueue();
        }
    }
}

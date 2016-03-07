namespace Mox.Lobby.Network
{
    internal class FreeReceptionDispatcher : IReceptionDispatcher
    {
        #region Implementation of IReceptionDispatcher

        public bool ReceiveMessagesSynchronously
        {
            get { return false; }
        }

        public void BeginInvoke(System.Action action)
        {
            action();
        }

        public void Invoke(System.Action action)
        {
            action();
        }

        public void OnAfterRequest()
        {
        }

        #endregion
    }
}
namespace Mox.UI.Game
{
    public class ManaPoolViewModel_DesignTime : ManaPoolViewModel
    {
        public ManaPoolViewModel_DesignTime()
        {
            Red.Amount = 10;
            Colorless.Amount = 2;
            Blue.Amount = 3;
            Blue.CanPay = true;
        }
    }
}
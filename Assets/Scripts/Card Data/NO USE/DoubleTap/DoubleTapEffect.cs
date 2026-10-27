public class DoubleTapEffect : ICardEffect
{
    public void Execute(IBattleEntity user, IBattleEntity target)
    {
        user.DoubleTapCharges += 1;
    }
}
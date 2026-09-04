public class LimitBreakEffect : ICardEffect
{
    public void Execute(IBattleEntity user, IBattleEntity target)
    {
        int currentStrength = user.GetStatusValue(StatusType.Strength);

        if (currentStrength <= 0)
            return; // Strength가 0 이하면 효과 없음

        user.AddStatus(new StrengthStatus(currentStrength));
    }
}

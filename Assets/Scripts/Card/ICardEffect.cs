public interface ICardEffect
{
    /// <summary>
    /// GameState를 받아 카드 효과 적용
    /// 에너지 소모/손패 처리는 GameStateManager에서 담당
    /// </summary>
    void Execute(GameState state);
}

/// <summary>
/// Entity를 조작하는 Controller
/// </summary>
public interface IEntityController
{
    /// <summary>
    /// 현재 상황에서 사용할 카드를 선택하여 리턴한다.
    /// null을 리턴하면 턴을 종료한다.
    /// </summary>
    /// <param name="entity">
    /// 현재 턴을 진행 중이며 카드를 사용할 Entity
    /// </param>
    /// <returns>
    /// 사용할 카드.
    /// 사용할 카드가 없으면 null
    /// </returns>
    CardInstance SelectCard(Entity entity);
}
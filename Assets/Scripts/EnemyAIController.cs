
using UnityEngine;

public class EnemyAIController : MonoBehaviour
{
    public object NextIntent { get; private set; }

    public void PrepareNextAction(GameState state) { }

    public GameState ExecuteAction(GameState state)
    {
        return state;
    }
}
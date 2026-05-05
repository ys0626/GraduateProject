
using UnityEngine;

public class BattleUIManager : MonoBehaviour
{
    public void UpdateAll(GameState state) { }
    public void ShowEnemyIntent(object intent) { }
    public void ShowEnemyAction(object action) { }
    public void ShowEnemyThinking() { }
    public void HideEnemyThinking() { }
    public void ShowNotEnoughEnergy() { }
    public void ShowBattleResult(bool playerWin) { }
}
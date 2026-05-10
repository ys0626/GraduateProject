using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHPBarUI : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;

    public void RefreshEnemyHPBar()
    {
        hpSlider.maxValue = GameData.instance.enemy.MaxHP;
        hpSlider.value = GameData.instance.enemy.CurrentHP;
    }
}
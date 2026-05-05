using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHPBarUI : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;

    private void Start()
    {
        hpSlider.maxValue = DBTest.instance.EnemyMaxHP;
        hpSlider.value = DBTest.instance.EnemyCurrentHP;
    }

    public void RefreshEnemyHPBar()
    {
        hpSlider.value = DBTest.instance.EnemyCurrentHP;
    }
}
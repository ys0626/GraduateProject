using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public static SceneChanger instance;

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
    }

    public void GoToMapScene()
    {
        SceneManager.LoadScene("MapScene");
    }

    public void GoToBattleScene()
    {
        SceneManager.LoadScene("BattleScene");
    }

    public void GoToShopScene()
    {
        SceneManager.LoadScene("ShopScene");
    }

    public void GoToEventScene()
    {
        SceneManager.LoadScene("EventScene");
    }

    public void GoToRestScene()
    {
        SceneManager.LoadScene("RestScene");
    }
}

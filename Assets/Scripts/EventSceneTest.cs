using UnityEngine;

public class EventSceneTest : MonoBehaviour
{
    public void GoToMapScene()
    {
        if(!SceneChanger.instance)
        {
            Debug.Log("SceneChanger가 없음");
            return;
        }
        else
        {
            SceneChanger.instance.GoToMapScene();
        }
    }
}

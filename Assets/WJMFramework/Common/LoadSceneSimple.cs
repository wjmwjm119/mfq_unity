using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadSceneSimple : MonoBehaviour
{

    public void LoadScene(string loadSceneName)
    {
        SceneManager.LoadScene(loadSceneName);
    }



}

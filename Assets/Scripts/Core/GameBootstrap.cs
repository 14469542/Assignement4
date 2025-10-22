using UnityEngine;


public class GameBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        Debug.Log("=== Pac-Man Game Bootstrap Started ===");

       
        GameObject menuObj = new GameObject("Main Menu");
        menuObj.AddComponent<MainMenu>();
        DontDestroyOnLoad(menuObj);

        Debug.Log("Bootstrap complete - Only Menu created");
    }
}
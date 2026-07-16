using UnityEngine;
using UnityEngine.SceneManagement;

// dead simple save/load - just remembers scene name
public static class SaveLoadSys
{
    private static string savedScene;
    private static int savedSceneIndex;

    // remember current scene
    public static void Save()
    {
        savedScene = SceneManager.GetActiveScene().name;
        savedSceneIndex = SceneManager.GetActiveScene().buildIndex;
        Debug.Log("Saved scene: " + savedScene);
    }

    // restart saved scene
    public static void Load()
    {
        if (string.IsNullOrEmpty(savedScene))
        {
            Debug.LogWarning("No save to load, dumbass!");
            return;
        }

        SceneManager.LoadScene(savedSceneIndex);
        Debug.Log("Loaded scene: " + savedScene);
    }
}
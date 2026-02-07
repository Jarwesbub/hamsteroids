using UnityEngine;
using System.Diagnostics;

public class MenuController : MonoBehaviour
{
    public void OnExitGame()
    {
        UnityEngine.Debug.Log("Exiting game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnOpenSaveFileLocation()
    {
        string path = Application.persistentDataPath;
        UnityEngine.Debug.Log($"Opening save file location: {path}");

        try
        {
            Application.OpenURL("file://" + path);

        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"Failed to open save file location: {e.Message}");
        }
    }
}

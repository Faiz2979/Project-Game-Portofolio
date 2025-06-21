using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    void Awake()
    {
        // Ensure that this GameObject persists across scene loads
        DontDestroyOnLoad(gameObject);
        if (FindObjectsOfType<UIManager>().Length > 1)
        {
            Destroy(gameObject);
        }

    }


    public void PlayFunction()
    {

    }

    public void QuitFunction()
    {
        // Quit the application
        Application.Quit();
        // If running in the editor, stop playing
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    
}

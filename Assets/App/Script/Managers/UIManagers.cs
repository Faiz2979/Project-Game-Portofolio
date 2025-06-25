using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIManagers : MonoBehaviour
{
    private PlayerControls playerControls;

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private bool isPaused = false;

    void Awake()
    {
        playerControls = new PlayerControls();
        
        // Ensure that this GameObject persists across scene loads
        DontDestroyOnLoad(gameObject);
        if (FindObjectsOfType<UIManagers>().Length > 1)
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

    void OnEnable()
    {
        playerControls.Enable();
        playerControls.UI.ToggleMenu.performed += ToggleMenu;
    }
    void OnDisable()
    {
        playerControls.Disable();
        playerControls.UI.ToggleMenu.performed -= ToggleMenu;
    }

    private void ToggleMenu(InputAction.CallbackContext context)
    {
        Debug.Log("Toggle menu");
        if (SceneManager.GetActiveScene().name == "MainMenu")
            return;
       
        isPaused = !isPaused;
        pauseMenu.SetActive(isPaused);
        Debug.Log("Pause menu " + (isPaused ? "opened" : "closed"));

    }

    public void ChangeScene(string sceneName)
    {
        // Implement scene change logic here
        SceneManager.LoadScene(sceneName);
    }

    public void TestFunction()
    {
        Debug.Log("Test function called");
    }

}

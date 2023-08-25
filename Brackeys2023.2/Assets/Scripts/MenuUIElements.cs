using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUIElements : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    /// <summary>
    /// Call this from teh main or the (not)pause menu. (it doesn't pause the game)
    /// </summary>
    public void SetAudioVolume(float volume)
    {

    }
}

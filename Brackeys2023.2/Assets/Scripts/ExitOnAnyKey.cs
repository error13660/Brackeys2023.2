using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitOnAnyKey : OnAnyKey
{
    protected override void OnKeyPress()
    {
        base.OnKeyPress();
        SceneManager.LoadScene(0);
    }
}

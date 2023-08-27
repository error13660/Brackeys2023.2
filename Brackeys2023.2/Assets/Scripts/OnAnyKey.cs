using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnAnyKey : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)
            || Input.GetKeyDown(KeyCode.W)
            || Input.GetKeyDown(KeyCode.S)
            || Input.GetKeyDown(KeyCode.A)
            || Input.GetKeyDown(KeyCode.D)
            || Input.GetKeyDown(KeyCode.Escape))
        {
            OnKeyPress();
        }
    }

    protected virtual void OnKeyPress()
    {
        UIManager.Instance.HideText();
    }
}

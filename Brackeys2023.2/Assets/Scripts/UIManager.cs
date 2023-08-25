using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }
    }

#nullable enable
    /// <summary>
    /// Sets the icon in the middle of the screen
    /// </summary>
    public void SetIcon(Sprite? icon)
    {

    }
#nullable disable

    public void SetIconFill(float f)
    {

    }

    public void DisplayText(string text)
    {

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseExitPrompt : OnAnyKey
{
    protected override void OnKeyPress()
    {
        UIManager.Instance.HideEscape();
    }
}

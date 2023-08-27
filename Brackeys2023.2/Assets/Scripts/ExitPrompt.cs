using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitPrompt : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        UIManager.Instance.PromptEscape();
    }
}

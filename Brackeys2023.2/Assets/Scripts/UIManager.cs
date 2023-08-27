using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField] private Image iconImage;
    [SerializeField] Sprite crosshairIcon;
    [SerializeField] GameObject readUI;
    [SerializeField] TextMeshProUGUI readText;
    [SerializeField] GameObject DeathUI;
    [SerializeField] GameObject EscapePrompt;
    [SerializeField] GameObject EscapeUi;

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

    /// <summary>
    /// Sets the icon in the middle of the screen
    /// </summary>
    public void SetIcon(Sprite icon)
    {
        if (icon == null)
        {
            iconImage.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            iconImage.sprite = crosshairIcon;
            return;
            SetIconFill(1);
        }

        iconImage.transform.localScale = new Vector3(1f, 1f, 1f);
        iconImage.sprite = icon;
    }

    public void SetIconFill(float f)
    {
        iconImage.fillAmount = f;
    }

    public void DisplayText(string text)
    {
        KinematicController.isPaused = true;
        readUI.SetActive(true);
        readText.SetText(text);
    }

    public void DisplayDead()
    {
        DeathUI.SetActive(true);
    }

    public void HideText()
    {
        KinematicController.isPaused = false;
        readUI.SetActive(false);
    }

    public void PromptEscape()
    {
        KinematicController.isPaused = true;
        EscapePrompt.SetActive(true);
    }

    public void HideEscape()
    {
        KinematicController.isPaused = false;
        EscapePrompt.SetActive(false);
    }

    public void ShowEscape()
    {
        EscapePrompt.SetActive(false);
        EscapeUi.SetActive(true);
    }
}

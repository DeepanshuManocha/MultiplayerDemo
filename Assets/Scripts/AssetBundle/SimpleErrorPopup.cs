// SimpleErrorPopup.cs
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class SimpleErrorPopup : MonoBehaviour
{
    public static SimpleErrorPopup Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button retryButton;

    private Action retryAction;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (panel != null) panel.SetActive(false);

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Show(string message, Action onRetry)
    {
        retryAction = onRetry;

        if (messageText != null) messageText.text = message;
        if (panel != null) panel.SetActive(true);
    }

    public void Hide()
    {
        retryAction = null;
        if (panel != null) panel.SetActive(false);
    }

    private void OnRetryClicked()
    {
        Action act = retryAction;
        Hide();
        act?.Invoke();
    }
}

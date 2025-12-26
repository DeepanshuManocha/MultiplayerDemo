using UnityEngine;
using TMPro;

public sealed class GameOverPopup : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text resultText;

    private void Awake()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void ShowWin()
    {
        Show("YOU WIN");
    }

    public void ShowLose()
    {
        Show("YOU LOSE");
    }

    private void Show(string msg)
    {
        if (resultText != null) resultText.text = msg;
        if (panel != null) panel.SetActive(true);
    }
}

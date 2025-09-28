using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject loginPanel;
    public GameObject registerPanel;

    void Start()
    {
        ShowLoginPanel();
    }

    public void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
    }

    public void ShowRegisterPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
    }

    public void OnSuccessfulAuth()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
    }

    public void OnCreateAccountButtonPressed()
    {
        ShowRegisterPanel();
    }

    public void OnBackToLoginButtonPressed()
    {
        ShowLoginPanel();
    }
}
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject loginPanel;
    public GameObject registerPanel;

    void Start()
    {
        if (!PlayerPrefs.HasKey("HasLaunchedBefore"))
        {
            ShowRegisterPanel();
        }
        else
        {
            ShowLoginPanel();
        }
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
        PlayerPrefs.SetInt("HasLaunchedBefore", 1);
        PlayerPrefs.Save();
    }
}
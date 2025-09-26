using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    public InputField usernameInput;
    public InputField passwordInput;
    public Text feedbackText;

    public void OnLoginButtonPressed()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            feedbackText.text = "������� ��� � ������.";
            return;
        }

        var request = new LoginWithPlayFabRequest
        {
            Username = username,
            Password = password
        };

        PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, OnLoginFailure);
    }

    void OnLoginSuccess(LoginResult result)
    {
        feedbackText.text = "���� ��������!";
        Debug.Log("PlayFabId: " + result.PlayFabId);

        // ��������� ���� �����
        PlayerPrefs.SetInt("HasLaunchedBefore", 1);
        PlayerPrefs.Save();

        // ����� ������� � ��������� ����� ��� �������� ��������
        // SceneManager.LoadScene("BattleScene");
    }

    void OnLoginFailure(PlayFabError error)
    {
        feedbackText.text = "������ �����: " + error.ErrorMessage;
        Debug.LogError(error.GenerateErrorReport());
    }
}
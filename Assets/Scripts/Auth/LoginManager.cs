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
            feedbackText.text = "Введите ник и пароль.";
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
        feedbackText.text = "Вход выполнен!";
        Debug.Log("PlayFabId: " + result.PlayFabId);

        // Сохраняем флаг входа
        PlayerPrefs.SetInt("HasLaunchedBefore", 1);
        PlayerPrefs.Save();

        // Можно перейти к следующей сцене или включить геймплей
        // SceneManager.LoadScene("BattleScene");
    }

    void OnLoginFailure(PlayFabError error)
    {
        feedbackText.text = "Ошибка входа: " + error.ErrorMessage;
        Debug.LogError(error.GenerateErrorReport());
    }
}
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;

public class AuthManager : MonoBehaviour
{
    public InputField usernameInput;
    public InputField emailInput;
    public InputField passwordInput;
    public InputField confirmPasswordInput;
    public Button registerButton;
    public Text feedbackText;

    void Start()
    {
        registerButton.onClick.AddListener(RegisterPlayer);
    }

    void RegisterPlayer()
    {
        string username = usernameInput.text.Trim();
        string email = emailInput.text.Trim();
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            feedbackText.text = "Заполните все поля.";
            return;
        }

        if (password.Length < 6)
        {
            feedbackText.text = "Пароль должен быть не менее 6 символов.";
            return;
        }

        if (password != confirmPassword)
        {
            feedbackText.text = "Пароли не совпадают.";
            return;
        }

        var request = new RegisterPlayFabUserRequest
        {
            Username = username,
            Email = email,
            Password = password,
            RequireBothUsernameAndEmail = true
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnRegisterFailure);
    }

    void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        feedbackText.text = "Регистрация успешна!";
        Debug.Log("PlayFabId: " + result.PlayFabId);
    }

    void OnRegisterFailure(PlayFabError error)
    {
        feedbackText.text = "Ошибка: " + error.ErrorMessage;
        Debug.LogError(error.GenerateErrorReport());
    }
}

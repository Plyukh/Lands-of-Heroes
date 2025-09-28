using System;
using UnityEngine;
using UnityEngine.UI;

public class AuthUIController : MonoBehaviour
{
    [Header("Login UI")]
    public InputField loginUsername;
    public InputField loginPassword;
    public Text loginFeedback;

    [Header("Register UI")]
    public InputField regUsername;
    public InputField regEmail;
    public InputField regPassword;
    public InputField regConfirm;
    public Text regFeedback;
    public Button regButton;

    [Header("Window Manager")]
    public UIManager uiManager;

    public IAuthService AuthService { get; set; }
    public Action<PlayerData> OnLoggedIn;

    void Awake()
    {
        AuthService = new PlayFabAuthService(); // или через DI
        regButton.onClick.AddListener(OnRegisterClicked);
    }

    public async void OnLoginClicked()
    {
        loginFeedback.text = "";
        try
        {
            // вызываем логин
            await AuthService.Login(
                loginUsername.text.Trim(),
                loginPassword.text
            );

            // загружаем данные
            var pd = await AuthService.LoadPlayerData();

            // показываем сообщение об успехе
            loginFeedback.text = "Вход выполнен!";

            // скрываем окна
            uiManager.OnSuccessfulAuth();

            // прокидываем результат наверх
            OnLoggedIn?.Invoke(pd);
        }
        catch (Exception e)
        {
            loginFeedback.text = e.Message;
        }
    }

    public async void OnRegisterClicked()
    {
        regFeedback.text = "";

        if (regPassword.text != regConfirm.text)
        {
            regFeedback.text = "Пароли не совпадают";
            return;
        }

        try
        {
            // регистрируем
            await AuthService.Register(
                regUsername.text.Trim(),
                regEmail.text.Trim(),
                regPassword.text
            );

            // говорим пользователю, что регистрация прошла
            regFeedback.text = "Регистрация успешна! Выполняется вход...";

            // инициализируем дефолтные данные
            await AuthService.InitializePlayerData();

            // логиним автоматически
            await AuthService.Login(
                regUsername.text.Trim(),
                regPassword.text
            );

            // загружаем данные
            var pd = await AuthService.LoadPlayerData();

            // обновляем текст, что вход выполнен
            regFeedback.text = "Вход выполнен!";

            // скрываем все окна
            uiManager.OnSuccessfulAuth();

            // передаём данные дальше
            OnLoggedIn?.Invoke(pd);
        }
        catch (Exception e)
        {
            regFeedback.text = e.Message;
        }
    }
}
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
        AuthService = new PlayFabAuthService(); // ��� ����� DI
        regButton.onClick.AddListener(OnRegisterClicked);
    }

    public async void OnLoginClicked()
    {
        loginFeedback.text = "";
        try
        {
            // �������� �����
            await AuthService.Login(
                loginUsername.text.Trim(),
                loginPassword.text
            );

            // ��������� ������
            var pd = await AuthService.LoadPlayerData();

            // ���������� ��������� �� ������
            loginFeedback.text = "���� ��������!";

            // �������� ����
            uiManager.OnSuccessfulAuth();

            // ����������� ��������� ������
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
            regFeedback.text = "������ �� ���������";
            return;
        }

        try
        {
            // ������������
            await AuthService.Register(
                regUsername.text.Trim(),
                regEmail.text.Trim(),
                regPassword.text
            );

            // ������� ������������, ��� ����������� ������
            regFeedback.text = "����������� �������! ����������� ����...";

            // �������������� ��������� ������
            await AuthService.InitializePlayerData();

            // ������� �������������
            await AuthService.Login(
                regUsername.text.Trim(),
                regPassword.text
            );

            // ��������� ������
            var pd = await AuthService.LoadPlayerData();

            // ��������� �����, ��� ���� ��������
            regFeedback.text = "���� ��������!";

            // �������� ��� ����
            uiManager.OnSuccessfulAuth();

            // ������� ������ ������
            OnLoggedIn?.Invoke(pd);
        }
        catch (Exception e)
        {
            regFeedback.text = e.Message;
        }
    }
}
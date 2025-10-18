using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AuthUIController : MonoBehaviour
{
    [Header("Login UI")]
    public TMP_InputField loginUsername;
    public TMP_InputField loginPassword;
    public TextMeshProUGUI loginFeedback;

    [Header("Register UI")]
    public TMP_InputField regUsername;
    public TMP_InputField regEmail;
    public TMP_InputField regPassword;
    public TMP_InputField regConfirm;
    public TextMeshProUGUI regFeedback;
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
            await ProcessLogin(loginUsername.text.Trim(), loginPassword.text, loginFeedback);
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
            regFeedback.text = "Passwords do not match";
            return;
        }

        try
        {
            // Registration
            await AuthService.Register(
                regUsername.text.Trim(),
                regEmail.text.Trim(),
                regPassword.text
            );

            regFeedback.text = "Registration successful! Logging in...";

            // Initialize player data
            await AuthService.InitializePlayerData();

            // Automatically log in the new user
            await ProcessLogin(regUsername.text.Trim(), regPassword.text, regFeedback);
        }
        catch (Exception e)
        {
            regFeedback.text = e.Message;
        }
    }

    /// <summary>
    /// Handles the login process and subsequent UI/data updates.
    /// </summary>
    private async Task ProcessLogin(string username, string password, TextMeshProUGUI feedback)
    {
        // Login
        await AuthService.Login(username, password);

        // Load player data
        var playerData = await AuthService.LoadPlayerData();

        // Update feedback text
        feedback.text = "Login successful!";

        // Notify UI Manager to switch scenes/views
        uiManager.OnSuccessfulAuth();

        // Invoke the global event for other systems
        OnLoggedIn?.Invoke(playerData);
    }
}
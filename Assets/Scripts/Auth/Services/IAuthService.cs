using PlayFab.ClientModels;
using System.Threading.Tasks;

public interface IAuthService
{
    Task<RegisterPlayFabUserResult> Register(string username, string email, string password);
    Task<LoginResult> Login(string username, string password);
    Task InitializePlayerData();
    Task<PlayerData> LoadPlayerData();
}
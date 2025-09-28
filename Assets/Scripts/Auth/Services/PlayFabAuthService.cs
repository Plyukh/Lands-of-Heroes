using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayFabAuthService : IAuthService
{
    public Task<RegisterPlayFabUserResult> Register(string username, string email, string password)
    {
        var tcs = new TaskCompletionSource<RegisterPlayFabUserResult>();
        var request = new RegisterPlayFabUserRequest
        {
            Username = username,
            Email = email,
            Password = password,
            RequireBothUsernameAndEmail = true
        };

        PlayFabClientAPI.RegisterPlayFabUser(request,
            result => tcs.SetResult(result),
            error => tcs.SetException(new Exception(error.ErrorMessage))
        );

        return tcs.Task;
    }

    public Task<LoginResult> Login(string username, string password)
    {
        var tcs = new TaskCompletionSource<LoginResult>();
        var request = new LoginWithPlayFabRequest
        {
            Username = username,
            Password = password
        };

        PlayFabClientAPI.LoginWithPlayFab(request,
            result => tcs.SetResult(result),
            error => tcs.SetException(new Exception(error.ErrorMessage))
        );

        return tcs.Task;
    }

    public Task InitializePlayerData()
    {
        var tcs = new TaskCompletionSource<bool>();

        // 1) Базовые поля
        var data = new Dictionary<string, string>
        {
            { "Level",      "1" },
            { "Experience", "0" },
            { "Gold",       "0" }
        };

        // 2) Загружаем все CreatureData и фильтруем по persistInPlayerData,
        //    затем сортируем по id (строковая сортировка по возрастанию).
        var sortedCreatures = Resources
            .LoadAll<CreatureData>("Creatures")
            .Where(cd => cd.persistInPlayerData)
            .OrderBy(cd => cd.id)          // ← вот оно
            .ToList();

        // 3) Создаём словарь в порядке sortedCreatures
        var persistDict = new Dictionary<string, int>();
        foreach (var cd in sortedCreatures)
            persistDict[cd.id] = 0;

        // 4) Сериализуем и отправляем
        data["CreatureLevels"] = JsonUtility
            .ToJson(new SerializableDictionary<string, int>(persistDict));

        var request = new UpdateUserDataRequest { Data = data };
        PlayFabClientAPI.UpdateUserData(request,
            _ => tcs.SetResult(true),
            error => tcs.SetException(new Exception(error.ErrorMessage))
        );

        return tcs.Task;
    }

    public Task<PlayerData> LoadPlayerData()
    {
        var tcs = new TaskCompletionSource<PlayerData>();

        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
        result =>
        {
            try
            {
                // 1) Парсим базовые поля
                var d = result.Data;
                int level = int.Parse(d["Level"].Value);
                int exp = int.Parse(d["Experience"].Value);
                int gold = int.Parse(d["Gold"].Value);

                // 2) Десериализуем полный словарь уровней существ
                var json = d["CreatureLevels"].Value;
                var fullDict = JsonUtility
                    .FromJson<SerializableDictionary<string, int>>(json)
                    .ToDictionary();

                // 3) Опять загружаем CreatureData и собираем set допустимых id
                var creatures = Resources.LoadAll<CreatureData>("Creatures");
                var validIds = new HashSet<string>(
                    creatures
                        .Where(cd => cd.persistInPlayerData)
                        .Select(cd => cd.id)
                );

                // 4) Фильтруем словарь по validIds
                var filteredDict = fullDict
                    .Where(kv => validIds.Contains(kv.Key))
                    .ToDictionary(kv => kv.Key, kv => kv.Value);

                // 5) Создаём доменную модель и возвращаем её
                var pd = new PlayerData(level, exp, gold, filteredDict);
                tcs.SetResult(pd);
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        },
        error =>
        {
            tcs.SetException(new Exception(error.ErrorMessage));
        });

        return tcs.Task;
    }
}
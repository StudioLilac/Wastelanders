using Systems.Persistence;
using UnityEngine;

public static class Versioning
{
    public const int CURRENT_GAMEDATA_VERSION = 1;
    public const int CURRENT_PREFERENCES_VERSION = 1;

    public static GameData MigrateGameData(GameData data)
    {
        if (data.SaveVersion == CURRENT_GAMEDATA_VERSION)
            return data;

        switch (data.SaveVersion)
        {
            case 0:
                Debug.Log("Migrating GameData from v0 to v1...");
                data.SaveVersion = 1;
                goto default;
            default:
                break;
        }

        if (data.SaveVersion != CURRENT_GAMEDATA_VERSION)
        {
            Debug.LogError($"Failed to fully migrate GameData. Expected v{CURRENT_GAMEDATA_VERSION}, but stopped at v{data.SaveVersion}");
        }

        return data;
    }

    public static UserPreferences MigratePreferences(UserPreferences preferences)
    {
        if (preferences.SaveVersion == CURRENT_PREFERENCES_VERSION)
            return preferences;

        switch (preferences.SaveVersion)
        {
            case 0:
                Debug.Log("Migrating UserPreferences from v0 to v1...");
                preferences.SaveVersion = 1;
                goto default;
            default:
                break;
        }

        if (preferences.SaveVersion != CURRENT_PREFERENCES_VERSION)
        {
            Debug.LogError($"Failed to fully migrate UserPreferences. Expected v{CURRENT_PREFERENCES_VERSION}, but stopped at v{preferences.SaveVersion}");
        }

        return preferences;
    }

}

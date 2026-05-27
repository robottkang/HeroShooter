using UnityEngine;

public static class LocalPlayerData
{
    public static string PlayerName
    {
        get => PlayerPrefs.GetString("PlayerName", "Player");
        set => PlayerPrefs.SetString("PlayerName", value);
    }
}

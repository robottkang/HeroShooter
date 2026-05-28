using UnityEngine;

public static class LocalPlayerData
{
    private static string _nickName;

    public static string NickName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_nickName))
            {
                var rngPlayer = Random.Range(0, 9999);
                _nickName = $"Player {rngPlayer.ToString("0000")}";
            }
            return _nickName;
        }
        set => _nickName = value;
    }
}

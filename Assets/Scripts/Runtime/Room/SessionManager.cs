using Cysharp.Threading.Tasks;
using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionManager : SimulationBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    //[SerializeField] private GameObject localPlayerArmsPrefab;
    [SerializeField] private Transform[] spawnPoints;

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] private bool isDebug;
    [SerializeField] private NetworkRunner runnerPrefab;
#else
    private bool isDebug = false;
#endif

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Camera.main.gameObject.SetActive(false);

#if UNITY_EDITOR
        if (isDebug && FindFirstObjectByType<NetworkRunner>() == null)
        {
            StartDebugSession().Forget();
            return;
        }
#endif
        RegisterOnRunner();
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        string sessionName = Runner.SessionInfo.Name;
        string sessionCreatedTime = Runner.SessionInfo.Properties.TryGetValue(SessionKeys.CreatedTime, out var ct) ?
            ct.PropertyValue.ToString() : "";
        int seed = (sessionName + sessionCreatedTime).GetHashCode();

        var rng = new System.Random(seed);

        Transform spawnPoint;
        if (Runner.IsSharedModeMasterClient ^ (rng.Next(0, 2) == 0))
            spawnPoint = spawnPoints[0];
        else
            spawnPoint = spawnPoints[1];
        
        //Instantiate(localPlayerArmsPrefab, spawnPoint.position, spawnPoint.rotation);
        Runner.SpawnAsync(playerPrefab, spawnPoint.position, spawnPoint.rotation, Runner.LocalPlayer);
    }
#if UNITY_EDITOR
    public void RegisterOnRunner()
    {
        var runner = NetworkRunner.GetRunnerForGameObject(gameObject);

        if (runner.IsRunning)
        {
            runner.AddGlobal(this);
        }
    }

    private async UniTask StartDebugSession()
    {
        var runner = Instantiate(runnerPrefab);

        var props = new Dictionary<string, SessionProperty>
        {
            { SessionKeys.HostName, "Debug" },
            { SessionKeys.CreatedTime, DateTime.Now.ToString("HHmmssfff") }
        };

        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = "debug-session",
            SessionProperties = props,
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
        }).AsUniTask();

        if (!result.Ok)
        {
            Debug.LogError($"[Debug] StartGame failed: {result.ShutdownReason}");
            return;
        }

        await runner.LoadScene(SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex)).ToUniTask();
    }
#endif
}

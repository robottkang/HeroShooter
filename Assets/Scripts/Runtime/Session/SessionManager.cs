using Cysharp.Threading.Tasks;
using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SessionManager : SimulationBehaviour, IEventListener<PlayerDiedEvent>, IEventListener<PlayerReadyEvent>
{
    [SerializeField] private GameObject playerPrefab;
    //[SerializeField] private GameObject localPlayerArmsPrefab;
    [SerializeField] private Transform[] spawnPoints;

    private StateMachine<SessionManager> _gameFSM;
    private readonly SessionReadyState _sessionReady = new();
    private readonly SessionPlayState _sessionPlaying = new();
    private readonly SessionResetState _sessionReset = new();
    private readonly SessionResultState _sessionResult = new();
    private bool _isLeavingSession = false;
    private int _readyPlayerCount;

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] private bool isDebug;
    [SerializeField] private GameMode debugGameMode = GameMode.Single;
    [SerializeField] private NetworkRunner runnerPrefab;
#endif

    private void OnEnable()
    {
        EventBus<PlayerDiedEvent>.Register(this);
    }

    private void OnDisable()
    {
        EventBus<PlayerDiedEvent>.Unregister(this);
    }

    private void Awake()
    {
        _gameFSM = new StateMachine<SessionManager>(this, _sessionReady);
    }

    private void Start()
    {
        Init().Forget();
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame && !_isLeavingSession)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            LeaveSessionAndReturnToLobby().Forget();
        }
    }

    private async UniTaskVoid Init()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (Runner != null)
        {
            RegisterOnRunner();
            SpawnPlayer();
        }
#if UNITY_EDITOR
        else if (isDebug)
        {
            await StartDebugSession();
        }
#endif
        else
        {
            LeaveSessionAndReturnToLobby().Forget();
        }
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

    public async UniTaskVoid LeaveSessionAndReturnToLobby()
    {
        _isLeavingSession = true;

        if (Runner != null)
        {
            await Runner.Shutdown();

            Destroy(Runner.gameObject);
        }

        SceneManager.LoadScene("Lobby");
    }


    public void RegisterOnRunner()
    {
        var runner = NetworkRunner.GetRunnerForGameObject(gameObject);

        if (runner.IsRunning)
            runner.AddGlobal(this);
    }

    public void OnEvent(PlayerDiedEvent e)
    {
        _gameFSM.ChangeState(new SessionResultState());
    }

    public void OnEvent(PlayerReadyEvent e)
    {
        _readyPlayerCount++;
        if (_readyPlayerCount >= 2)
            _gameFSM.ChangeState(new SessionPlayState());
    }

    #region Debug
#if UNITY_EDITOR
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
            GameMode = debugGameMode,
            SessionName = "debug-session",
            SessionProperties = props,
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
        }).AsUniTask();

        if (!result.Ok)
        {
            Debug.LogError($"[Debug] StartGame failed: {result.ShutdownReason}");
            return;
        }

        RegisterOnRunner();
        SpawnPlayer();
        /*
        if (runner.IsSharedModeMasterClient)
            await runner.LoadScene(SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex)).ToUniTask();
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);*/
    }
#endif
    #endregion
}

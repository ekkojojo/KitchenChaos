using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KitchenGameManager : NetworkBehaviour
{
    public static KitchenGameManager Instance {  get; private set; }
    public event EventHandler OnStageChanged;
    public event EventHandler OnLocalGamePaused;
    public event EventHandler OnLocalGameUnPaused;
    public event EventHandler OnMultiplayerGamePaused;
    public event EventHandler OnMultiplayerGameUnPaused;
    //public event EventHandler OnLocalPlayerReadyChanged;
    enum GameState
    {
        WaitingToStart,
        CountDownToStart,
        GamePlaying,
        GameOver
    }

    [SerializeField] Transform playerPrefab;

    NetworkVariable<GameState> gameState=new(GameState.WaitingToStart);
    NetworkVariable<float> CountDownToStartTimer = new(3f);
    NetworkVariable<float> GamePlayingTimer = new(0f);
    NetworkVariable<bool> isGamePaused = new(false);
    float GamePlayingTimerMax = 90f;
    bool isLocalPlayerReady;
    bool isLocalGamePaused = false;
    bool autoGamePaused = false;
    Dictionary<ulong, bool> playerReadyDictionary;
    Dictionary<ulong, bool> playerPauseDictionary;

    private void Awake()
    {
        Instance = this;

        playerReadyDictionary = new Dictionary<ulong, bool>();
        playerPauseDictionary = new Dictionary<ulong, bool>();
    }
    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    public override void OnNetworkSpawn()
    {
        gameState.OnValueChanged += State_OnStateChanged;
        isGamePaused.OnValueChanged += isGamePaused_OnValueChanged;

        if(IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach(ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform playerTransform = Instantiate(playerPrefab);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID,true);
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong obj)
    {
        autoGamePaused = true;
    }

    private void isGamePaused_OnValueChanged(bool previousValue, bool newValue)
    {
        if(isGamePaused.Value)
        {
            Time.timeScale = 0f;

            OnMultiplayerGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;

            OnMultiplayerGameUnPaused?.Invoke(this, EventArgs.Empty);
        }
    }

    private void State_OnStateChanged(GameState previousValue, GameState newValue)
    {
        OnStageChanged?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if(gameState.Value == GameState.WaitingToStart)
        {
            isLocalPlayerReady = true;

            SetPlayerReadyServerRpc();

            //OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);

            //gameState.Value = GameState.CountDownToStart;
            //OnStageChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams=default)
    {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach(ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientID) || !playerReadyDictionary[clientID]) 
            {
                allClientsReady = false;
                break;
            }
        }
        if (allClientsReady)
        {
            gameState.Value = GameState.CountDownToStart;
        }
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }
        switch (gameState.Value)
        {
            case GameState.WaitingToStart:
                /*waitingToStartTimer -= Time.deltaTime;
                if (waitingToStartTimer < 0f)
                {
                    gameState = GameState.CountDownToStart;
                    OnStageChanged?.Invoke(this, EventArgs.Empty);print("trigger");
                }*/
                break;
            case GameState.CountDownToStart:
                CountDownToStartTimer.Value -= Time.deltaTime;
                if (CountDownToStartTimer.Value < 0f)
                {
                    gameState.Value = GameState.GamePlaying;
                    GamePlayingTimer.Value = GamePlayingTimerMax;
                    //OnStageChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case GameState.GamePlaying:
                GamePlayingTimer.Value -= Time.deltaTime;
                if (GamePlayingTimer.Value < 0f)
                {
                    gameState.Value = GameState.GameOver;
                    //OnStageChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case GameState.GameOver:
                
                break;
        }
        //print(gameState.Value);
    }

    private void LateUpdate()
    {
        if (autoGamePaused)
        {
            autoGamePaused = false;
            TestGamePausedState();
        }
    }

    public bool IsLocalPlayerReady() 
    {  
        return isLocalPlayerReady; 
    } 

    public bool IsWatingToStart()
    {
        return gameState.Value == GameState.WaitingToStart;
    }

    public bool IsGamePlaying()
    {
        return gameState.Value == GameState.GamePlaying;
    }

    public bool IsCountdownToStartActive()
    {
        return gameState.Value == GameState.CountDownToStart;
    }

    public bool IsGameOver()
    {
        return gameState.Value == GameState.GameOver;
    }

    public float GetCoutDownToStartTimer()
    {
        return CountDownToStartTimer.Value;
    }

    public float GetGamePlayingTimerNormalized()
    {
        return 1 - (GamePlayingTimer.Value / GamePlayingTimerMax);
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        TogglePauseGame();
    }

    public void TogglePauseGame()
    {
        isLocalGamePaused = !isLocalGamePaused;
        if (isLocalGamePaused)
        {
            PauseGameServerRpc();

            OnLocalGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            UnpauseGameServerRpc();

            OnLocalGameUnPaused?.Invoke(this, EventArgs.Empty);
        }   
    }

    [ServerRpc(RequireOwnership =false)]
    void PauseGameServerRpc(ServerRpcParams serverRpcParams=default)
    {
        playerPauseDictionary[serverRpcParams.Receive.SenderClientId] = true;

        TestGamePausedState();
    }

    [ServerRpc(RequireOwnership = false)]
    void UnpauseGameServerRpc(ServerRpcParams serverRpcParams=default)
    {
        playerPauseDictionary[serverRpcParams.Receive.SenderClientId] = false;

        TestGamePausedState();
    }

    void TestGamePausedState()
    {
        foreach(ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (playerPauseDictionary.ContainsKey(clientID) && playerPauseDictionary[clientID])
            {
                isGamePaused.Value = true;
                return;
            }
        }

        isGamePaused.Value= false;
    }
}

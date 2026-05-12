using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KitchenGameManager : NetworkBehaviour
{

    public static KitchenGameManager Instance {  get; private set; }

    //通知倒计时UI改变的事件
    public event EventHandler OnStateChange;

    //通知开启暂停和关闭暂停的时间
    public event EventHandler OnLocalPauseUIAction;
    public event EventHandler OnLocalUnPauseUIAction;
    public event EventHandler OnLocalPlayerReadyChanged;

    //通知多人游戏暂停和取消暂停的事件 有人处于或都不处于暂停状态
    public event EventHandler OnMultiplayerGamePaused;
    public event EventHandler OnMultiplayerGameUnpaused;    

    private enum State
    {
        WaitingToStart,//等待开始 这个状态更有意义的是后续在多人游戏种等待一起准备才允许开始游戏
        CountdownToStart,//倒计时后开始
        GamePlaying,//游戏中
        GameOver,//游戏结束
        
    }
    //将State变为网络变量
    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);
    //本地玩家是否准备
    private bool isLocalPlayerReady;
    private float waittingToStartTimer = 1f;
    //倒计时开始时常
    private NetworkVariable<float> countDownToStartTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);
    private float gamePlayingTimerMax = 90f; //最大时长

    //检测是否有经历过countDownToStart部分了
    private bool isCountdownToStartPassed;

    //关联玩家以解决之前那样需要Instance来监听事件的问题
    [SerializeField] private Player player;

    //玩家准备字典
    private Dictionary<ulong, bool> playerReadyDictionary;
    //玩家暂停字典
    private Dictionary<ulong, bool> playerPauseDictionary;
    //是否处于时间暂停状态
    public bool isLocalGamePause;
    //网络变量存储实际的网络暂停状态
    private NetworkVariable<bool> isGamePause = new NetworkVariable<bool>(false);
    //用于等待一帧
    private bool autoTestGamePausedState;
    private void Awake()
    { 
        Instance = this;
        isCountdownToStartPassed = false;
       
        playerReadyDictionary = new Dictionary<ulong, bool>();
        playerPauseDictionary = new Dictionary<ulong, bool>();
    


    }
    private void Start() 
    {
        //暂停
        GameInput.Instance.OnPauseAction += PlayerPause_OnPauseAction; 
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;


        
    } 
    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;
        //监听暂停这个网络变量的变化
        isGamePause.OnValueChanged += IsGamePause_OnValueChanged;

        //监听是否断开连接
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += NetworkManager_OnLoadEventCompleted;
        }
    }
    //玩家预制体 用于下面创建使用
    [SerializeField] private Transform playerObj;





    #region 教程里的方法 利用加载完成后的回调创建对象
    private void NetworkManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clienid in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform playerTransform = Instantiate(playerObj);
            //根据id生成对象 第二个参数表示随场景销毁而销毁 玩家对象确实需要这样
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clienid, true);

        }
    }
    #endregion

    private void NetworkManager_OnClientDisconnectCallback(ulong client)
    {
        //设置这个主要是为了等待一帧已让下面执行 因为如果不等待 其实再触发回调时 断开链接的客户端还是再列表里的 所以要等待一帧
        autoTestGamePausedState = true ;
    }
    private void LateUpdate()
    {
        if (autoTestGamePausedState)
        {
            autoTestGamePausedState=  false;
            TestGamePauseState();

        }
    }

    private void IsGamePause_OnValueChanged(bool previousValue, bool newValue)
    {
        if (isGamePause.Value)
        {
            Time.timeScale = 0f; 
            //触发UI显示
            OnMultiplayerGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            OnMultiplayerGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChange?.Invoke(this, EventArgs.Empty);

    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        //如果是第一次进入游戏还没尽力过countdown阶段
        if (state.Value == State.WaitingToStart)
        {
            //变为准备
           isLocalPlayerReady = true;
            OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);
            //检擦是否处于准备状态
            SetPlayerReadyServerRpc();
            


        }
       
       
    }
    //通知玩家加入准备字典 (检查是否准备）
    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        //字典里标记未已准备
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;
        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                //如果任意玩家没准备 
                allClientsReady = false;
                break;

            }
        }
        Debug.Log("玩家准备状态：" + allClientsReady);
        //所有玩家准备就绪 就切换状态
        if (allClientsReady)
        {
            state.Value = State.CountdownToStart;
        }
    }


    //执行暂停
    private void PlayerPause_OnPauseAction(object sender, EventArgs e)
    {
        TogglePauseGame();
 
    }

    private void Update()
    {
        //状态改变只在服务器发生
        if(!IsServer) return;
        switch (state.Value)
        {//结束对应状态后进入下一个状态
            case State.WaitingToStart:
             
                break;
            case State.CountdownToStart:
                countDownToStartTimer.Value -= Time.deltaTime;
                if (countDownToStartTimer.Value < 0f)
                {
                    gamePlayingTimer.Value = gamePlayingTimerMax;
                    state.Value = State.GamePlaying;
                }
                break;
            case State.GamePlaying:
                gamePlayingTimer.Value -= Time.deltaTime;
                if (gamePlayingTimer.Value < 0f)
                {
                    state.Value = State.GameOver;
                }
                break;
            case State.GameOver:
                break;
        }
    }
    //返回给外部 当不处于游戏状态时静止交互
    public bool IsGamePlaying()
    {
        bool isPlaying = state.Value == State.GamePlaying;
        return isPlaying;
    }
    //是否处于倒计时开始状态
    public bool IsCountdownToStartActive()
    {
        return state.Value == State.CountdownToStart;
    }
    public bool IsWaitingToStartActive()
    {
        return state.Value == State.WaitingToStart;
    }
    //返回倒计时数字 让UI能够得到
    public float GetCountdownToStartTimer()
    {
        return countDownToStartTimer.Value;
    }
    //是否处于结束状态
    public bool IsGameOverActive()
    {
        return state.Value == State.GameOver;
    }
    //公开是否准备
    public bool IsLocalPlayerReady()
    {
        return isLocalPlayerReady;
    }
    public float GetGamePlayingTimerNormalized()
    {
        //因为是倒计时我们要取反
        return gamePlayingTimer.Value    / gamePlayingTimerMax;
    }
    public  void TogglePauseGame()
    {
        isLocalGamePause = !isLocalGamePause;
        //暂停时间
        if (isLocalGamePause)
        {
            PauseGameServerRpc();
            //通知开启暂停 
            OnLocalPauseUIAction?.Invoke(this, EventArgs.Empty);

        }
        else
        {//恢复时间流速

            UnpauseGameServerRpc();
            //通知UI关闭暂停
            OnLocalUnPauseUIAction?.Invoke(this, EventArgs.Empty);
        }
            Debug.Log("游戏暂停");
    }
    [ServerRpc(RequireOwnership = false)]
    private void PauseGameServerRpc(ServerRpcParams serverRpcParams =  default)
    {
        //像服务器发起暂停指令 且要服务器知道是谁在调用暂停
        playerPauseDictionary[serverRpcParams.Receive.SenderClientId] = true;
        TestGamePauseState();
    }
    [ServerRpc(RequireOwnership = false)]
    private void UnpauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        //像服务器发起取消暂停指令 且要服务器知道是谁在调用暂停
        playerPauseDictionary[serverRpcParams.Receive.SenderClientId] = false;
        TestGamePauseState();

    }
    private void TestGamePauseState()
    {
        //NetworkManager.Singleton.ConnectedClientsIds获取已连接id列表
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (playerPauseDictionary.ContainsKey(clientId) && playerPauseDictionary[clientId])
            {
                //有任意玩家暂停设置为true
                isGamePause.Value = true;
                return;
             }
        }
        //没有玩家暂停 设置为false
        isGamePause.Value = false;
    }
}

using Mono.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class KitchenGameMultiplayer : NetworkBehaviour
{
    public static KitchenGameMultiplayer Instance { get; private set; }
    public static bool playerMultiplayer;   
    //最大玩家数量
    public const int MAX_PLAYER_AMOUNT = 5;
    //关联存储了所有对象SO的SO文件
    [SerializeField] private KitchenObjectsListSO kitchenObjectsListSO;
    //玩家颜色列表
    [SerializeField] private List<Color> playerColorList;
    //玩家角色列表
    [SerializeField] private List<Sprite> playerCharacterSpriteList;
    [SerializeField] public List<GameObject> playerCharacterObjectList;


    [SerializeField] private GameObject LoadingPanel;
    [SerializeField] private Slider progressSlider;
    //成功加入游戏和加入游戏失败的事件
    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    //列表发生变化的事件
    public event EventHandler OnPlayerDataNetworkListChanged;

    //玩家网络列表
    private NetworkList<PlayerData> playerDataNetworkList;

    //玩家名字
    private string playerName;
    private const string PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER = "PlayerNameMultiplayer";   
    private void Awake()
    {
        Instance = this;
        //持久化玩家名字
        playerName = PlayerPrefs.GetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, "Player" + UnityEngine.Random.Range(1000, 9999));
        //过场景不销毁
        DontDestroyOnLoad(gameObject);
        //网络列表初始化
        playerDataNetworkList = new NetworkList<PlayerData>();
        //列表发生变化时 触发事件
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    }
   
    private void Start()
    {
        if (!playerMultiplayer)
        {
            StartHost();
            
            StartCoroutine(FakeLoadingAndSwitch("GameScene"));
        }
    }
    //获得玩家名称
    public string GetPlayerName()
    {
        return playerName;
    }
    //更新玩家名称
    public void SetPlayerName(string playerName)
    {
        this.playerName = playerName;
        PlayerPrefs.SetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, playerName);
    }

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    //启动主机方法
    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectApprovalCallback;
        //连接上去时把玩家加入列表
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId)
    {
        for(int i = 0; i < playerDataNetworkList.Count; i++)
        {
            PlayerData playerData = playerDataNetworkList[i];
            if (playerData.clientId == clientId)
            {
                //找到断开连接的对象了
                playerDataNetworkList.RemoveAt(i);
            }
        }
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        playerDataNetworkList.Add(new PlayerData
        {
            clientId = clientId,
            colorId = GetFirstUnusedColorId(), 
            characterId = GetFirstUnusedCharacterId()

        });
        SetPlayerNameServerRpc(GetPlayerName());
        // 修复开始：处理 PlayerId 为 null 的情况
        string playerId = AuthenticationService.Instance.PlayerId;
        if (string.IsNullOrEmpty(playerId))
        {
            // 如果是单人模式或未登录，生成一个默认ID
            playerId = "LocalPlayer_" + clientId;
        }
        SetPlayerIdServerRpc(playerId);
    }

    public void StartClient()
    {
        //触发尝试加入的时间
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);
        //断开连接的回调 用于触发连接失败的情况
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
        //监听如果连接上向服务器发送名字
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Server_OnClientConnectedCallback;

        NetworkManager.Singleton.StartClient();
    }
     
    private void NetworkManager_Server_OnClientConnectedCallback(ulong clientId)
    {
        SetPlayerNameServerRpc(GetPlayerName());
        // --- 同样增加保护 ---
        string playerId = AuthenticationService.Instance.PlayerId;
        if (string.IsNullOrEmpty(playerId))
        {
            playerId = "Client_" + clientId;
        }
        SetPlayerIdServerRpc(playerId);
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string playerName,ServerRpcParams serverRpcParams = default)
    {
        //向服务器发送玩家名字
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.playerName = playerName;
        playerDataNetworkList[playerDataIndex] = playerData;
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerIdServerRpc(string playerId, ServerRpcParams serverRpcParams = default)
    {
        //向服务器发送玩家名字
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.playerId = playerId;
        playerDataNetworkList[playerDataIndex] = playerData;
    }
    private void NetworkManager_Client_OnClientDisconnectCallback(ulong clientId)
        {
            //触发断开连接的事件
            OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
        }

    private void NetworkManager_ConnectApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        //不是在角色选择场景就不允许连接
        if (SceneManager.GetActiveScene().name != "CharacterSelectScene")
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game Is Already Started";
            return;
        }
        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT)
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game Is Full";
            return;
        }
        connectionApprovalResponse.Approved = true;
    }
    public void SpawnKitchenObjects(KitchenObjectsSO kitchenObjectsSO, IKitchenObjectsParents kitchenObjectsParents)
    {
        SpawnKitchenObjectsServerRpc(GetKichenObjectsSOIndex(kitchenObjectsSO), kitchenObjectsParents.GetNetworkObject());
    }
    //Rpc不允许传递自定义对象
    [ServerRpc(RequireOwnership = false)]
    private void SpawnKitchenObjectsServerRpc(int kitchenObjectsSOIndex, NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        //根据索引得到SO
        KitchenObjectsSO kitchenObjectsSO = GetKitchenObjectsSO(kitchenObjectsSOIndex);
        //从网络对象重新取回kitchenOBjetct
        kitchenObjectParentNetworkObjectReference.TryGet(out NetworkObject kitchenObjectParentNetworkObject);
        IKitchenObjectsParents kitchenObjectsParents = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectsParents>();
        //如果已有父级对象直接返回防止报错
        if(kitchenObjectsParents.HasKitchenObject())
        {
            return;
        }
        //创建切片 或者 物体
        //创建食物对象设置未counterTopPoint的子对象
        Transform kitchenObjectsTransform = Instantiate(kitchenObjectsSO.prefab);

        //拿到他的网络对象
        NetworkObject kitchenObjectNetworkObject = kitchenObjectsTransform.GetComponent<NetworkObject>();
        //调用 Spawn() 方法将该物体“注册”到网络系统中，使其成为网络对象，并同步给所有客户端。
        kitchenObjectNetworkObject.Spawn(true);
        //强制让新生成的番茄相对于它的父物体（即 counterTopPoint）居中。
        //设置菜品父对象为当前柜台
        KitchenObjects kitchenObjects = kitchenObjectsTransform.GetComponent<KitchenObjects>();

        kitchenObjects.SetKitchenObjectsParents(kitchenObjectsParents);
    }
    //根据传入的厨房对象放回他在List中的索引
    public int GetKichenObjectsSOIndex(KitchenObjectsSO kitchenObjectsSO)
    {
        return kitchenObjectsListSO.kitchenObjectsSOList.IndexOf(kitchenObjectsSO);
    }
    //根据索引值获得厨房对象
    public KitchenObjectsSO GetKitchenObjectsSO(int kitchenObjectsSOIndex)
    {
        return kitchenObjectsListSO.kitchenObjectsSOList[kitchenObjectsSOIndex];
    }
    //销毁网络对象
    public void DestoryKitchenObject(KitchenObjects kitchenObjects)
    {
        DestoryKitchenObjectServerRpc(kitchenObjects.NetworkObject);
    }
    [ServerRpc(RequireOwnership = false)]
    private void DestoryKitchenObjectServerRpc(NetworkObjectReference kitchenObjectNetworkObjectReference)
    {

        kitchenObjectNetworkObjectReference.TryGet(out NetworkObject kitchenObjectNetworkObject);
        //防止延迟造成多次交互为空报错的情况
        if(kitchenObjectNetworkObject == null)
        {
            return;
        }
        KitchenObjects kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObjects>();
        ClearKitchenObjectOnParentClientRpc(kitchenObjectNetworkObjectReference);
        kitchenObject.DestorySelf();
    }
    //解除父子关系
    [ClientRpc]
    private void ClearKitchenObjectOnParentClientRpc(NetworkObjectReference kitchenObjectNetworkObjectReference)
    {
        kitchenObjectNetworkObjectReference.TryGet(out NetworkObject kitchenObjectNetworkObject);
        KitchenObjects kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObjects>();
        kitchenObject.ClearKitchenObjectOnParent();
    }
    public bool IsPlayerIndexConnected(int playerIndex)
    {
        //返回玩家的索引是否小于网络列表的计数
        return playerIndex < playerDataNetworkList.Count;
    }
    //根据玩家索引返回其在网络列表里的信息
    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        return playerDataNetworkList[playerIndex];
    }
    //根据id获取颜色
    public Color GetPlayerColor(int colorId)
    {
        return playerColorList[colorId];
    }
    //获取团子图标和团子对象
    public Sprite GetPlayerCharacterSprite(int characterId)
    {
        return playerCharacterSpriteList[characterId];
    }
    public GameObject GetPlayerCharacterObject(int characterId)
    {
        return playerCharacterObjectList[characterId];
    }
    //返回玩家信息
    public PlayerData GetPlayerData()
    {
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
    }
    //根据玩家id返回玩家信息
    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach(PlayerData playerData in playerDataNetworkList)
        {
            if(playerData.clientId == clientId)
            {
                return playerData;
            }
        }
        return default;
    }
    //根据玩家索引返回玩家id
    public int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        for(int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
            {
                return i;
            }
        }
        return -1;
       
    }
    //改变颜色 通过远程服务器过程调用实现同步颜色更改
    public void ChangePlayerColor(int colorId)
    {
        ChangePlayerColorServerRpc(colorId);  
    }
    [ServerRpc(RequireOwnership =false)]
    private void  ChangePlayerColorServerRpc(int  colorId, ServerRpcParams serverRpcParams = default)
    {
        //如果已有玩家使用 不允许更改颜色
        if (!IsColorAvailable(colorId))
        {
            return;
        }
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.colorId = colorId;
        playerDataNetworkList[playerDataIndex] = playerData;
    }
    private bool IsColorAvailable(int colorId)
    {
        foreach(PlayerData playerData in playerDataNetworkList)
        {
            if(playerData.colorId == colorId)
            {
                //已有玩家使用该颜色
                return false;
            }
        }
        return true;    
    }
    //更换角色
    public void ChangePlayerCharacter(int characterId)
    {
        ChangePlayerCharacterServerRpc(characterId);
    }
    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerCharacterServerRpc(int characterId, ServerRpcParams serverRpcParams = default)
    {
        //如果已有玩家使用 不允许更改颜色
        if (!IsCharacterAvailable(characterId))
        {
            return;
        }
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.characterId = characterId;
        playerDataNetworkList[playerDataIndex] = playerData;
    }
    private bool IsCharacterAvailable(int characterId)
    {
        foreach (PlayerData playerData in playerDataNetworkList)
        {
            if (playerData.characterId == characterId )
            {
                //已有玩家使用该团子
                return false;
            }
        }
        return true;
    }

    //用于分配未使用的颜色id
    private int GetFirstUnusedColorId()
    {
        for(int i = 0; i < playerColorList.Count; i++)
        {
            if (IsColorAvailable(i))
            {
                return i;
            }
        }
        return -1;
    }
    //用于分配未使用的团子id
    public int GetFirstUnusedCharacterId()
    {
        for (int i = 0; i < playerCharacterObjectList.Count; i++)
        {
            if (IsCharacterAvailable(i))
            {
                return i;
            }
        }
        return -1;
    }
    //踢人的方法
    public void KickPlayer(ulong clientId)
    {
        NetworkManager.Singleton.DisconnectClient(clientId);
        NetworkManager_Server_OnClientDisconnectCallback(clientId);
    }

    // 协程：先播放假进度条，再切换场景
    private IEnumerator FakeLoadingAndSwitch(string sceneName)
    {
        // 1. 显示加载界面
        LoadingPanel.SetActive(true);
        progressSlider.value = 0;

        // 2. 模拟加载进度 (耗时1秒)
        float duration = 1.0f;
        float timer = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            // 线性增加进度条
            progressSlider.value = timer / duration;

            yield return null;
        }

        // 3. 确保进度条走满
        progressSlider.value = 1;
        if (IsServer)
        {
            //确保只有服务端能够执行场景切换
            LoadNetworkScene(sceneName);

        }

        // 4. 稍微停顿一小下让视觉跟上，然后执行真正的加载
        // 注意：这里会阻塞主线程，但因为界面已经在显示“100%”，所以用户体验尚可
        yield return new WaitForSeconds(0.1f);
        if (IsServer)
        {
            //全部准备后删除大厅 开始进行场景跳转
            KitchenGameLobby.Instance.DeleteLobby();
        }

        // 5. 调用 NetworkManager 的同步加载
        // 这一步会导致画面卡住，直到场景加载完毕

    }

    public void LoadNetworkScene(string sceneName)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}


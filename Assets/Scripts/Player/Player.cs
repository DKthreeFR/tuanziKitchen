using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour, IKitchenObjectsParents
{
    //设为单例
    public static Player LocalInstance { get; set; }

    //设置为静态是为了让事件归属于玩家本身
    public static event EventHandler OnAnyPlayerSpawned;
    //拾取事件
    public static event EventHandler OnAnyPickedSomething;
    //清除事件
    new public static void ResetStaticData()
    {
        OnAnyPlayerSpawned = null;
        OnAnyPickedSomething = null;
    }
    

  
    //暂停事件
    public event EventHandler OnPauseAction;

    //记录绑定按键的字段
    private const string PLAYER_PREFS_BINDINGS = "PlayerPrefsBindings";


    [SerializeField] private LayerMask counterLayer;
    [SerializeField] private LayerMask collisonLayer;
    private Transform tr;
    //键盘输入方向
    private Vector2 playerMoveDir;
    // 移动 转身相关
    Vector3 move;
    private float moveSpeed = 10;
    float rotateSpeed = 50;
    private bool isWalking;
    //检测是否可移动 配合射线检测是否撞墙 以及射线距离
    bool canMove;
    float playerSize =  0.7f;
    float playerHeight = 2f;
    float playerRadius = 0.7f;
    public Vector3 lastInteractDir;

    //交互相关 监听事件
    public event EventHandler OnInteractAction;
    public event EventHandler OnInteractAlternateAction;
    //选中的柜子 并用事件来监听哪个柜子高亮
    private BaseCounter selectedCounter;
    public event EventHandler<OnSelectedCounterChangedEventArgs> onSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }
    //玩家拿起物品的音效监听事件
    public event EventHandler OnPickedSometh;
    // 手 和 手持的菜品
    [SerializeField] private Transform kitchenObjectsHoldPoint;
    private KitchenObjects kitchenObjects;

    //玩家不同的出生点
    [SerializeField] private List<Vector3> spawnPositionList;

    //玩家视觉对象
    [SerializeField] private PlayerVisual playerVisual;
    [SerializeField] private TuanZiVisual tuanZiVisual;


    private void Awake()
    {
 
        //Instance = this;
       

       
    }
    //网络对象中尽量避免使用Awake该为使用这个函数
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
        }
        //根据客户端索引进行选择出生位置
        transform.position = spawnPositionList[KitchenGameMultiplayer.Instance.GetPlayerDataIndexFromClientId(OwnerClientId)];
        //通知其他的需要使用LocalInstance的 玩家实例已创建
        OnAnyPlayerSpawned?.Invoke(this,EventArgs.Empty);
        if (IsServer)
        { 
            //断开连接时
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

        }
       
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        //如果断开链接时手上还有物品 就销毁
        if(clientId == OwnerClientId&&HasKitchenObject())
        {
            KitchenObjects.DestoryKitchenObject(GetKitchenObjects());
        }
    }

    //暂停事件
    private void Pause_performed(InputAction.CallbackContext obj)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void Interact_performed(InputAction.CallbackContext callbackContext)
    {
        if (OnInteractAction != null)
        {
            OnInteractAction?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Debug.Log("无");
        }
    
    }
    private void InteractAlternate_performed(InputAction.CallbackContext callbackContext)
    {
        if (OnInteractAction != null)
        {
            OnInteractAlternateAction?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Debug.Log("无");
        }

    }
    private void Start()
    {
        playerMoveDir = new Vector2(0, 0);
        tr = this.GetComponent<Transform>();
        // 订阅 GameInput 的事件
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        GameInput.Instance.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        //设置玩家颜色
        PlayerData playerData = KitchenGameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerVisual.SetPlayerColor(KitchenGameMultiplayer.Instance.GetPlayerColor(playerData.colorId));
        tuanZiVisual.SetPlayerCharacter(KitchenGameMultiplayer.Instance.GetPlayerCharacterObject(playerData.characterId));
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return; 
        }
        HandleMove();
        HandleInteractionsHighlight();//高亮选中物体

    }
    //清除订阅和释放输入 防止下次进入读取调用旧的面板造成空引用
    private void OnDestroy()
    {
        // 记得取消订阅
        GameInput.Instance.OnInteractAction -= GameInput_OnInteractAction;
        GameInput.Instance.OnInteractAlternateAction -= GameInput_OnInteractAlternateAction;
        GameInput.Instance.OnPauseAction -= GameInput_OnPauseAction;
    }
    // 修改事件处理方法名以示区分
    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        // 暂停逻辑
    }

    private void GameInput_OnInteractAlternateAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;
        if (selectedCounter != null)
        {
            selectedCounter.IntertactAternate(this);
        }
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;
        if (selectedCounter != null)
        {
            selectedCounter.Intertact(this);
        }
    }

    /// <summary>
    /// 角色移动
    /// </summary>
    /// <param name="context"></param>

    public bool IsWalking()
    {
        return isWalking;
    }
    //角色移动 逻辑
   

    public void HandleMove()
    {
        //移动输入 
        playerMoveDir = GameInput.Instance.GetMovementVectorNormalized();
        playerMoveDir = playerMoveDir.normalized;
        Vector3 moveDir = new Vector3(playerMoveDir.x, 0, playerMoveDir.y);

        isWalking = moveDir != Vector3.zero;
        //检测是否撞墙 撞墙时不许移动
        float moveDistance = moveSpeed * Time.deltaTime;

        canMove = !Physics.BoxCast(this.transform.position,  Vector3.one * playerRadius, moveDir, Quaternion.identity, moveDistance,collisonLayer);
        if (!canMove)
        {
            //不能走时先检测横向有没有障碍 没有就将移动方向切为横向 否则如果横向不能走那检测纵向
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = (moveDir.x<-.5f||moveDir.x>+.5f)&& !Physics.BoxCast(this.transform.position, Vector3.one * playerRadius, moveDir, Quaternion.identity, moveDistance, collisonLayer);
            if (canMove)
            {
                moveDir = moveDirX;
            }
            else
            {
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = (moveDir.z < -.5f || moveDir.z > +.5f) && !Physics.BoxCast(this.transform.position, Vector3.one * playerRadius, moveDirZ, Quaternion.identity, moveDistance, collisonLayer);
                if (canMove)
                {
                    moveDir = moveDirZ;
                }
            }
        }
        //因为上面逻辑会对moveDIr更新 所以move应该写在他们下面
        move = moveDir * Time.deltaTime * moveSpeed;
        if (canMove)
        {
            tr.Translate(move, Space.World);
        }
        //tr.forward = Vector3.Slerp(tr.forward, moveDir, Time.deltaTime * rotateSpeed);
        // 修正代码
        if (moveDir != Vector3.zero)
        {
            tr.forward = Vector3.Slerp(tr.forward, moveDir, Time.deltaTime * rotateSpeed);
        }
    }
    /// <summary>
    /// 与场景上的柜台交互
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    //对当前选中的柜子进行交互
    public void HandleInteractions(object sender, EventArgs e)
    {
        //不处于游戏状态禁止交互直接返回
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;
        //射线面向方向
        Vector2 inputVector = playerMoveDir.normalized;
        Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y).normalized;
        //记录最后面向位置 防止没输入时不对前方进行检测了
            lastInteractDir = tr.forward;

            //交互的射线检测距离
            float interactDistance = 2f;
       if(Physics.Raycast(transform.position, lastInteractDir,out RaycastHit raycastHit , interactDistance,counterLayer))
        {
            if(raycastHit.transform.TryGetComponent(out BaseCounter baseCounter))
            { 
           
                if (selectedCounter != null)
                {
                    selectedCounter.Intertact(this);
                }

            }
        }
       
    }
    //设置选中柜子 以及事件触发高亮
    public void SetSelectedCounter(BaseCounter selectedCounter)
    {
        this.selectedCounter = selectedCounter;
        onSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs
        {
            selectedCounter = selectedCounter
        });
    }
    //触发高亮的逻辑 放在update里以实现实时高亮前方物体
    public void HandleInteractionsHighlight()
    {
        //不处于游戏状态禁止交互直接返回
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;
        //交互的射线检测距离
        float interactDistance = 2f;
        if (Physics.Raycast(transform.position, tr.forward, out RaycastHit raycastHit, interactDistance, counterLayer))
        {
            if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter))
            {
                //将当前检测到的柜台设置为我们选中的柜台
                if (selectedCounter != baseCounter)
                {
                    // 如果当前选中的不是这个柜台，则更新选中（触发高亮）
                    SetSelectedCounter(baseCounter);
                }

            }
            else
            { // --- 击中了物体，但不是柜台 (比如墙) ---
              // 必须清空选中状态，触发 Hide()
                SetSelectedCounter(null);
            }
        }
        else
        {
            // --- 【关键修改】根本没击中任何物体 (前方是空气) ---
            SetSelectedCounter(null);
        }
    }
    //二次交互 比如用于切片
    public void HandleInterAlternateactions(object sender, EventArgs e)
    { 
        if (selectedCounter != null)
        {
            selectedCounter.IntertactAternate(this);
        }
    
    }

    //提供拿的节点（比如手）
    public Transform GetKitchenObjectFollowTransform()
    {
        return kitchenObjectsHoldPoint;
    }

    //玩家拿到物品
    public void SetKitchenObject(KitchenObjects kitchenObjects)
    {
        this.kitchenObjects = kitchenObjects;
        //如果玩家真的拿到物品 播放拿到物品的音效
        if (kitchenObjects != null)
        {
            OnPickedSometh?.Invoke(this, EventArgs.Empty);
            OnAnyPickedSomething?.Invoke(this, EventArgs.Empty); 
        }

}
    //得到玩家手上的物品
    public KitchenObjects GetKitchenObjects()
    {
       return kitchenObjects;
    }
    //清除玩家手上的物品
    public void ClearKitchenObject()
    {
        kitchenObjects = null;
    }

    //检测玩家是否有物品
    public bool HasKitchenObject()
    {
        return kitchenObjects != null;
    }
    //提供显示当前绑定的键位
    public enum Binding
    {
        Move_Up,
        Move_Down, 
        Move_Left,
        Move_Right,
        Interact,
        InteractAlternate,
        Pause,
        Gamepad_Interact,
        Gamepad_InteractAlternate,
        Gamepad_Pause,
    }

  
    //实现接口里的方法
    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }


}

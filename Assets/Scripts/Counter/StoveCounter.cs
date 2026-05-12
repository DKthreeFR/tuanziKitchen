using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using static CuttingCounter;

public class StoveCounter : BaseCounter,IHasProgress
{
    [SerializeField] private FireRecipeSO[] fireRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;
    [SerializeField] private StoveCounter stoveCounter;
    private FireRecipeSO fireRecipeSO;

    //传递进度 实现接口里的方法
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    //通知视觉脚本开启或关闭显示
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
    public class OnStateChangedEventArgs : EventArgs
    {
        public State state;
    }
    //状态
    public enum State
    {
        Idle,
        Frying,
        Fried,
        Burned,
    }
    private NetworkVariable<State> state=new NetworkVariable<State>(State.Idle) ;
    //利用float实现一个定时器 来模拟煮熟的过程
    //从生肉到煮熟的定时器
    private NetworkVariable<float> fryingTimer = new NetworkVariable<float>(0f);
    //从熟肉到烧焦的定时器
    private NetworkVariable<float> burningTimer = new NetworkVariable<float>(0f);
    private BurningRecipeSO burningRecipeSO;

    public override void OnNetworkSpawn()
    {
        fryingTimer.OnValueChanged += FryingTimer_OnValueChanged;
        burningTimer.OnValueChanged += BurningTimer_OnValueChanged;
        state.OnValueChanged += State_OnValueChanged;
    } 
    private void FryingTimer_OnValueChanged(float previousValue,float newValue)
    {
        //设置该变量避免空引用勤快出现 如果为空则该变量为1 避免为0导致计算出错
        float fryingTimerMax = fireRecipeSO != null ? fireRecipeSO.fireTimerMax : 1f;
        //通知进度
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = fryingTimer.Value / fryingTimerMax
        });
    }
    private void BurningTimer_OnValueChanged(float previousValue,float newValue)
    {
        //设置该变量避免空引用勤快出现 如果为空则该变量为1 避免为0导致计算出错
        float burningTimerMax = burningRecipeSO != null ? burningRecipeSO.burningTimerMax : 1f;
        //通知进度
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = burningTimer.Value / burningTimerMax
        });
    }
    private void State_OnValueChanged(State previousState,State newState)
    {
        //通知切换状态
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state.Value });
        //处于烧焦或闲置状态时隐藏进度条
        if (state.Value == State.Burned || state.Value == State.Idle)
        {
            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
            {
                progressNormalized = 0f
            });
        }
    }
    private void Update()
    {
        //如果不是服务器直接返回
        if (!IsServer)
        {
            return;
        }
        if (HasKitchenObject())
        {
            switch (state.Value)
        {
            case State.Idle:
                break;
            case State.Frying:
                fryingTimer.Value += Time.deltaTime;
                    //在交互中 获得输入输出食谱 传入输入食谱 获得到整个食谱 把食谱的输出食谱得到 时间移到 摧毁自己 重新生成煮熟了的面饼
                 
                    if (fryingTimer.Value > fireRecipeSO.fireTimerMax)
                {
                    
                    Debug.Log("煮熟了");
                        KitchenObjects.DestoryKitchenObject(GetKitchenObjects());
                    KitchenObjects.SpawnKitchenObjects(fireRecipeSO.output, this);
                     
                        state.Value = State.Fried;
                        burningTimer.Value = 0;
                        //得到由熟肉到烧焦肉的配方
                        SetburningRecipeSOClientRpc(KitchenGameMultiplayer.Instance.GetKichenObjectsSOIndex(GetKitchenObjects().GetKitchenObjectsSO()));
                        burningRecipeSO = GetBurningRecipeSOWithInput(GetKitchenObjects().GetKitchenObjectsSO());
      

                    }
                    break;
            case State.Fried:
                    burningTimer.Value += Time.deltaTime;
                    //在交互中 获得输入输出食谱 传入输入食谱 获得到整个食谱 把食谱的输出食谱得到 时间移到 摧毁自己 重新生成煮熟了的面饼

           
                    if (burningTimer.Value > burningRecipeSO.burningTimerMax)
                    {

                        Debug.Log("煮熟了");
                        KitchenObjects.DestoryKitchenObject(GetKitchenObjects());
                        KitchenObjects.SpawnKitchenObjects(burningRecipeSO.output, this);
                        state.Value = State.Burned;
                        
                    }
                    break;
            case State.Burned:
                    //通知进度 烧焦后归0不显示
                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        progressNormalized = 0
                    });
                    break;
        }
      
         
        }
    }
    public override void Intertact(Player player)
    {
        if (!HasKitchenObject())
        {
            //这个干净柜台上没有物体
            if (player.HasKitchenObject())
            {
                //玩家手上有物体
                //调用获取到玩家手上物体的方法
                if (HasRecipeWithInput(player.GetKitchenObjects().GetKitchenObjectsSO()))
                {
                    KitchenObjects kitchenObjects = player.GetKitchenObjects();
                    //如果玩家手上的物体是可切的 有这个配方才允许他放上去
                    kitchenObjects.SetKitchenObjectsParents(this);
                    InteractLogicPlaceObjectOnCounterServerRpc(KitchenGameMultiplayer.Instance.GetKichenObjectsSOIndex(kitchenObjects.GetKitchenObjectsSO()));
                 

                }


            }
            else
            {
                //玩家手上没物体 不做任何事情
            }
        }
        else
        {
            //这个柜子上有物体
            if (player.HasKitchenObject())
            {
                //这个柜子上有物体
                if (player.HasKitchenObject())
                {
                    //如果玩家拿的是盘子
                    if (player.GetKitchenObjects().TryGetPlate(out PlateKitchenObjects plateKitchenObjects))
                    {
                        //把当前柜子上的菜品放到盘子里
                        if (plateKitchenObjects.TryAddIngredient(GetKitchenObjects().GetKitchenObjectsSO()))
                        {
                            //如果成功添加再销毁
                          KitchenObjects.DestoryKitchenObject(GetKitchenObjects());
                            //不允许客户端远程修改状态 只允许客户端远程修改状态
                            SetStateIdleServerRpc();

                        }
                    }
                }

            }
            else
            {
                //玩家手上没物体
                stoveCounter.GetKitchenObjects().SetKitchenObjectsParents(player);
                //返回初始状态
                SetStateIdleServerRpc();
            }
        }

    }
    [ServerRpc(RequireOwnership = false)]
    private void SetStateIdleServerRpc()
    {
        //通知视觉脚本前先把状态变为待机
        state.Value = State.Idle;
    }
    //远程Rpc调用交互逻辑
    [ServerRpc(RequireOwnership =false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc(int kitchenObjectSOIndex)
    {
        fryingTimer.Value = 0;
        state.Value = State.Frying;
        SetFryingRecipeSOClientRpc(kitchenObjectSOIndex);
    }
    [ClientRpc]
    private void SetFryingRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectsSO kitchenObjectsSO = KitchenGameMultiplayer.Instance.GetKitchenObjectsSO(kitchenObjectSOIndex);
        fireRecipeSO = GetFireRecipeSOWithInput(kitchenObjectsSO);

    }
    [ClientRpc]
    private void SetburningRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectsSO kitchenObjectsSO = KitchenGameMultiplayer.Instance.GetKitchenObjectsSO(kitchenObjectSOIndex);
        burningRecipeSO = GetBurningRecipeSOWithInput(kitchenObjectsSO);

    }

    //获取烧后的对象
    private KitchenObjectsSO GetOutputForInput(KitchenObjectsSO inputKitchenObjectSO)
    {
        FireRecipeSO fireRecipeSO = GetFireRecipeSOWithInput(inputKitchenObjectSO);
        if (fireRecipeSO.input == inputKitchenObjectSO)
        {
            return fireRecipeSO.output;
        }
        return null;


    }
    //判断是否有这个输入的配方 判断是否可切
    private bool HasRecipeWithInput(KitchenObjectsSO inputKitchenObjectSO)
    {
        FireRecipeSO fireRecipeSO = GetFireRecipeSOWithInput(inputKitchenObjectSO);
        //防止不可烧的东西放到柜子上导致报错
        if (fireRecipeSO == null)
        {
            return false;
        }
        if (fireRecipeSO.input == inputKitchenObjectSO)
        {
            return true;
        }
        return false;
    }
    private FireRecipeSO GetFireRecipeSOWithInput(KitchenObjectsSO inputKitchenObjectSO)
    {
        foreach (FireRecipeSO fireRecipeSO in fireRecipeSOArray)
        {
            if (fireRecipeSO.input == inputKitchenObjectSO)
            {
                return fireRecipeSO;
            }
        }
        return null;
    }
    private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectsSO inputKitchenObjectSO)
    {
        foreach (BurningRecipeSO burningRecipeSO in burningRecipeSOArray)
        {
            if (burningRecipeSO.input == inputKitchenObjectSO)
            {
                return burningRecipeSO;
            }
        }
        return null;
    }
    //提供一个检测是否处于Fried状态
    public bool IsFried()
    {
        return state.Value == State.Fried;
    }

}

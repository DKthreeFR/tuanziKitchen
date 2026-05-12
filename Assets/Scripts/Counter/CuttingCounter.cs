using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class CuttingCounter : BaseCounter,IHasProgress
{
    [SerializeField] private CuttingCounter cuttingCounter;
    [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;

    //传递进度
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
 
    //调用动画的事件
    public event EventHandler Oncut;
    //播放音频
    public static event EventHandler OnAnyCut;
    
    //切割次数
    private int cuttingProgress;
    //重置静态监听
    new public static void ResetStaticData()
    {
        OnAnyCut = null;
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
                    //如果玩家手上的物体是可切的 有这个配方才允许他放上去
                    KitchenObjects kitchenObjects = player.GetKitchenObjects();
                    player.GetKitchenObjects().SetKitchenObjectsParents(this);
                    InteractLogicPlaceObjectOnCounterServerRpc();

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
                //如果玩家拿的是盘子
                if (player.GetKitchenObjects().TryGetPlate(out PlateKitchenObjects plateKitchenObjects))
                {
                    //把当前柜子上的菜品放到盘子里
                    if (plateKitchenObjects.TryAddIngredient(GetKitchenObjects().GetKitchenObjectsSO()))
                    {
                        //如果成功添加再销毁
                        KitchenObjects.DestoryKitchenObject(GetKitchenObjects());

                    }
                }
            }
            else
            {
                //玩家手上没物体
                cuttingCounter.GetKitchenObjects().SetKitchenObjectsParents(player);
            }
        }

    }
    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc()
    {
        InteractLogicPlaceObjectOnCounterClientRpc();
    }
    [ClientRpc]
    private void InteractLogicPlaceObjectOnCounterClientRpc()
    {
        //放上去时切割次数记为0
        cuttingProgress = 0;
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObjects().GetKitchenObjectsSO());
        //发送进度信息
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
        });
    }
    public override void IntertactAternate(Player player)
    {
        //当柜子上存在物品 且物品有对应的切割配方时才执行
        if (HasKitchenObject()&&HasRecipeWithInput(GetKitchenObjects().GetKitchenObjectsSO()))
        {

            CutObjetcServerRpc();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void CutObjetcServerRpc()
    {
        //当柜子上存在物品 且物品有对应的切割配方时才执行
        if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObjects().GetKitchenObjectsSO()))
        {
            CutObjetcClientRpc();
        TestCuttingProgressDoneServerRpc();
        }
    }
    [ClientRpc]
    private void CutObjetcClientRpc()
    {
        //每次点击切割交互就加1 
        cuttingProgress++;

        Oncut?.Invoke(this, EventArgs.Empty);
        OnAnyCut?.Invoke(this, EventArgs.Empty);

        //当切割次数大于菜品最大切割次数时才能切成菜
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObjects().GetKitchenObjectsSO());
        //发送进度信息
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
        });
     

    }

    [ServerRpc(RequireOwnership =false)]
    private void TestCuttingProgressDoneServerRpc()
    {
        //当切割次数大于菜品最大切割次数时才能切成菜
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObjects().GetKitchenObjectsSO());
        if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax)
        {
            //获得当前要切的对象身上的SO 以便我们后续 传入获取切后的对象
            KitchenObjectsSO outputKitchenObjectSO = GetOutputForInput(GetKitchenObjects().GetKitchenObjectsSO());
            KitchenObjects.DestoryKitchenObject(GetKitchenObjects());
            KitchenObjects.SpawnKitchenObjects(outputKitchenObjectSO, this);
        }
    }
    //获取切后的对象
    private KitchenObjectsSO GetOutputForInput(KitchenObjectsSO inputKitchenObjectSO)
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
        if (cuttingRecipeSO.input == inputKitchenObjectSO)
        {
            return cuttingRecipeSO.output;
        }
        return null;
      
       
    }
    //判断是否有这个输入的配方 判断是否可切
    private bool HasRecipeWithInput(KitchenObjectsSO inputKitchenObjectSO)
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
        //防止不可切的东西放到柜子上导致报错
        if (cuttingRecipeSO == null)
        {
            return false;
        }
        if (cuttingRecipeSO.input == inputKitchenObjectSO)
        {
            return true;
        }
        return false;
    }
    private  CuttingRecipeSO GetCuttingRecipeSOWithInput(KitchenObjectsSO inputKitchenObjectSO)
    {
        foreach (CuttingRecipeSO cuttingRecipeSO in cuttingRecipeSOArray)
        {
            if (cuttingRecipeSO.input == inputKitchenObjectSO)
            {
                return cuttingRecipeSO;
            }
        }
        return null;
    }
}

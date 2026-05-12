using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlateKitchenObjects : KitchenObjects
{
    //只允许对应类型的被加入盘子
    [SerializeField] private List<KitchenObjectsSO> validKitchenObjectSOList;

    //通知盘子上的汉堡来显示
    public event EventHandler<OnIngredientAddedEventArgs> OnIngredientAdded;
    public class OnIngredientAddedEventArgs : EventArgs
    {
        public KitchenObjectsSO kitchenObjectsSO;
    }

    private List<KitchenObjectsSO> kitchenObjectsSOList;

    protected override void Awake()
    {
        base.Awake();
        kitchenObjectsSOList = new List<KitchenObjectsSO>();
    }
    //把传入的菜加入列表 
    public bool TryAddIngredient(KitchenObjectsSO kitchenObjectsSO)
    {
        //如果不是能够放到盘子上的东西 不许加入盘子
        if (!validKitchenObjectSOList.Contains(kitchenObjectsSO))
        {
            return false;
        }
      
        if (kitchenObjectsSOList.Contains(kitchenObjectsSO))
        {
            return false;
        }
        else
        {
            AddIngredientServerRpc(KitchenGameMultiplayer.Instance.GetKichenObjectsSOIndex(kitchenObjectsSO));
            return true; 
        }

    }
    //使用服务端客户端远程Rpc调用  实现同步内容被添加到盘子上
    [ServerRpc(RequireOwnership = false)]
    private void AddIngredientServerRpc(int kitchenObjectSOIndex)
    {
        AddIngredientClientRpc(kitchenObjectSOIndex);
    }
    [ClientRpc]
    private void AddIngredientClientRpc(int kitchenObjectSOIndex) {
        KitchenObjectsSO kitchenObjectsSO = KitchenGameMultiplayer.Instance.GetKitchenObjectsSO(kitchenObjectSOIndex);
        kitchenObjectsSOList.Add(kitchenObjectsSO);
        //通知视觉脚本时哪个被加到盘子里了
        OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs
        {
            kitchenObjectsSO = kitchenObjectsSO
        });
    }
    //返回一个List给显示图标的脚本里使用 PlateIconsUI里来使用
    public List<KitchenObjectsSO> GetKitchenObjectsSOList()
    {
        return kitchenObjectsSOList;
    }
}

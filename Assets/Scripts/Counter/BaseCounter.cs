using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BaseCounter : NetworkBehaviour,IKitchenObjectsParents
{
    private KitchenObjects kitchenObjects;
    [SerializeField] private Transform counterTopPoint;

    //玩家放下物品
    public static event EventHandler OnAnyObjectPlacedHere;
    //重置静态监听
    public static void ResetStaticData()
    {
        OnAnyObjectPlacedHere = null; 
    }
    public virtual void Intertact(Player player)
 {
    Debug.LogError("BaseCounter里的Interact被调用了 请检查是否override了");
 }
    public virtual void IntertactAternate(Player player)
    {
        //Debug.LogError("BaseCounter里的InteractAlternate被调用了 请检查是否override了");
    }
    //提供一个返回counterTopPoint的方法以便我们呢后续移动食物
    public Transform GetKitchenObjectFollowTransform()
    {
        return counterTopPoint;
    }
    //柜台上设置物品相关
    public void SetKitchenObject(KitchenObjects kitchenObjects)
    {
        this.kitchenObjects = kitchenObjects;
        if(kitchenObjects != null)
        {
            OnAnyObjectPlacedHere?.Invoke(this, EventArgs.Empty);
        }
    }
    public KitchenObjects GetKitchenObjects()
    {
        return kitchenObjects;
    }
    public void ClearKitchenObject()
    {
        kitchenObjects = null;
    }
    public bool HasKitchenObject()
    {
        return kitchenObjects != null;
    }
    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}

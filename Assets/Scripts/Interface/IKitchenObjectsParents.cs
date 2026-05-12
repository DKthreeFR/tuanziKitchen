using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public interface IKitchenObjectsParents 
{
    //提供一个返回counterTopPoint的方法以便我们呢后续移动食物
    public Transform GetKitchenObjectFollowTransform();
    //柜台上设置物品相关
    public void SetKitchenObject(KitchenObjects kitchenObjects);

    public KitchenObjects GetKitchenObjects();

    public void ClearKitchenObject();

    public bool HasKitchenObject();

    public NetworkObject GetNetworkObject();
}

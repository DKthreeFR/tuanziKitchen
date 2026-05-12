using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 挂载到预制体身上 以供返回SO
/// </summary>
public class KitchenObjects : NetworkBehaviour
{
    //获取有关跟随的对象的脚本
    private FollowTransform followTransform;
    protected virtual void Awake()
    {
        followTransform = GetComponent<FollowTransform>();
    }
    [SerializeField] private KitchenObjectsSO kitchenObjectsSO;
    private IKitchenObjectsParents  kitchenObjectsParents;
    public KitchenObjectsSO GetKitchenObjectsSO()
    {
        return kitchenObjectsSO; 
    }
    //给物品知道他所在柜台
    //因为player和ClearCounter都继承了接口 所以二者都可以作为参数被传入进去
    public void SetKitchenObjectsParents(IKitchenObjectsParents  kitchenObjectParent)
    {
        //获取父级网络对象
        SetKitchenObjectParentServerRpc(kitchenObjectParent.GetNetworkObject());
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetKitchenObjectParentServerRpc(NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        SetKitchenObjectParentClientRpc(kitchenObjectParentNetworkObjectReference);
    }
    [ClientRpc]
    private void SetKitchenObjectParentClientRpc(NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        //从网络对象重新取回kitchenOBjetct
        kitchenObjectParentNetworkObjectReference.TryGet(out NetworkObject kitchenObjectParentNetworkObject);
        IKitchenObjectsParents kitchenObjectsParents = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectsParents>();
        //前菜品的父级为非空那么设置新父级时要清除父级 注意this 否则可能判定为传入的参数
        if (this.kitchenObjectsParents != null)
        {
            this.kitchenObjectsParents.ClearKitchenObject();
        }
        this.kitchenObjectsParents = kitchenObjectsParents;
        //菜品新移动上去的柜台如果已有菜品就阻止
        if (kitchenObjectsParents.HasKitchenObject())
        {
            Debug.LogError("当前对象已经存在菜品");
        }
        //让柜子知道他有这个菜品 （这里解决了如何让柜子知道他身上有什么菜品）

        kitchenObjectsParents.SetKitchenObject(this);
        //移动食材到现在他所在的对象
        followTransform.SetTargetTransfome(kitchenObjectsParents.GetKitchenObjectFollowTransform());
    }
    
    //获得物品所在柜台
    public IKitchenObjectsParents  GetKitchenObjectsParent() {
        return kitchenObjectsParents;
    }
    public void DestorySelf()
    {
       
        Destroy(gameObject);
    }
    public void ClearKitchenObjectOnParent()
    {
        //销毁前清空父级
        kitchenObjectsParents.ClearKitchenObject();
    }
    //用于判断当前物品是否是盘子
    public bool TryGetPlate(out PlateKitchenObjects plateKitchenObjects)
    {
        if(this is PlateKitchenObjects)
        {
            plateKitchenObjects = this as PlateKitchenObjects;
            return true;
        }
        else
        {
            plateKitchenObjects=null;
            return false;
        } 
    }
    public static void SpawnKitchenObjects(KitchenObjectsSO kitchenObjectsSO,IKitchenObjectsParents kitchenObjectsParents)
    {
        KitchenGameMultiplayer.Instance.SpawnKitchenObjects(kitchenObjectsSO, kitchenObjectsParents);
    }
    //销毁对象的函数 可以使用非静态 此处使用静态只是为了形式上和上面比较匹配
    public static void DestoryKitchenObject(KitchenObjects kitchenObjects)
    {
        KitchenGameMultiplayer.Instance.DestoryKitchenObject(kitchenObjects);
    }
}
    
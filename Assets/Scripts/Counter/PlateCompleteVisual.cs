using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateCompleteVisual : MonoBehaviour
{
    //该脚本控制盘子上汉堡各部分的显示隐藏 （模拟东西被一点一点装入盘子）
    //关联盘子预设体 

    [SerializeField] private PlateKitchenObjects plateKitchenObjects;
    //建立SO文件与汉堡上的各部分的关系
    [Serializable]
    public struct KitchenObjectSO_GameObject
    {
        public KitchenObjectsSO kitchenObjectsSO;
        public GameObject gameObject;
    }
    [SerializeField] List<KitchenObjectSO_GameObject> kitchenObjectSOGameObjectsList;
  
    public void Start()
    {
        plateKitchenObjects.OnIngredientAdded += PlateKitchenObjects_OnIngredientAdded;
        foreach (KitchenObjectSO_GameObject kitchenObjectSOGameObject in kitchenObjectSOGameObjectsList)
        {
            kitchenObjectSOGameObject.gameObject.SetActive(false);
        }
        }

    private void PlateKitchenObjects_OnIngredientAdded(object sender, PlateKitchenObjects.OnIngredientAddedEventArgs e)
    {
        //遍历检测 如果放入盘子的相匹配 对应部分就会显示
      foreach(KitchenObjectSO_GameObject kitchenObjectSOGameObject in kitchenObjectSOGameObjectsList)
        {
            if(kitchenObjectSOGameObject.kitchenObjectsSO == e.kitchenObjectsSO)
            {
                kitchenObjectSOGameObject.gameObject.SetActive(true);
            }
        }
    }
 
}


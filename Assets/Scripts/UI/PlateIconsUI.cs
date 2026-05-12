using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateIconsUI : MonoBehaviour
{
    [SerializeField] private PlateKitchenObjects plateKitchenObjects;
    [SerializeField] private Transform iconTemplate;
    private void Awake()
    {
        iconTemplate.gameObject.SetActive(false);
    }
    private void Start()
    {
        plateKitchenObjects.OnIngredientAdded += PlateKitchenObjects_OnIngredientAdded;
    }

    //往盘子内添加食物时会触发该方法
    private void PlateKitchenObjects_OnIngredientAdded(object sender, PlateKitchenObjects.OnIngredientAddedEventArgs e)
    {
        UpdateVisual();
    }
    private void UpdateVisual()
    { 
       //每次更新前先删除所有图标再依次建立所有图标 放置重复生成
       foreach(Transform child in transform)
        {
            if (child == iconTemplate) continue;
            Destroy(child.gameObject);
        }
        foreach(KitchenObjectsSO kitchenObjectsSO in plateKitchenObjects.GetKitchenObjectsSOList())
        {
            Transform iconTransform = Instantiate(iconTemplate, transform);
            iconTransform.gameObject.SetActive(true);
            //图标设置为对应的图标
            iconTransform.GetComponent<PlateIconsSingleUI>().SetKitchenOBjectSO(kitchenObjectsSO);
        }
    }

}

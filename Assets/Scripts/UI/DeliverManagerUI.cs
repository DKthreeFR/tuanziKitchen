using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliverManagerUI : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private Transform recipeTemplate;

    private void Awake()
    {
        recipeTemplate.gameObject.SetActive(false);

    }
    public void Start()
    {
        //添加监听
        DeliverManager.Instance.OnRecipeSpawned += DeliverManager_OnRecipeSpawned;
        DeliverManager.Instance.OnRecipeCompleted += DeliverManager_OnRecipeCompleted;
        UpdateVisual();
    }
    //订单加入List后更新UI
    private void DeliverManager_OnRecipeSpawned(object sender, System.EventArgs e)
    {
        UpdateVisual();
    }
    //订单成功交付后更新UI
    private void DeliverManager_OnRecipeCompleted(object sender, System.EventArgs e)
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        foreach(Transform child in container)
        {
            if (child == recipeTemplate)
            {
                continue;
            }
            else
            { 
                Destroy(child.gameObject);
            }

           
        }
        foreach(RecipeSO recipeSO  in DeliverManager.Instance.GetWaitingRecipeSOList())
        {
            //将等待列表里的东西创建在UI上

            Transform recipeTransform = Instantiate(recipeTemplate, container);
            recipeTransform.gameObject.SetActive(true);
            //将生成的UI格子设置为等待中菜品的名字
            recipeTransform.GetComponent<DeliverManagerSingleUI>().SetRecipeSO(recipeSO);

           
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestingNetcodeUI : MonoBehaviour
{
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;

    private void Awake()
    {
        hostBtn.onClick.AddListener(() =>
        {
            Debug.Log("以主机Host启动");
            //开启主机
            KitchenGameMultiplayer.Instance.StartHost();
            Hide();
        });
        clientBtn.onClick.AddListener(() =>
        {
            Debug.Log("以客户端client启动");
            //开启客户端
            KitchenGameMultiplayer.Instance.StartClient();

            Hide();
        });
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}

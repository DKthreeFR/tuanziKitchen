using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour
{
    [SerializeField]private Button closeButton;
    [SerializeField] private Button createPublicBtn;
    [SerializeField] private Button createPrivateBtn;
    [SerializeField] private TMP_InputField lobbyNameInputField;

    

    private void Awake()
    {
        lobbyNameInputField.text = "Lobby " + Random.Range(1000, 9999);
        closeButton.onClick.AddListener(() =>
        {
            Hide();
        });
        createPublicBtn.onClick.AddListener(() =>
        {
           //传入大厅名字和是否私密
            KitchenGameLobby.Instance.CreateLobby(lobbyNameInputField.text, false);
        });
        createPrivateBtn.onClick.AddListener(() =>
        {
            //传入大厅名字和是否私密
            KitchenGameLobby.Instance.CreateLobby(lobbyNameInputField.text, true);
        });

    }
    private void Start()
    {
        //初始设置为隐藏
        Hide();
    }
    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}

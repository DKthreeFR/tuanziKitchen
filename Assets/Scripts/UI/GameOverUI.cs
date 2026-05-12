using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI DeliveredCountText;
    [SerializeField] private Button playAgainBtn;

    private void Awake()
    {
        playAgainBtn.onClick.AddListener(() =>
        {
            //关闭网络管理器
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("MainMenuScene");
        });
    }
    private void Start()
    {
        KitchenGameManager.Instance.OnStateChange += KitchenGameManager_OnStateChange;
        gameObject.SetActive(false);
    }

    private void KitchenGameManager_OnStateChange(object sender, System.EventArgs e)
    {
        if (KitchenGameManager.Instance.IsGameOverActive())
        {
            //修改显示的数字
            DeliveredCountText.text = DeliverManager.Instance.GetSuccessfulRecipesAmout().ToString();
            Show();
        }
        else
        {
            Hide();
        }
    }
    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}

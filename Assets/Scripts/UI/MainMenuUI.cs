using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playBtn;
    [SerializeField] private Button singlePlayerBtn;
    [SerializeField] private Button quitBtn;

    [SerializeField] private GameObject LoadingPanel;
    [SerializeField] private Slider progressSlider;

    private void Awake()
    {
        playBtn.onClick.AddListener(() =>
        {
            KitchenGameMultiplayer.playerMultiplayer = true;
            LoadSceneAsync("LobbyScene");
        });
        singlePlayerBtn.onClick.AddListener(() =>
        {
            KitchenGameMultiplayer.playerMultiplayer = false;
            SceneManager.LoadScene("LobbyScene");
        });
        quitBtn.onClick.AddListener(() => {
        
        Application.Quit(); 
        });
        //游戏内暂停时间后退出到主界面让游戏流速变为正常
        //防止游戏内卡住
        Time.timeScale = 1f;
        //选中允许导航
        playBtn.Select();
    }
    //异步加载场景
    public void LoadSceneAsync(string sceneName)
    {
        LoadingPanel.SetActive(true);
        StartCoroutine(IELoadSceneAsync(sceneName));
    }
    IEnumerator IELoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        while (!operation.isDone)
        {
            progressSlider.value = operation.progress / 0.9f;
            yield return null;
        }
    }
}

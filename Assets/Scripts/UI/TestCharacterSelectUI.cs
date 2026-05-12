using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestCharacterSelectUI : MonoBehaviour
{
    [SerializeField] private Button readyBtn;
    
    private void Awake()
    {
        //按下确定准备时玩家进行准备
        readyBtn.onClick.AddListener(() =>
        {
            CharacterSelectReady.Instance.SetPlayerReady();
        });
    }

}

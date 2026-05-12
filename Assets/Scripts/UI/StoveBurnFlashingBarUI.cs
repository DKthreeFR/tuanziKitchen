using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StoveBurnFlashingBarUI : MonoBehaviour
{
    private const string IS_FLASHING = "IsFlashing";
    //关联炉灶柜台
    [SerializeField] private StoveCounter stoveCounter;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        //最初默认情况下设置为false
        animator.SetBool(IS_FLASHING, false);
    }
    private void Start()
    {
        stoveCounter.OnProgressChanged += StoveCounter_OnProgressChanged;
    }

    private void StoveCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        float burnShowProgressAmout = .5f;
        //当炉灶进度超过50%且处于煎炸状态时 我们就要显示警告
        bool show = stoveCounter.IsFried() && e.progressNormalized >= burnShowProgressAmout;
        animator.SetBool(IS_FLASHING, show);
  
    }
  
}

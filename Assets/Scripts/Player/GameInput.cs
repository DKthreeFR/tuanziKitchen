using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class GameInput : MonoBehaviour
{
    // 单例模式
    public static GameInput Instance { get; private set; }
    // 输入配置C#类
    private PlayerInputAction playerInputAction;
    // 记录绑定按键的字段
    private const string PLAYER_PREFS_BINDINGS = "PlayerPrefsBindings";
    // 定义事件，供 Player 和其他脚本订阅
    public event EventHandler OnInteractAction;
    public event EventHandler OnInteractAlternateAction;
    //暂停事件
    public event EventHandler OnPauseAction;
    private void Awake()
    {
        // 单例初始化
        if (Instance != null)
        {
            Debug.LogError("There is more than one GameInput instance!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // 初始化输入
        playerInputAction = new PlayerInputAction();
        // 读取按键设置
        if (PlayerPrefs.HasKey(PLAYER_PREFS_BINDINGS))
        {
            playerInputAction.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PLAYER_PREFS_BINDINGS));
        }
        // 启用输入并绑定事件
        playerInputAction.PlayerMove.Enable();
        playerInputAction.PlayerMove.Interact.performed += Interact_performed;
        playerInputAction.PlayerMove.InteractAlternate.performed += InteractAlternate_performed;
        playerInputAction.PlayerMove.Pause.performed += Pause_performed;
    }
    private void OnDestroy()
    {
        playerInputAction.PlayerMove.Interact.performed -= Interact_performed;
        playerInputAction.PlayerMove.InteractAlternate.performed -= InteractAlternate_performed;
        playerInputAction.PlayerMove.Pause.performed -= Pause_performed;
        playerInputAction.Dispose();
    }
    // 事件触发回调
    private void Pause_performed(InputAction.CallbackContext obj)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }
    private void Interact_performed(InputAction.CallbackContext obj)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }
    private void InteractAlternate_performed(InputAction.CallbackContext obj)
    {
        OnInteractAlternateAction?.Invoke(this, EventArgs.Empty);
    }
    /// <summary>
    /// 提供给外部获取移动向量的方法
    /// </summary>
    public Vector2 GetMovementVectorNormalized()
    {
        Vector2 inputVector = playerInputAction.PlayerMove.Move.ReadValue<Vector2>();
        return inputVector.normalized;
    }
    // --- 以下为按键重绑定相关逻辑 ---
    public enum Binding
    {
        Move_Up,
        Move_Down,
        Move_Left,
        Move_Right,
        Interact,
        InteractAlternate,
        Pause,
        Gamepad_Interact,
        Gamepad_InteractAlternate,
        Gamepad_Pause,
    }
    public string GetBindingText(Binding binding)
    {
        switch (binding)
        {
            default:
            case Binding.Move_Up:
                return playerInputAction.PlayerMove.Move.bindings[1].ToDisplayString();
            case Binding.Move_Down:
                return playerInputAction.PlayerMove.Move.bindings[2].ToDisplayString();
            case Binding.Move_Left:
                return playerInputAction.PlayerMove.Move.bindings[3].ToDisplayString();
            case Binding.Move_Right:
                return playerInputAction.PlayerMove.Move.bindings[4].ToDisplayString();
            case Binding.Interact:
                return playerInputAction.PlayerMove.Interact.bindings[0].ToDisplayString();
            case Binding.InteractAlternate:
                return playerInputAction.PlayerMove.InteractAlternate.bindings[0].ToDisplayString();
            case Binding.Pause:
                return playerInputAction.PlayerMove.Pause.bindings[0].ToDisplayString();
            case Binding.Gamepad_Interact:
                return playerInputAction.PlayerMove.Interact.bindings[1].ToDisplayString();
            case Binding.Gamepad_InteractAlternate:
                return playerInputAction.PlayerMove.InteractAlternate.bindings[1].ToDisplayString();
            case Binding.Gamepad_Pause:
                return playerInputAction.PlayerMove.Pause.bindings[1].ToDisplayString();
        }
    }
    //提供按键重映射的方法
    public void Rebinding(Binding binding, Action onActionRebound)
    {
        playerInputAction.PlayerMove.Disable();
        InputAction inputAction;
        int bindIndex;
        switch (binding)
        {
            default:
            case Binding.Move_Up:
                inputAction = playerInputAction.PlayerMove.Move;
                bindIndex = 1;
                break;
            case Binding.Move_Down:
                inputAction = playerInputAction.PlayerMove.Move;
                bindIndex = 2;
                break;
            case Binding.Move_Left:
                inputAction = playerInputAction.PlayerMove.Move;
                bindIndex = 3;
                break;
            case Binding.Move_Right:
                inputAction = playerInputAction.PlayerMove.Move;
                bindIndex = 4;
                break;
            case Binding.Interact:
                inputAction = playerInputAction.PlayerMove.Interact;
                bindIndex = 0;
                break;
            case Binding.InteractAlternate:
                inputAction = playerInputAction.PlayerMove.InteractAlternate;
                bindIndex = 0;
                break;
            case Binding.Pause:
                inputAction = playerInputAction.PlayerMove.Pause;
                bindIndex = 0;
                break;
            case Binding.Gamepad_Interact:
                inputAction = playerInputAction.PlayerMove.Interact;
                bindIndex = 1;
                break;
            case Binding.Gamepad_InteractAlternate:
                inputAction = playerInputAction.PlayerMove.InteractAlternate;
                bindIndex = 1;
                break;
            case Binding.Gamepad_Pause:
                inputAction = playerInputAction.PlayerMove.Pause;
                bindIndex = 1;
                break;
        }
        inputAction.PerformInteractiveRebinding(bindIndex)
            .OnComplete(callback =>
            {
                callback.Dispose();
                playerInputAction.PlayerMove.Enable();
                onActionRebound();
                // 保存设置
                PlayerPrefs.SetString(PLAYER_PREFS_BINDINGS, playerInputAction.SaveBindingOverridesAsJson());
                PlayerPrefs.Save();
            })
            .Start();
    }
}
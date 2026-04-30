using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    private const string PLAYER_PREFS_BINDINGS = "InputBindings";

    private InputSystem_Actions inputAction;

    public event Action OnInteractAction;
    public event Action OnPauseAction;
    public event Action OnBindingRebinding;
    public event Action<Vector2> OnClickAction;
    public event Action OnDataWipeAction;

    public enum Binding
    {
        Interact,
        Pause,
        GamepadInteract,
        GamepadPause
    }
    public static GameInput Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        inputAction = new InputSystem_Actions();

        if (PlayerPrefs.HasKey(PLAYER_PREFS_BINDINGS))
        {
            inputAction.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PLAYER_PREFS_BINDINGS));
        }

        inputAction.Player.Enable();

        inputAction.Player.Interact.performed += Interact_performed;
        inputAction.Player.Pause.performed += Pause_performed;
        inputAction.Player.Click.performed += Click_performed;
        inputAction.Player.WipeData.performed += WipeData_performed;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (inputAction == null) return;
        inputAction.Player.Interact.performed -= Interact_performed;
        inputAction.Player.Pause.performed -= Pause_performed;
        inputAction.Player.Click.performed -= Click_performed;
        inputAction.Player.WipeData.performed -= WipeData_performed;
        inputAction.Player.Disable();
        inputAction.Dispose();
    }

    private (InputAction action, int bindingIndex) GetInputActionForBinding(Binding binding)
    {
        switch (binding)
        {
            default:
            case Binding.Interact: return (inputAction.Player.Interact, 0);
            case Binding.Pause: return (inputAction.Player.Pause, 0);
            case Binding.GamepadInteract: return (inputAction.Player.Interact, 1);
            case Binding.GamepadPause: return (inputAction.Player.Pause, 1);
        }
    }

    public string GetBindingText(Binding binding)
    {
        var (action, bindingIndex) = GetInputActionForBinding(binding);
        return action.bindings[bindingIndex].ToDisplayString();
    }

    public void RebindBinding(Binding binding, Action OnComplete = null)
    {
        inputAction.Player.Disable();

        var (action, bindingIndex) = GetInputActionForBinding(binding);

        action.PerformInteractiveRebinding(bindingIndex)
            .OnComplete(callback =>
            {
                callback.Dispose();
                inputAction.Player.Enable();
                OnComplete?.Invoke();

                PlayerPrefs.SetString(PLAYER_PREFS_BINDINGS, inputAction.SaveBindingOverridesAsJson());
                PlayerPrefs.Save();
                OnBindingRebinding?.Invoke();
            })
            .OnCancel(callback =>
            {
                callback.Dispose();
                inputAction.Player.Enable();
            })
            .Start();
    }

    public Vector3 GetMousePosition()
    {
        return GetPointerPosition();
    }

    private void Click_performed(InputAction.CallbackContext obj)
    {
        Vector2 pos;

        var touch = Touchscreen.current;
        if (touch != null)
            pos = touch.primaryTouch.position.ReadValue();
        else
            pos = Mouse.current.position.ReadValue();

        OnClickAction?.Invoke(pos);
    }

    private Vector2 GetPointerPosition()
    {
        return inputAction.Player.PointerPosition.ReadValue<Vector2>();
    }

    private void Pause_performed(InputAction.CallbackContext obj)
    {
        OnPauseAction?.Invoke();
    }

    private void WipeData_performed(InputAction.CallbackContext obj)
    {
        OnDataWipeAction?.Invoke();
    }

    private void Interact_performed(InputAction.CallbackContext obj)
    {
        OnInteractAction?.Invoke();
    }
}
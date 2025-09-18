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

    public enum Binding
    {
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        Interact,
        InteractAlt,
        Pause,

        GamepadInteract,
        GamepadInteractAlt,
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
    }

    private void Click_performed(InputAction.CallbackContext obj)
    {
        Vector2 pointerPosition = GetPointerPosition();
        OnClickAction?.Invoke(pointerPosition);
    }

    private Vector2 GetPointerPosition()
    {
        return inputAction.Player.PointerPosition.ReadValue<Vector2>();
    }

    private void Pause_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnPauseAction?.Invoke();
    }


    private void Interact_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnInteractAction?.Invoke();
    }

    private void OnDestroy()
    {
        OnInteractAction = null;
        OnPauseAction = null;

        if (inputAction != null)
        {
            inputAction.Player.Interact.performed -= Interact_performed;
            inputAction.Player.Pause.performed -= Pause_performed;

            inputAction.Player.Disable();

            inputAction.Dispose();
        }
    }

    public string GetBindingText(Binding binding)
    {
        switch (binding)
        {
            default:

            case Binding.Interact:
                return inputAction.Player.Interact.bindings[0].ToDisplayString();

            case Binding.Pause:
                return inputAction.Player.Pause.bindings[0].ToDisplayString();

            case Binding.GamepadInteract:
                return inputAction.Player.Interact.bindings[1].ToDisplayString();

            case Binding.GamepadPause:
                return inputAction.Player.Pause.bindings[1].ToDisplayString();



        }
    }

    public void RebindBinding(Binding binding, Action OnComplete = null)
    {
        inputAction.Player.Disable();

        InputActionRebindingExtensions.RebindingOperation rebindingOperation = null;

        switch (binding)
        {
            default:
   
            case Binding.Interact:
                rebindingOperation = inputAction.Player.Interact.PerformInteractiveRebinding(0);
                break;
            case Binding.Pause:
                rebindingOperation = inputAction.Player.Pause.PerformInteractiveRebinding(0);
                break;

            case Binding.GamepadInteract:
                rebindingOperation = inputAction.Player.Interact.PerformInteractiveRebinding(1);
                break;

            case Binding.GamepadPause:
                rebindingOperation = inputAction.Player.Pause.PerformInteractiveRebinding(1);
                break;
        }

        rebindingOperation?
            .OnComplete((callback) =>
            {
                inputAction.Player.Enable();
                OnComplete?.Invoke();
                callback.Dispose();

                PlayerPrefs.SetString(PLAYER_PREFS_BINDINGS, inputAction.SaveBindingOverridesAsJson());
                PlayerPrefs.Save();
                OnBindingRebinding?.Invoke();
            })
            .OnCancel((callback) =>
            {
                inputAction.Player.Enable();
                callback.Dispose();
            })
            .Start();
    }

}
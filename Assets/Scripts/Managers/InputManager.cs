using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-10)]
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public Vector2 ScreenPosition { get; private set; }
    public int PressedHotkey { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Update()
    {
        PressedHotkey = 0;

        var pointer = UnityEngine.InputSystem.Mouse.current;
        if (pointer != null)
            ScreenPosition = pointer.position.ReadValue();

        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.digit1Key.wasPressedThisFrame)
            PressedHotkey = 1;
        else if (keyboard.digit2Key.wasPressedThisFrame)
            PressedHotkey = 2;
        else if (keyboard.digit3Key.wasPressedThisFrame)
            PressedHotkey = 3;
        else if (keyboard.digit4Key.wasPressedThisFrame)
            PressedHotkey = 4;
    }
}

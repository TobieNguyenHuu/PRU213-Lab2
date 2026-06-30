using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public static class SnowboardInput
{
    public static float Horizontal()
    {
        float value = 0f;

#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                value -= 1f;
            }

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                value += 1f;
            }

            return Mathf.Clamp(value, -1f, 1f);
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        value = Input.GetAxisRaw("Horizontal");
#endif

        return Mathf.Clamp(value, -1f, 1f);
    }

    public static float Vertical()
    {
        float value = 0f;

#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                value -= 1f;
            }

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                value += 1f;
            }

            return Mathf.Clamp(value, -1f, 1f);
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        value = Input.GetAxisRaw("Vertical");
#endif

        return Mathf.Clamp(value, -1f, 1f);
    }

    public static bool BoostHeld()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            return keyboard.spaceKey.isPressed;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKey(KeyCode.Space);
#else
        return false;
#endif
    }

    public static bool BoostPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            return keyboard.spaceKey.wasPressedThisFrame;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.Space);
#else
        return false;
#endif
    }

    public static bool BrakeHeld()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            return keyboard.sKey.isPressed
                || keyboard.downArrowKey.isPressed
                || keyboard.leftShiftKey.isPressed
                || keyboard.rightShiftKey.isPressed;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKey(KeyCode.S)
            || Input.GetKey(KeyCode.DownArrow)
            || Input.GetKey(KeyCode.LeftShift)
            || Input.GetKey(KeyCode.RightShift);
#else
        return false;
#endif
    }

    public static bool PausePressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            return keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);
#else
        return false;
#endif
    }

    public static bool RestartPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            return keyboard.rKey.wasPressedThisFrame;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.R);
#else
        return false;
#endif
    }
}

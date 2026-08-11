using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace GameEngine3D;

public class Keybinds
{
    private Dictionary<InputAction, Keys> keybindDict = new Dictionary<InputAction, Keys>();
    private Dictionary<InputAction, MouseButton> mousebindDict = new Dictionary<InputAction, MouseButton>();

    private bool[] actionArr = new bool[(int)InputAction.ACTION_COUNT];
    private bool[] prevActionArr = new bool[(int)InputAction.ACTION_COUNT];
    private bool[] actionClickArr = new bool[(int)InputAction.ACTION_COUNT];

    public Keybinds()
    {
        Array.Fill(actionArr, false);
        Array.Fill(prevActionArr, false);
        Array.Fill(actionClickArr, false);
    }

    public void Update()
    {
        prevActionArr = actionArr.ToArray();
        Array.Fill(actionArr, false);
        Array.Fill(actionClickArr, false);
        
        KeyboardState keyState = Keyboard.GetState();
        MouseState mouseState = Mouse.GetState();

        // keyboard
        foreach (Keys key in keyState.GetPressedKeys())
        {
            InputAction[] actionKey = GetActionsOfKeybind(key);

            foreach(InputAction action in actionKey)
            {
                actionArr[(int)action] = true;
            }
        }

        // mouse
        if (mouseState.LeftButton == ButtonState.Pressed && mousebindDict.ContainsValue(MouseButton.LEFT_BUTTON))
        {
            InputAction[] actionMouse = GetActionsOfMousebind(MouseButton.LEFT_BUTTON);

            foreach(InputAction action in actionMouse)
            {
                actionArr[(int)action] = true;
            }
        }

        if (mouseState.RightButton == ButtonState.Pressed && mousebindDict.ContainsValue(MouseButton.RIGHT_BUTTON))
        {
            InputAction[] actionMouse = GetActionsOfMousebind(MouseButton.RIGHT_BUTTON);

            foreach(InputAction action in actionMouse)
            {
                actionArr[(int)action] = true;
            }
        }

        if (mouseState.MiddleButton == ButtonState.Pressed && mousebindDict.ContainsValue(MouseButton.MIDDLE_BUTTON))
        {
            InputAction[] actionMouse = GetActionsOfMousebind(MouseButton.MIDDLE_BUTTON);

            foreach(InputAction action in actionMouse)
            {
                actionArr[(int)action] = true;
            }
        }

        // ...

        UpdateClick();
    }

    private void UpdateClick()
    {
        for (int i = 0; i < (int)InputAction.ACTION_COUNT; i++)
        {
            if (i == (int)InputAction.NONE) continue;

            if (actionArr[i] && !prevActionArr[i])
            {
                actionClickArr[i] = true;
            }
        }
    }

    public void AddKeybind(InputAction action, Keys key)
    {
        keybindDict[action] = key;
    }

    public void RemoveKeybind(InputAction action)
    {
        keybindDict.Remove(action);
    }

    public void AddMousebind(InputAction action, MouseButton button)
    {
        mousebindDict[action] = button;
    }

    public void RemoveMousebind(InputAction action)
    {
        mousebindDict.Remove(action);
    }

    public Keys? GetKeybindOfAction(InputAction action)
    {
        foreach (var item in keybindDict)
        {
            if (item.Key == action)
            {
                return item.Value;
            }
        }

        return null;
    }

    public InputAction[] GetActionsOfKeybind(Keys key)
    {
        InputAction[] outArr = new InputAction[(int)InputAction.ACTION_COUNT];

        int i = 0;
        foreach(var item in keybindDict)
        {
            if (item.Value == key)
            {
                outArr[i] = item.Key;
                i++;
            }
        }

        return outArr;
    }

    public MouseButton? GetMousebindOfAction(InputAction action)
    {
        foreach (var item in mousebindDict)
        {
            if (item.Key == action)
            {
                return item.Value;
            }
        }

        return null;
    }

    public InputAction[] GetActionsOfMousebind(MouseButton button)
    {
        InputAction[] outArr = new InputAction[(int)InputAction.ACTION_COUNT];

        int i = 0;
        foreach(var item in mousebindDict)
        {
            if (item.Value == button)
            {
                outArr[i] = item.Key;
                i++;
            }
        }

        return outArr;
    }

    public bool IsActionPressed(InputAction action)
    {
        return actionArr[(int)action];
    }
    
    public bool IsActionClicked(InputAction action)
    {
        return actionClickArr[(int)action];
    }
}
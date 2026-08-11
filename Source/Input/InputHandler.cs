using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameEngine3D;

public static class InputHandler
{
    public static Keybinds keybinds;

    public static void Initialize()
    {
        keybinds = new Keybinds();

        keybinds.AddKeybind(InputAction.FORWARD, Keys.W);
        keybinds.AddKeybind(InputAction.LEFT, Keys.A);
        keybinds.AddKeybind(InputAction.BACKWARD, Keys.S);
        keybinds.AddKeybind(InputAction.RIGHT, Keys.D);
        keybinds.AddKeybind(InputAction.JUMP, Keys.Space);
        
        keybinds.AddMousebind(InputAction.BREAK, MouseButton.LEFT_BUTTON);
        keybinds.AddMousebind(InputAction.PLACE, MouseButton.RIGHT_BUTTON);
        
        keybinds.AddKeybind(InputAction.SLOT_1, Keys.D1);
        keybinds.AddKeybind(InputAction.SLOT_2, Keys.D2);
        keybinds.AddKeybind(InputAction.SLOT_3, Keys.D3);
        keybinds.AddKeybind(InputAction.SLOT_4, Keys.D4);
        keybinds.AddKeybind(InputAction.SLOT_5, Keys.D5);
        keybinds.AddKeybind(InputAction.SLOT_6, Keys.D6);

        keybinds.AddKeybind(InputAction.TOGGLE_LIGHTING, Keys.E);
        keybinds.AddKeybind(InputAction.TOGGLE_WORLDGEN, Keys.Q);
        keybinds.AddKeybind(InputAction.FORWARD_TIME, Keys.Up);
        keybinds.AddKeybind(InputAction.BACKWARD_TIME, Keys.Down);
        
        keybinds.AddKeybind(InputAction.QUIT_GAME, Keys.Escape);
    }

    public static bool IsActionPressed(InputAction action)
    {
        return keybinds.IsActionPressed(action);
    }
    
    public static bool IsActionClicked(InputAction action)
    {
        return keybinds.IsActionClicked(action);
    }

    public static void Update()
    {
        keybinds.Update();
    }
}
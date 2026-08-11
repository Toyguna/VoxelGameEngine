namespace GameEngine3D;

public enum InputAction
{
    NONE,

    // MOVEMENT
    FORWARD, BACKWARD,
    RIGHT, LEFT,
    JUMP,

    // INTERACTION
    BREAK, PLACE,

    // INVENTORY
    SLOT_1, SLOT_2,
    SLOT_3, SLOT_4,
    SLOT_5, SLOT_6,
    SLOT_7, SLOT_8,
    SLOT_9,

    // DEBUG
    TOGGLE_LIGHTING,
    TOGGLE_WORLDGEN,
    FORWARD_TIME, BACKWARD_TIME,

    // MISC
    QUIT_GAME,

    //
    ACTION_COUNT
}
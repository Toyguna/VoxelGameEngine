using System;
using System.IO.Compression;
using System.Security.Cryptography;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameEngine3D;

public class Player : PhysicsEntity
{
    public Model PlayerModel { get; private set; }
    public Camera PlayerCamera { get; set; }

    public Vector3 CameraOffset { get; set; } = new Vector3(0, 2, 2);

    public TileData SelectedTile { get; set; } = TileRegistry.STONE;

    public Player(Camera playerCamera, World currentWorld) : base(new Vector3(-1, 15, -1), null, currentWorld)
    {
        PlayerCamera = playerCamera;
        CurrentWorld = currentWorld;

        Rotation = Vector3.Forward;
    }

    public override void Update(GameTime gameTime)
    {
        float deltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;

        HandleInput();
        Physics(deltaTime);
        UpdateCamera(gameTime);

        Rotation = PlayerCamera.Direction;
    }

    public void UpdateCamera(GameTime gameTime)
    {
        if (!PlayerCamera.FreeCam)
        {
            /*
            Vector3 forward = new Vector3(Rotation.X, 0, Rotation.Z);
            forward.Normalize();
            PlayerCamera.Position = forward + CameraOffset;
            */
            PlayerCamera.Position = Position + new Vector3(0, HitboxSize.Y - 1f, 0);
        }

        PlayerCamera.Update(gameTime);
    }

    public void HandleInput()
    {
        Movement();
        
        if (InputHandler.IsActionClicked(InputAction.BREAK)) BreakTile();
        if (InputHandler.IsActionClicked(InputAction.PLACE)) PlaceTile();

        if (InputHandler.IsActionClicked(InputAction.SLOT_1)) SelectedTile = TileRegistry.GRASS;
        if (InputHandler.IsActionClicked(InputAction.SLOT_2)) SelectedTile = TileRegistry.DIRT;
        if (InputHandler.IsActionClicked(InputAction.SLOT_3)) SelectedTile = TileRegistry.SAND;
        if (InputHandler.IsActionClicked(InputAction.SLOT_4)) SelectedTile = TileRegistry.STONE;
        if (InputHandler.IsActionClicked(InputAction.SLOT_5)) SelectedTile = TileRegistry.WATER;
    }

    public void Movement()
    {
        Vector3 forward = new Vector3(Rotation.X, 0, Rotation.Z);
        forward.Normalize();

        Vector3 right = Vector3.Cross(forward, Vector3.Up);
        right.Normalize();

        Vector3 moveDir = Vector3.Zero;

        if (InputHandler.IsActionPressed(InputAction.FORWARD)) moveDir += forward; 
        if (InputHandler.IsActionPressed(InputAction.BACKWARD)) moveDir -= forward; 
        if (InputHandler.IsActionPressed(InputAction.RIGHT)) moveDir += right; 
        if (InputHandler.IsActionPressed(InputAction.LEFT)) moveDir -= right;
        if (InputHandler.IsActionPressed(InputAction.JUMP) && OnGround) Velocity = Vector3.Up * JumpVelocity;

        if (moveDir != Vector3.Zero) moveDir.Normalize();
        Velocity = new Vector3(moveDir.X * WalkSpeed, Velocity.Y, moveDir.Z * WalkSpeed);
    }

    public void BreakTile()
    {
        MouseState mouseState = Mouse.GetState();

        Vector3? mwPos = CurrentWorld.MouseToWorldPosition(mouseState.Position, PlayerCamera);
        if (mwPos == null) return;

        CurrentWorld.DestroyTileAtWorld((Vector3)mwPos);
    }

    public void PlaceTile()
    {
        MouseState mouseState = Mouse.GetState();

        RaycastHit mwPos = CurrentWorld.MouseToWorldRay(mouseState.Position, PlayerCamera);
        if (mwPos == null) return;

        int wx = (int)Math.Floor(mwPos.HitPos.X + mwPos.HitNormal.X);
        int wy = (int)Math.Floor(mwPos.HitPos.Y + mwPos.HitNormal.Y);
        int wz = (int)Math.Floor(mwPos.HitPos.Z + mwPos.HitNormal.Z);

        if (Position.X >= wx - (HitboxSize.X / 2) && Position.X <= wx + 1 + (HitboxSize.X / 2) &&
            Position.Y >= wy - 0.1f && Position.Y <= wy + HitboxSize.Y + 0.1f &&
            Position.Z >= wz - (HitboxSize.Z / 2) && Position.Z <= wz + 1 + (HitboxSize.Z / 2))
        return;

        if (CurrentWorld.GetTileAtWorld(wx, wy, wz) != null) return;

        CurrentWorld.PlaceTileAtWorld(wx, wy, wz, new WorldTile(SelectedTile));
    }
}
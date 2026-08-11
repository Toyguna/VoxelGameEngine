using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameEngine3D;

public class Player : Entity
{
    public float MaxHealth { get; set; } = 100f;
    public float Health { get; set; } = 100f;

    public float WalkSpeed { get; set; } = 5f;
    public float JumpVelocity { get; set; } = 10f;
    public float Gravity { get; set; } = 8f;
    public float MaxFallVel { get; set; } = -30f;
    public Vector3 Velocity { get; set; } = Vector3.Zero;
    public bool OnGround { get; set; } = false;
    
    public Vector3 HitboxSize { get; set; } = new Vector3(0.4f, 1.6f, 0.4f);

    public Model PlayerModel { get; private set; }
    public Camera PlayerCamera { get; set; }
    public World CurrentWorld { get; set; }

    public Vector3 CameraOffset { get; set; } = new Vector3(0, 2, 2);

    private GraphicsDevice _graphicsDevice;

    private Vector3[] _directions =
    {
        Vector3.Forward,
        Vector3.Backward,
        Vector3.Right,
        Vector3.Left
    };

    public Player(Camera playerCamera, World currentWorld, GraphicsDevice gd) : base(new Vector3(-1, 15, -1), null)
    {
        PlayerCamera = playerCamera;
        CurrentWorld = currentWorld;

        Rotation = Vector3.Forward;

        _graphicsDevice = gd;
    }

    public void Update(GameTime gameTime)
    {
        float deltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;

        HandleInput(deltaTime);
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

    public void HandleInput(float deltaTime)
    {
        KeyboardState keyboardState = Keyboard.GetState();
        MouseState mouseState = Mouse.GetState();

        Vector3 forward = new Vector3(Rotation.X, 0, Rotation.Z);
        forward.Normalize();

        Vector3 right = Vector3.Cross(forward, Vector3.Up);
        right.Normalize();

        if (keyboardState.IsKeyDown(Keys.W)) Velocity += forward * WalkSpeed; 
        if (keyboardState.IsKeyDown(Keys.S)) Velocity -= forward * WalkSpeed; 
        if (keyboardState.IsKeyDown(Keys.D)) Velocity += right * WalkSpeed; 
        if (keyboardState.IsKeyDown(Keys.A)) Velocity -= right * WalkSpeed;
        if (keyboardState.IsKeyDown(Keys.Space) && OnGround) Velocity = Vector3.Up * JumpVelocity;

        // Look Around
        Point mousePos = mouseState.Position;

        float diffX = mousePos.X - _graphicsDevice.Viewport.Width / 2;
        float diffY = - mousePos.Y + _graphicsDevice.Viewport.Height / 2;

        PlayerCamera.Pitch += diffY * PlayerCamera.MouseSensitivity;
        PlayerCamera.Yaw += diffX * PlayerCamera.MouseSensitivity;
            
        Mouse.SetPosition(_graphicsDevice.Viewport.Width / 2, _graphicsDevice.Viewport.Height / 2);
    }

    public void Physics(float deltaTime)
    {
        Position += Velocity * deltaTime;

        bool foundGround = false;
        WorldTile groundTile = null;

        // GRAVITY
        if (Velocity.Y <= 0) // do ground check if not jumping
        {
            for (int i = 0; i < 4; i++)
            {
                WorldTile tileUnder = CurrentWorld.GetTileAtWorld(
                    Position + new Vector3(_directions[i].X * HitboxSize.X, -1f, _directions[i].Z * HitboxSize.Z)
                );
                if (tileUnder != null)
                {
                    if (Position.Y < tileUnder.Position.Y + 1.5f) continue;

                    WorldTile tileUnderAbove = CurrentWorld.GetTileAtWorld(
                        Position + new Vector3(_directions[i].X * HitboxSize.X, 0f, _directions[i].Z * HitboxSize.Z)
                    );

                    if (tileUnderAbove != null) continue;

                    foundGround = true;
                    groundTile = tileUnder;
                    break;
                }
            }
        }

        if (foundGround && groundTile != null)
        {
            Position = new Vector3(Position.X, groundTile.Position.Y + 2f, Position.Z);
            Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
            
            OnGround = true;
        }
        else
        {
            Velocity -= new Vector3(0, Gravity * deltaTime, 0);
            if (Velocity.Y < MaxFallVel)
            {
                Velocity = new Vector3(Velocity.X, MaxFallVel, Velocity.Z);
            }
            OnGround = false;
        }

        // HEAD COLLISION
        WorldTile aboveTile = CurrentWorld.GetTileAtWorld(
            Position + new Vector3(0, HitboxSize.Y - 1, 0)
        );

        if (aboveTile != null) // bonk the head
        {
            Position = new Vector3(Position.X, aboveTile.Position.Y - (HitboxSize.Y - 1) - 0.01f, Position.Z);
            Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
        }


        // X-Z COLLISION
        float newX = Position.X;
        float newZ = Position.Z;

        if (MathF.Abs(Velocity.X) > 0f)
        {
            WorldTile tileX = CurrentWorld.GetTileAtWorld(
                Position + new Vector3(HitboxSize.X / 2 * MathF.Sign(Velocity.X), -1, 0)
            ); // topleft is position

            if (tileX != null)
            {
                if (Velocity.X < 0) // right of the tile
                {
                    newX = tileX.Position.X + 1f + HitboxSize.X / 2;
                }
                else if (Velocity.X > 0) // left of the tile
                {
                    newX = tileX.Position.X - HitboxSize.X / 2;
                }
            }
            
            Velocity = new Vector3(0, Velocity.Y, Velocity.Z);
        }

        Position = new Vector3(newX, Position.Y, Position.Z);

        if (MathF.Abs(Velocity.Z) > 0f)
        {
            WorldTile tileZ = CurrentWorld.GetTileAtWorld(
                Position + new Vector3(0, -1, HitboxSize.Z / 2 * MathF.Sign(Velocity.Z))
            ); // topleft is position

            if (tileZ != null)
            {
                if (Velocity.Z < 0) // back of the tile
                {
                    newZ = tileZ.Position.Z + 1f + HitboxSize.Z / 2;
                }
                else if (Velocity.Z > 0) // front of the tile
                {
                    newZ = tileZ.Position.Z - HitboxSize.Z / 2;
                }
            }

            Velocity = new Vector3(Velocity.X, Velocity.Y, 0);            
        }
        
        Position = new Vector3(Position.X, Position.Y, newZ);
    }

    public void Draw()
    {
        
    }
}
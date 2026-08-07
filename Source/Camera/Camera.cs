using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameEngine3D;

public class Camera
{
    private GraphicsDevice _graphicsDevice;

    public float FieldOfView { get; set; } = 70;
    public int RenderDistance { get; set; } = 10;

    public Vector3 Position { get; set; } = new Vector3(0f, 5f, 0f);
    public Vector3 Direction { get; set; } = Vector3.Forward;
    public Vector3 Target { get; set; } = Vector3.Zero;

    public float MouseSensitivity = 0.3f;
    public float Pitch = 0f; // up - down
    public float Yaw = -90f; // right - left

    public bool LockMouse = true;
    
    public Matrix ProjectionMatrix { get; set; }
    public Matrix ViewMatrix { get; set; }
    public Matrix WorldMatrix { get; set; }

    private float MoveSpeed = 5f;

    private readonly float MAX_DRAW_LENGTH = 1000f;
    private readonly float NORMAL_SPEED = 15f;
    private readonly float FAST_SPEED = 30f;

    public bool _previousLockMouse = false;

    public Camera(GraphicsDevice gd)
    {
        _graphicsDevice = gd;

        Initialize();
    }

    private void Initialize()
    {
        WorldMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);
        ViewMatrix = Matrix.CreateLookAt(Position, Target, Vector3.Up);
        ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(FieldOfView), 
            _graphicsDevice.DisplayMode.AspectRatio, 
            0.1f, 
            MAX_DRAW_LENGTH
        );
    }

    public void CameraRotation()
    {
        MouseState mouseState = Mouse.GetState();

        if (LockMouse)
        {
            if (!_previousLockMouse)
            {
                Mouse.SetPosition(_graphicsDevice.Viewport.Width / 2, _graphicsDevice.Viewport.Height / 2);
                _previousLockMouse = true;
            }
            else
            {
                Point mousePos = mouseState.Position;

                float diffX = mousePos.X - _graphicsDevice.Viewport.Width / 2;
                float diffY = - mousePos.Y + _graphicsDevice.Viewport.Height / 2;

                Pitch += diffY * MouseSensitivity;
                Yaw += diffX * MouseSensitivity;
            
                Mouse.SetPosition(_graphicsDevice.Viewport.Width / 2, _graphicsDevice.Viewport.Height / 2);          
            }
        }

        Pitch = Math.Clamp(Pitch, -89, 89);
        float radPitch = MathHelper.ToRadians(Pitch);
        float radYaw = MathHelper.ToRadians(Yaw);

        Vector3 newDir = new Vector3(
            (float)(Math.Cos(radPitch) * Math.Cos(radYaw)),
            (float)Math.Sin(radPitch),
            (float)(Math.Cos(radPitch) * Math.Sin(radYaw))
        );

        Direction = Vector3.Normalize(newDir);
    }

    public void CameraMovement(float deltaTime)
    {
        KeyboardState state = Keyboard.GetState();

        Vector3 forward = new Vector3(Direction.X, 0, Direction.Z);
        forward.Normalize();

        Vector3 right = Vector3.Cross(forward, Vector3.Up);
        right.Normalize();

        MoveSpeed = state.IsKeyDown(Keys.LeftShift) ? NORMAL_SPEED : FAST_SPEED; 

        if (state.IsKeyDown(Keys.W)) Position += forward * MoveSpeed * deltaTime; 
        if (state.IsKeyDown(Keys.S)) Position -= forward * MoveSpeed * deltaTime; 
        if (state.IsKeyDown(Keys.D)) Position += right * MoveSpeed * deltaTime; 
        if (state.IsKeyDown(Keys.A)) Position -= right * MoveSpeed * deltaTime;
        if (state.IsKeyDown(Keys.Space)) Position += Vector3.Up * MoveSpeed * deltaTime;
        if (state.IsKeyDown(Keys.LeftControl)) Position -= Vector3.Up * MoveSpeed * deltaTime;
    }

    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        CameraMovement(deltaTime);
        CameraRotation();

        Target = Position + Direction;

        ViewMatrix = Matrix.CreateLookAt(Position, Target, Vector3.Up);
        ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(FieldOfView), 
            _graphicsDevice.DisplayMode.AspectRatio, 
            0.1f, 
            MAX_DRAW_LENGTH
        );
    }

    public Vector3 GetChunkGridPosition()
    {
        return new Vector3(
            (int)Math.Floor(Position.X / 16),
            (int)Math.Floor(Position.Y / 16),
            (int)Math.Floor(Position.Z / 16)
        );
    }

    public Vector3 GetChunkLocalPosition()
    {
        return new Vector3(
            Math.Abs((int)Math.Floor(Position.X % 16)),
            Math.Abs((int)Math.Floor(Position.Y % 16)),
            Math.Abs((int)Math.Floor(Position.Z % 16))
        );
    }

}
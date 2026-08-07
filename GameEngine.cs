using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameEngine3D;

public class GameEngine : Game
{
    private GraphicsDeviceManager _graphics;

    private GameRenderer _gameRenderer;

    private Camera _camera;
    private WorldGenerator _worldGen;
    private World _world;
    private MeshBufferPool _meshBuffer;

    public int FrameRate { get; private set; } = 0;
    private TimeSpan _elapsedTime = TimeSpan.Zero;
    private int _frameCounter = 0;

    private bool _qpressed = false;
    private bool _epressed = false;
    private bool _altpressed = false;
    private bool _altlock = false;
    private bool _m1pressed = false;

    public GameEngine()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
    }

    protected override void Initialize()
    {
        _graphics.SynchronizeWithVerticalRetrace = false;
        _graphics.ApplyChanges();

        IsFixedTimeStep = false;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        GraphicsDevice.BlendState = BlendState.Opaque;
        GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

        _meshBuffer = new MeshBufferPool();
        _gameRenderer = new GameRenderer(GraphicsDevice, _meshBuffer);
        _camera = new Camera(GraphicsDevice);
        
        EffectHandler.Initialize(GraphicsDevice);
        TextureHandler.Initialize(GraphicsDevice);
        IdRegistry.Initialize();
        TileRegistry.Initialize();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        TextureHandler.LoadContent();

        TileRegistry.AddTexturesToTiles();

        EffectHandler.MainEffect.Texture = TextureHandler.TextureAtlas;
        EffectHandler.MainEffect.TextureEnabled = true;

        GenerateWorld();
    }

    protected override void Update(GameTime gameTime)
    {
        if (IsActive && IsMouseInWindow())
        {
            IsMouseVisible = !_camera.LockMouse;

            HandleInput(gameTime);

            _camera.Update(gameTime);
            _world.Update(gameTime, _camera);
        }

        UpdateTitle();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        CalculateFps(gameTime);

        _gameRenderer.DrawWorld(_world, _camera);

        base.Draw(gameTime);
    }

    private void HandleInput(GameTime gameTime)
    {
        KeyboardState state = Keyboard.GetState();
        MouseState mouseState = Mouse.GetState();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || state.IsKeyDown(Keys.Escape))
            Exit();

        if (state.IsKeyDown(Keys.Q) && !_qpressed)
        {
            _qpressed = true;
            if (_worldGen.IsWorldGenPaused())
            {
                _worldGen.ResumeWorldGen();
                Console.WriteLine("World Generation resumed.");
            }
            else
            {
                _worldGen.PauseWorldGen();
                _gameRenderer.UpdateMeshes(_world);
                Console.WriteLine("World Generation paused.");
            }
        }

        if (state.IsKeyUp(Keys.Q))
        {
            _qpressed = false;
        }

        if (state.IsKeyDown(Keys.E) && !_epressed)
        {
            _epressed = true;
            if (!EffectHandler.MainEffect.LightingEnabled)
            {
                EffectHandler.MainEffect.LightingEnabled = true;
                Console.WriteLine("Lighting enabled.");
            }
            else
            {
                EffectHandler.MainEffect.LightingEnabled = false;
                Console.WriteLine("Lighting disabled.");
            }
        }

        if (state.IsKeyUp(Keys.E))
        {
            _epressed = false;
        }

        if (!_altlock)
        {
            _camera.LockMouse = mouseState.RightButton == ButtonState.Pressed;
            if (!_camera.LockMouse == true)
            {
                _camera._previousLockMouse = false;
            }
        }

        if (state.IsKeyDown(Keys.LeftAlt) && !_altpressed)
        {
            _altpressed = true;
            _camera.LockMouse = !_camera.LockMouse;

            _altlock = _camera.LockMouse;
        }

        if (state.IsKeyUp(Keys.LeftAlt))
        {
            _altpressed = false;
        }

        if (mouseState.LeftButton == ButtonState.Pressed && !_m1pressed)
        {
            _m1pressed = true;

            Vector3? mwPos = _world.MouseToWorldPosition(mouseState.Position, _camera);
            if (mwPos != null)
            {
                _world.DestroyTileAtWorld((Vector3)mwPos);
            }
        }

        if (mouseState.LeftButton == ButtonState.Released)
        {
            _m1pressed = false;
        }
    }

    private void GenerateWorld()
    {
        _worldGen = new WorldGenerator();

        _world = new World(GraphicsDevice, _meshBuffer);
        _worldGen.GenerateNewWorld(WorldGenType.DEFAULT, new Vector2(1000, 1000), _world, 11293192, _camera, _meshBuffer);
    }

    private void CalculateFps(GameTime gameTime)
    {
        _elapsedTime += gameTime.ElapsedGameTime;

        _frameCounter++;

        if (_elapsedTime >= TimeSpan.FromSeconds(1))
        {
            FrameRate = _frameCounter;
            _frameCounter = 0;
            _elapsedTime -= TimeSpan.FromSeconds(1);
        }
    }

    private void UpdateTitle()
    {
        if (!IsActive)
        {
            Window.Title = "Voxel Engine (Window out of focus)";
            return;
        }

        Vector3 camChunkLocalPos = _camera.GetChunkLocalPosition();
        Vector3 camChunkGridPos = _camera.GetChunkGridPosition();

        Window.Title = $"Voxel Engine | FPS: {FrameRate} | " +
         $"Pos: ({_camera.Position.X:F2}, {_camera.Position.Y:F2}, {_camera.Position.Z:F2}) " +
         $"Rot: ({_camera.Direction.X:F2}, {_camera.Direction.Y:F2}, {_camera.Direction.Z:F2}) | " +
         $"Chunk Local: ({camChunkLocalPos.X}, {camChunkLocalPos.Y}, {camChunkLocalPos.Z}) " +
         $"Chunk Grid: ({camChunkGridPos.X}, {camChunkGridPos.Y}, {camChunkGridPos.Z})";
    }

    private bool IsMouseInWindow()
    {
        return GraphicsDevice.Viewport.Bounds.Contains(Mouse.GetState().Position);
    }
}
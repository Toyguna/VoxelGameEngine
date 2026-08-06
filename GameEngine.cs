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
    private BasicEffect _effect;
    private MeshBufferPool _meshBuffer;

    public int FrameRate { get; private set; } = 0;
    private TimeSpan _elapsedTime = TimeSpan.Zero;
    private int _frameCounter = 0;

    private bool _qpressed = false;
    private bool _epressed = false;
    private bool _altpressed = false;
    private bool _altlock = false;

    public GameEngine()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
    }

    private void CreateWorld()
    {
        _worldGen = new WorldGenerator();

        _world = new World(GraphicsDevice);
        _worldGen.GenerateNewWorld(WorldGenType.DEFAULT, new Vector2(1000, 1000), _world, 11293192, _camera, _meshBuffer);
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
        _gameRenderer = new GameRenderer(_meshBuffer);

        _camera = new Camera(GraphicsDevice);
        _effect = new BasicEffect(GraphicsDevice)
        {
            VertexColorEnabled = true,
            LightingEnabled = true
        };
        
        _effect.DirectionalLight0.Enabled = true;
        _effect.DirectionalLight0.DiffuseColor = new Vector3(1f, 1f, 1f);
        _effect.DirectionalLight0.Direction = new Vector3(-1.0f, -1.0f, -1.0f);

        TextureHandler.Initialize(GraphicsDevice);
        IdRegistry.Initialize();
        TileRegistry.Initialize();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        TextureHandler.LoadContent();

        TileRegistry.AddTexturesToTiles();

        _effect.Texture = TextureHandler.TextureAtlas;
        _effect.TextureEnabled = true;

        CreateWorld();
    }

    protected override void Update(GameTime gameTime)
    {
        float time = (float)gameTime.TotalGameTime.TotalSeconds;

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
            if (!_effect.LightingEnabled)
            {
                _effect.LightingEnabled = true;
                Console.WriteLine("Lighting enabled.");
            }
            else
            {
                _effect.LightingEnabled = false;
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

        // TODO: Add your update logic here

        if (IsActive)
        {
            _camera.Update(gameTime);

            IsMouseVisible = !_camera.LockMouse;
        }    
        
        _effect.DirectionalLight0.DiffuseColor = new Vector3(1f, 1f, 1f);

        Vector3 camChunkLocalPos = _camera.GetChunkLocalPosition();
        Vector3 camChunkGridPos = _camera.GetChunkGridPosition();

        Window.Title = $"FPS: {FrameRate} | " +
         $"Pos: ({_camera.Position.X:F2}, {_camera.Position.Y:F2}, {_camera.Position.Z:F2}) " +
         $"Rot: ({_camera.Direction.X:F2}, {_camera.Direction.Y:F2}, {_camera.Direction.Z:F2}) | " +
         $"Chunk Local: ({camChunkLocalPos.X}, {camChunkLocalPos.Y}, {camChunkLocalPos.Z}) " +
         $"Chunk Grid: ({camChunkGridPos.X}, {camChunkGridPos.Y}, {camChunkGridPos.Z})";

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        CalculateFps(gameTime);

        // TODO: Add your drawing code here
        _gameRenderer.DrawWorld(_world, _effect, _camera);

        base.Draw(gameTime);
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
}

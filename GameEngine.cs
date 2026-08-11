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
    private Player _player;

    public int FrameRate { get; private set; } = 0;
    private TimeSpan _elapsedTime = TimeSpan.Zero;
    private int _frameCounter = 0;

    public float WorldTime = 0.7f;
    public int WorldDayLength = 2000;

    private Vector3 minLightLevel = new Vector3(0.2f, 0.2f, 0.2f);
    private readonly Vector3 baseSkyColor = new Vector3(0.53f, 0.81f, 0.98f);

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

        EffectHandler.Initialize(GraphicsDevice);
        TextureHandler.Initialize(GraphicsDevice);
        ModelHandler.Initialize(Content);
        IdRegistry.Initialize();
        TileRegistry.Initialize();
        InputHandler.Initialize();

        _meshBuffer = new MeshBufferPool();
        _gameRenderer = new GameRenderer(GraphicsDevice, _meshBuffer);
        _camera = new Camera(GraphicsDevice);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        TextureHandler.LoadContent();
        ModelHandler.LoadModels();

        TileRegistry.AddTexturesToTiles();

        EffectHandler.MainEffect.Texture = TextureHandler.TextureAtlas;
        EffectHandler.MainEffect.TextureEnabled = true;

        GenerateWorld();
        
        _player = new Player(_camera, _world);
    }

    protected override void Update(GameTime gameTime)
    {
        InputHandler.Update();

        if (IsActive && IsMouseInWindow())
        {
            _camera.LockMouse = true;
            IsMouseVisible = !_camera.LockMouse;

            HandleInput(gameTime);

            _player.Update(gameTime);
            _world.Update(gameTime, _camera);
        }
        else
        {
            _camera.LockMouse = false;
            IsMouseVisible = true;
        }

        UpdateSkyLight();
        UpdateTitle();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(baseSkyColor * WorldTime));
        
        CalculateFps(gameTime);

        _gameRenderer.DrawWorld(_world, _camera);
    }

    private void HandleInput(GameTime gameTime)
    {        
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (InputHandler.IsActionClicked(InputAction.QUIT_GAME)) Exit();

        if (InputHandler.IsActionClicked(InputAction.TOGGLE_WORLDGEN))
        {
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

        if (InputHandler.IsActionClicked(InputAction.TOGGLE_LIGHTING))
        {
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
    
        if (InputHandler.IsActionPressed(InputAction.FORWARD_TIME))
        {
            WorldTime += 0.3f * deltaTime;

            if (WorldTime > 1) WorldTime = 1;
        }
        
        if (InputHandler.IsActionPressed(InputAction.BACKWARD_TIME))
        {
            WorldTime -= 0.3f * deltaTime;
            
            if (WorldTime < 0f) WorldTime = 0f;
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
         $"Pos: ({_player.Position.X:F2}, {_player.Position.Y:F2}, {_player.Position.Z:F2}) " +
         $"Rot: ({_player.Rotation.X:F2}, {_player.Rotation.Y:F2}, {_player.Rotation.Z:F2}) | " +
         $"Chunk Local: ({camChunkLocalPos.X}, {camChunkLocalPos.Y}, {camChunkLocalPos.Z}) " +
         $"Chunk Grid: ({camChunkGridPos.X}, {camChunkGridPos.Y}, {camChunkGridPos.Z})";
    }

    private void UpdateSkyLight()
    {
        EffectHandler.MainEffect.DirectionalLight0.DiffuseColor = Vector3.One * WorldTime + minLightLevel;
        EffectHandler.MainEffect.DirectionalLight1.DiffuseColor = Vector3.One * WorldTime / 2 + minLightLevel / 2;
    }

    private bool IsMouseInWindow()
    {
        return GraphicsDevice.Viewport.Bounds.Contains(Mouse.GetState().Position);
    }
}
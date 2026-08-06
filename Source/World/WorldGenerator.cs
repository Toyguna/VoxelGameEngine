using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Xna.Framework;

namespace GameEngine3D;

public class WorldGenerator
{
    public WorldGenType GenerationType { get; private set; }

    private PerlinNoise _perlinNoise;
    private float scale = 0.05f;

    private MeshBufferPool _meshBuffer;

    private Thread WorldGenThread;
    private ManualResetEventSlim _wgThreadPause = new ManualResetEventSlim(true); 

    public void GenerateNewWorld(WorldGenType genType, Vector2 size, World world, int seed, Camera camera, MeshBufferPool meshBuffer)
    {
        GenerationType = genType;

        _meshBuffer = meshBuffer;
        _perlinNoise = new PerlinNoise(seed);
        world.ClearWorld();

        ThreadEntryPoint(world, camera);
    }

    public void GenerateWorldAroundCamera(World world, Camera camera)
    {
        Vector3 camGridPos = camera.GetChunkGridPosition();
        int chunkGenerationRadius = camera.RenderDistance + 5;

        ConcurrentDictionary<Vector3, Chunk> chunkDict = world.GetChunks();

        for (int x = (int)camGridPos.X - chunkGenerationRadius; x < camGridPos.X + chunkGenerationRadius; x++)
        {
            for (int z = (int)camGridPos.Z - chunkGenerationRadius; z < camGridPos.Z + chunkGenerationRadius; z++)
            {
                Vector3 newChunkPos = new Vector3(x, 0, z);
                if (chunkDict.ContainsKey(newChunkPos)) continue;

                GenerateSurfaceChunk(newChunkPos, world);
            }
        }
    }

    private Chunk CreateEmptyChunk(int cx, int cy, int cz, World world)
    {
        Chunk chunk = new Chunk(cx, cy, cz, world);

        world.AddChunk(chunk);

        return chunk;
    }

  

    private void DefaultSurfaceChunkGeneration(int cx, int cz, World world)
    {
        Vector3 chunkWorldOrigin = new Vector3(cx, 0, cz) * 16;

        Chunk chunk = new Chunk(cx, 0, cz, world);
        world.AddChunk(chunk);

        Chunk belowChunk = null;

        // Surface generation
        for (int x = 0; x < chunk.ChunkSizeX; x++)
        {
            for (int z = 0; z < chunk.ChunkSizeZ; z++)
            {   
                double noiseValue = _perlinNoise.Noise(
                    (x + chunkWorldOrigin.X) * scale, 
                    (z + chunkWorldOrigin.Z) * scale
                );

                int tileY = (int)(chunkWorldOrigin.Y + (noiseValue * 5));

                Vector3 worldPos = new Vector3(
                    (int)(x + chunkWorldOrigin.X),
                    tileY, 
                    (int)(z + chunkWorldOrigin.Z)  
                );


                BasicTileData tileData = TileDataRange(tileY);
                WorldBasicTile tile = new WorldBasicTile(tileData, worldPos);

                if (tileY < 0)
                {
                    belowChunk = world.GetChunkAtGrid(cx, - 1, cz);
                    belowChunk.SetTile(tile);
                }
                else
                {
                    chunk.SetTile(tile);
                }
            }
        }
        
        world.UpdateChunkMesh(chunk, _meshBuffer);
        world.UpdateChunkMesh(belowChunk, _meshBuffer);
    }

    private void FlatSurfaceChunkGeneration(int cx, int cz, World world)
    {
        Vector3 chunkWorldOrigin = new Vector3(cx, 0, cz) * 16;

        Chunk chunk = new Chunk(cx, 0, cz, world);
        world.AddChunk(chunk);

        if (cx == 0 && cz == 0) return;

        Chunk belowChunk = null;

        // Surface generation
        for (int x = 0; x < chunk.ChunkSizeX; x++)
        {
            for (int y = -10; y < 1; y++)
            {
                for (int z = 0; z < chunk.ChunkSizeZ; z++)
                {   
                    Vector3 worldPos = new Vector3(
                        (int)(x + chunkWorldOrigin.X),
                        (int)(y + chunkWorldOrigin.Y), 
                        (int)(z + chunkWorldOrigin.Z)  
                    );

                    BasicTileData tileData = y switch
                    {
                        <= -10 => TileRegistry.WATER,
                        <= -3 => TileRegistry.STONE,
                        <= -1 => TileRegistry.DIRT,
                        >= 0 => TileRegistry.GRASS
                    };

                    WorldBasicTile tile = new WorldBasicTile(tileData, worldPos);

                    if (y < 0)
                    {
                        belowChunk = world.GetChunkAtGrid(cx, - 1, cz);
                        belowChunk.SetTile(tile);
                    }
                    else
                    {
                        chunk.SetTile(tile);
                    }
                }
            }
        }
        
        world.UpdateChunkMesh(chunk, _meshBuffer);
        world.UpdateChunkMesh(belowChunk, _meshBuffer);
    }

    private void GenerateSurfaceChunk(int cx, int cz, World world)
    {
        if (GenerationType == WorldGenType.DEFAULT)
        {
            DefaultSurfaceChunkGeneration(cx, cz, world);
        }
        else if (GenerationType == WorldGenType.FLAT)
        {
            FlatSurfaceChunkGeneration(cx, cz, world);
        }
    }

    private void GenerateSurfaceChunk(Vector3 cposition, World world)
    {
        GenerateSurfaceChunk((int)cposition.X, (int)cposition.Z, world);    
    }

    private BasicTileData TileDataRange(float y)
    {
        BasicTileData tileData;

        tileData = y switch
        {
          <= -0.3f => TileRegistry.WATER,
          <= 0.2f  => TileRegistry.SAND,
          >= 0.2f  => TileRegistry.GRASS,
          _        => TileRegistry.SAND
        };

        return tileData;
    }
    
    private void ThreadEntryPoint(World world, Camera camera)
    {
        WorldGenThread = new Thread(() => WorldGenThreadLoop(world, camera))
        {
            IsBackground = true
        };
        WorldGenThread.Start();
    }

    private void WorldGenThreadLoop(World world, Camera camera)
    {
        // Generate first 5 chunks
        for (int x = -2; x < 2; x++)
        {
            for (int y = -2; y < 2; y++)
            {
                GenerateSurfaceChunk(x, y, world);
            }
        }

        while (true)
        {
            if (!_wgThreadPause.IsSet) continue;
            
            GenerateWorldAroundCamera(world, camera);
        }
    }

    public void PauseWorldGen() => _wgThreadPause.Reset();
    public void ResumeWorldGen() => _wgThreadPause.Set();
    public bool IsWorldGenPaused() => !_wgThreadPause.IsSet;
}
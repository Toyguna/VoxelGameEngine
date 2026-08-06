using System;
using System.Collections.Concurrent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine3D;

public class World
{
    public GraphicsDevice _graphicsDevice;
    
    private ConcurrentDictionary<Vector3, Chunk> chunks;

    public World(GraphicsDevice gd)
    {
        _graphicsDevice = gd;

        Initialize();
    }

    private void Initialize()
    {
        chunks = new ConcurrentDictionary<Vector3, Chunk>();
    }

    public void ClearWorld()
    {
        chunks.Clear();
    }

    public void AddChunk(Chunk chunk)
    {
        if (chunk == null) return;
        chunks.TryAdd(chunk.GridPosition, chunk);
    }

    public int GetChunkCount()
    {
        return chunks.Count;
    }

    public Chunk GetChunkAtGrid(int cx, int cy, int cz)
    {
        Vector3 key = new Vector3(cx, cy, cz);

        if (!chunks.ContainsKey(key))
        {
            Chunk chunk = new Chunk(cx, cy, cz, this);
            AddChunk(chunk);

            return chunk;
        }

        Chunk chunk_out;
        chunks.TryGetValue(key, out chunk_out);

        return chunk_out; 
    }

    public Chunk GetChunkAtWorld(int wx, int wy, int wz)
    {
        Vector3 chunkGridPos = new Vector3(
            (int)Math.Floor(wx / 16.0f),
            (int)Math.Floor(wy / 16.0f),
            (int)Math.Floor(wz / 16.0f)
        );

        Chunk chunk_out;
        chunks.TryGetValue(chunkGridPos, out chunk_out);

        return chunk_out; 
    }

    public WorldBasicTile GetTileAtWorld(Vector3 worldpos)
    {
        return GetTileAtWorld((int)worldpos.X, (int)worldpos.Y, (int)worldpos.Z);
    }

    public WorldBasicTile GetTileAtWorld(int wx, int wy, int wz)
    {
        Chunk chunk = GetChunkAtWorld(wx, wy, wz);
        if (chunk == null) return null;

        return chunk.GetTileAtWorld(wx, wy, wz);
    }

    public ConcurrentDictionary<Vector3, Chunk> GetChunks()
    {
        return chunks;
    }

    public void Draw(BasicEffect effect, Camera camera)
    {
        foreach (var item in chunks)
        {
            Chunk chunk;
            chunks.TryGetValue(item.Key, out chunk);
            if (chunk == null) continue;

            if (!chunk.ShouldDraw(camera.GetChunkGridPosition(), camera.RenderDistance)) continue;

            chunk.Draw(_graphicsDevice, effect, camera);
        }
    }

    public void UpdateChunkMesh(Chunk chunk, MeshBufferPool meshBuffer)
    {
        if (chunk == null) return;

        meshBuffer.Clear();
        ChunkMeshBuilder.BuildMesh(chunk, chunk.ParentWorld, meshBuffer.VertexScratchpad, meshBuffer.IndexScratchpad);
        chunk.SendToGPU(_graphicsDevice, meshBuffer.VertexScratchpad, meshBuffer.IndexScratchpad);
    }
}
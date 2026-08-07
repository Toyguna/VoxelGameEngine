using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine3D;

public class GameRenderer
{
    private GraphicsDevice _graphicsDevice;
    private MeshBufferPool _meshBuffer;
    private int lastChunkCount = 0;

    public BasicEffect MainEffect { get; set; }

    public GameRenderer(GraphicsDevice gd, MeshBufferPool meshBufferPool)
    {
        _graphicsDevice = gd;
        _meshBuffer = meshBufferPool;

        Initialize();
    }

    public void Initialize()
    {
    }

    public void UpdateMeshes(World world)
    {
        int chunkCount = world.GetChunkCount();

        if (lastChunkCount != chunkCount)
        {
            foreach (var item in world.GetChunks())
            {
                Chunk chunk = item.Value;

                world.UpdateChunkMesh(chunk);
            }
        }

        lastChunkCount = chunkCount;
    }

    public void DrawWorld(World world, Camera camera)
    {
        world.Draw(camera);
    }
}
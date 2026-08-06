using Microsoft.Xna.Framework.Graphics;

namespace GameEngine3D;

public class GameRenderer
{
    private MeshBufferPool _meshBuffer;

    private int lastChunkCount = 0;

    public GameRenderer(MeshBufferPool meshBufferPool)
    {
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
                //if (chunk.MeshUpdated) continue;

                world.UpdateChunkMesh(chunk, _meshBuffer);
            }
        }

        lastChunkCount = chunkCount;
    }

    public void DrawWorld(World world, BasicEffect effect, Camera camera)
    {
        world.Draw(effect, camera);
    }
}
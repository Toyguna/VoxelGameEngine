using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine3D;

public class Chunk
{
    public DynamicVertexBuffer VertexBuffer;
    public IndexBuffer IndexBuffer;
    public int TriangleCount;

    public readonly int ChunkSizeX = 16;
    public readonly int ChunkSizeY = 16;
    public readonly int ChunkSizeZ = 16;

    public Vector3 GridPosition;
    public Vector3 WorldPosition;
    public World ParentWorld;

    private WorldBasicTile[] tileArray;

    public Chunk(Vector3 gridPosition, World parentWorld)
    {
        GridPosition = gridPosition;
        ParentWorld = parentWorld;
        WorldPosition = new Vector3(
            (int)GridPosition.X * 16,
            (int)GridPosition.Y * 16,
            (int)GridPosition.Z * 16
        );

        Initialize();
    }

    public Chunk(int x, int y, int z, World parentWorld)
    {
        ParentWorld = parentWorld;
        GridPosition = new Vector3(x, y, z);
        WorldPosition = WorldPosition = new Vector3(
            (int)GridPosition.X * 16,
            (int)GridPosition.Y * 16,
            (int)GridPosition.Z * 16
        );

        Initialize();
    }

    public void Initialize()
    {
        tileArray = new WorldBasicTile[ChunkSizeX * ChunkSizeY * ChunkSizeZ];
    }

    public void ClearChunk()
    {
        tileArray = new WorldBasicTile[ChunkSizeX * ChunkSizeY * ChunkSizeZ];
    }

    public void SetTile(int x, int y, int z, WorldBasicTile tile)
    {
        tile.Position = new Vector3(x, y, z);
        tileArray[GetIndexFromPos(x, y, z)] = tile;
    }
    
    public void SetTile(WorldBasicTile tile)
    {
        tileArray[GetIndexFromPos(tile.Position)] = tile;
    }

    public WorldBasicTile[] GetTiles()
    {
        return tileArray;
    }

    public bool HasTiles()
    {
        return tileArray.Length != 0;
    }

    public WorldBasicTile GetTileAtWorld(int x, int y, int z)
    {
        Vector3 localPos = LocalFromWorldPosition(new Vector3(x, y, z));

        return GetTileAtLocal((int)localPos.X, (int)localPos.Y, (int)localPos.Z);
    }

    public WorldBasicTile GetTileAtLocal(int x, int y, int z)
    {
        return tileArray[GetIndexFromPos(x, y, z)];
    }

    public void DestroyTileAt(int x, int y, int z)
    {
        tileArray[GetIndexFromPos(x, y, z)] = null;
    }

    public bool ShouldDraw(Vector3 camGridPos, int renderDistance)
    {
        return (
            camGridPos.X >= GridPosition.X - renderDistance && camGridPos.X <= GridPosition.X + renderDistance &&
            camGridPos.Y >= GridPosition.Y - renderDistance && camGridPos.Y <= GridPosition.Y + renderDistance &&
            camGridPos.Z >= GridPosition.Z - renderDistance && camGridPos.Z <= GridPosition.Z + renderDistance
        );
    }

    public Vector3 LocalFromWorldPosition(Vector3 position)
    {
        return new Vector3(
            ((int)Math.Floor(position.X)) & 15,
            ((int)Math.Floor(position.Y)) & 15,
            ((int)Math.Floor(position.Z)) & 15
        );
    }

    public Vector3 WorldFromLocalPosition(Vector3 position)
    {
        return new Vector3(
            (int)GridPosition.X * 16 + position.X,
            (int)GridPosition.Y * 16 + position.Y,
            (int)GridPosition.Z * 16 + position.Z
        );
    }

    private int GetIndexFromPos(int x, int y, int z)
    {
        return x + (y * ChunkSizeX) + (z * ChunkSizeX * ChunkSizeY);
    }

    private int GetIndexFromPos(Vector3 position)
    {
        var localVec = LocalFromWorldPosition(position);

        return GetIndexFromPos((int)localVec.X, (int)localVec.Y, (int)localVec.Z);
    }

    public void SendToGPU(GraphicsDevice graphicsDevice, List<VertexPositionColorNormalTexture> vertices, List<int> indices)
    {
        if (vertices.Count == 0) return;

        TriangleCount = indices.Count / 3;

        if (VertexBuffer == null || VertexBuffer.VertexCount < vertices.Count)
        {
            VertexBuffer?.Dispose();
            VertexBuffer = new DynamicVertexBuffer(
                graphicsDevice, typeof(VertexPositionColorNormalTexture), vertices.Count, BufferUsage.WriteOnly
            );
        }

        if (IndexBuffer == null || IndexBuffer.IndexCount < indices.Count)
        {
            IndexBuffer?.Dispose();
            IndexBuffer = new IndexBuffer(
                graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.WriteOnly
            );
        }

        VertexBuffer.SetData(vertices.ToArray(), 0, vertices.Count, SetDataOptions.Discard);
        IndexBuffer.SetData(indices.ToArray());
    }

    public void Draw(GraphicsDevice graphicsDevice, BasicEffect effect, Camera camera)
    {
        if (VertexBuffer == null || TriangleCount == 0 || IndexBuffer == null || IndexBuffer.IndexCount == 0) return;

        effect.World = Matrix.CreateTranslation(WorldPosition);
        effect.View = camera.ViewMatrix;
        effect.Projection = camera.ProjectionMatrix;

        graphicsDevice.SetVertexBuffer(VertexBuffer);
        graphicsDevice.Indices = IndexBuffer;

        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawIndexedPrimitives(
                PrimitiveType.TriangleList, 0, 0, TriangleCount);
        }
    }
}  
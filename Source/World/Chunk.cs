using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameEngine3D;

public class Chunk
{
    public DynamicVertexBuffer VertexBuffer;
    public IndexBuffer IndexBuffer;
    public int TriangleCount;

    public bool GenerationCompleted = false;
    public bool MeshBuilt = false;

    public readonly int ChunkSizeX = 16;
    public readonly int ChunkSizeY = 16;
    public readonly int ChunkSizeZ = 16;

    public Vector3 GridPosition;
    public Vector3 WorldPosition;
    public World ParentWorld;

    private WorldTile[] tileArray;

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

    public Chunk(int cx, int cy, int cz, World parentWorld)
    {
        ParentWorld = parentWorld;
        GridPosition = new Vector3(cx, cy, cz);
        WorldPosition = new Vector3(
            (int)GridPosition.X * 16,
            (int)GridPosition.Y * 16,
            (int)GridPosition.Z * 16
        );

        Initialize();
    }

    private void Initialize()
    {
        tileArray = new WorldTile[ChunkSizeX * ChunkSizeY * ChunkSizeZ];
    }

/// <summary>
/// Clears the tile array of the chunk.
/// </summary>
    public void ClearChunk()
    {
        tileArray = new WorldTile[ChunkSizeX * ChunkSizeY * ChunkSizeZ];
    }

/// <summary>
/// Sets the tile at the given world position.
/// </summary>
/// <param name="wx"></param>
/// <param name="wy"></param>
/// <param name="wz"></param>
/// <param name="tile"></param>
    public void SetTileAtWorld(int wx, int wy, int wz, WorldTile tile)
    {
        tile.Position = new Vector3(wx, wy, wz);
        tileArray[GetIndexFromWorldPos(wx, wy, wz)] = tile;
    }

/// <summary>
/// Sets the tile at the tile's world position.
/// </summary>
/// <param name="tile"></param>
    public void SetTileAtWorld(WorldTile tile)
    {
        tileArray[GetIndexFromWorldPos(tile.Position)] = tile;
    }

/// <summary>
/// Returns the tile array of the chunk.
/// </summary>
/// <returns>Tiles of the chunk in a <b>WorldTile[]</b></returns>
    public WorldTile[] GetTiles()
    {
        return tileArray;
    }

/// <summary>
/// Checks if the chunk has tiles.
/// </summary>
/// <returns><b>true</b> if chunk has tiles, <b>false</b> otherwise.</returns>
    public bool HasTiles()
    {
        return tileArray.Length != 0;
    }

/// <summary>
/// Returns the tile at the world position.
/// </summary>
/// <param name="wx"></param>
/// <param name="wy"></param>
/// <param name="wz"></param>
/// <returns><b>WorldTile</b> at position, <b>null</b> if tile doesn't exist.</returns>
    public WorldTile GetTileAtWorld(int wx, int wy, int wz)
    {
        return GetTileAtWorld(new Vector3(wx, wy, wz));
    }

/// <summary>
/// Returns the tile at the world position.
/// </summary>
/// <param name="worldPos"></param>
/// <returns><b>WorldTile</b> at position, <b>null</b> if tile doesn't exist.</returns>
    public WorldTile GetTileAtWorld(Vector3 worldPos)
    {
        Vector3 localPos = LocalFromWorldPosition(worldPos);

        return GetTileAtLocal((int)localPos.X, (int)localPos.Y, (int)localPos.Z);
    }

/// <summary>
/// Returns the tile at the given local position.
/// </summary>
/// <param name="lx"></param>
/// <param name="ly"></param>
/// <param name="lz"></param>
/// <returns><b>WorldTile</b> at the given local position, <b>null</b> if no tile exists as position.</returns>
    public WorldTile GetTileAtLocal(int lx, int ly, int lz)
    {
        return tileArray[GetIndexFromLocalPos(lx, ly, lz)];
    }

/// <summary>
/// Returns the tile at the given local position.
/// </summary>
/// <param name="localPos"></param>
/// <returns><b>WorldTile</b> at the given local position, <b>null</b> if no tile exists as position.</returns>
    public WorldTile GetTileAtLocal(Vector3 localPos)
    {
        return tileArray[GetIndexFromLocalPos((int)localPos.X, (int)localPos.Y, (int)localPos.Z)];
    }

/// <summary>
/// /!\ Work in Progress, do not use.
/// </summary>
/// <param name="lx"></param>
/// <param name="ly"></param>
/// <param name="lz"></param>
    public void UpdateChunkNeighbouringTile(int lx, int ly, int lz)
    {
        Vector3 worldPos = WorldFromLocalPosition(lx, ly, lz);

        Vector3 nTilePos = worldPos;
        if (Math.Sign(GridPosition.X) == 1)
        {
            if (lx == 0) nTilePos.X -= 1;
            if (lx == 15) nTilePos.X += 1;  
        }
        else
        {
            if (lx == 1) nTilePos.X += 1;
            if (lx == 16) nTilePos.X -= 1;  
        }

        if (Math.Sign(GridPosition.Y) == 1)
        {
            if (lx == 0) nTilePos.Y -= 1;
            if (lx == 15) nTilePos.Y += 1;  
        }
        else
        {
            if (lx == 1) nTilePos.Y += 1;
            if (lx == 16) nTilePos.Y -= 1;  
        }

        if (Math.Sign(GridPosition.Z) == 1)
        {
            if (lx == 0) nTilePos.Z -= 1;
            if (lx == 15) nTilePos.Z += 1;  
        }
        else
        {
            if (lx == 1) nTilePos.Z += 1;
            if (lx == 16) nTilePos.Z -= 1;  
        }

        Vector3 nChunkGrid = ParentWorld.ChunkGridFromWorld((int)nTilePos.X, (int)nTilePos.Y, (int)nTilePos.Z);
        if (GridPosition == nChunkGrid) return;

        Chunk nChunk = ParentWorld.GetChunkAtGrid((int)nChunkGrid.X, (int)nChunkGrid.Y, (int)nChunkGrid.Z);
        Console.WriteLine($"current chunk: {GridPosition}");
        Console.WriteLine($"neighbouring chunk: {nChunkGrid}");

        WorldTile tile = nChunk.GetTileAtWorld(nTilePos);
            if (tile == null)
        {
            nChunk.MeshBuilt = false;
        }
    }

/// <summary>
/// Sets the nearby chunks' (up, down, left, right, front, back) <b>MeshBuilt</b> flag to false causing them to rebuild their meshes 
/// in the next game update.
/// </summary>
    public void DirtyNeighbourChunks()
    {
        Chunk xpChunk = ParentWorld.GetChunkAtGrid((int)GridPosition.X + 1, (int)GridPosition.Y, (int)GridPosition.Z);
        Chunk xnChunk = ParentWorld.GetChunkAtGrid((int)GridPosition.X + -1, (int)GridPosition.Y, (int)GridPosition.Z);
        Chunk ypChunk = ParentWorld.GetChunkAtGrid((int)GridPosition.X, (int)GridPosition.Y + 1, (int)GridPosition.Z);
        Chunk ynChunk = ParentWorld.GetChunkAtGrid((int)GridPosition.X, (int)GridPosition.Y - 1, (int)GridPosition.Z);
        Chunk zpChunk = ParentWorld.GetChunkAtGrid((int)GridPosition.X, (int)GridPosition.Y, (int)GridPosition.Z + 1);
        Chunk znChunk = ParentWorld.GetChunkAtGrid((int)GridPosition.X, (int)GridPosition.Y, (int)GridPosition.Z - 1);

        if (xpChunk != null) xpChunk.MeshBuilt = false;
        if (xnChunk != null) xnChunk.MeshBuilt = false;
        if (ypChunk != null) ypChunk.MeshBuilt = false;
        if (ynChunk != null) ynChunk.MeshBuilt = false;
        if (zpChunk != null) zpChunk.MeshBuilt = false;
        if (znChunk != null) znChunk.MeshBuilt = false;
    }

/// <summary>
/// Destroys the tile at the given local position.
/// </summary>
/// <param name="lx"></param>
/// <param name="ly"></param>
/// <param name="lz"></param>
    public void DestroyTileAtLocal(int lx, int ly, int lz)
    {
        MeshBuilt = false;
        tileArray[GetIndexFromLocalPos(lx, ly, lz)] = null;
        
        DirtyNeighbourChunks();
    }

/// <summary>
/// Destroys the tile at the given local position.
/// </summary>
/// <param name="localPos"></param>
    public void DestroyTileAtLocal(Vector3 localPos)
    {
        DestroyTileAtLocal((int)localPos.X, (int)localPos.Y, (int)localPos.Z);
    }

/// <summary>
/// Checks if the chunk should draw this frame depending on the distance from the camera.
/// </summary>
/// <param name="camGridPos"></param>
/// <param name="renderDistance"></param>
/// <returns><b>true</b> if the chunk should draw, <b>false</b> otherwise.</returns>
    public bool ShouldDraw(Vector3 camGridPos, int renderDistance)
    {
        return (
            camGridPos.X >= GridPosition.X - renderDistance && camGridPos.X <= GridPosition.X + renderDistance &&
            camGridPos.Y >= GridPosition.Y - renderDistance && camGridPos.Y <= GridPosition.Y + renderDistance &&
            camGridPos.Z >= GridPosition.Z - renderDistance && camGridPos.Z <= GridPosition.Z + renderDistance
        );
    }

/// <summary>
/// Converts the given world coordinates into the chunk's local coordinates.
/// </summary>
/// <param name="wx"></param>
/// <param name="wy"></param>
/// <param name="wz"></param>
/// <returns><b>Vector3</b> of the converted coordinates.</returns>
    public Vector3 LocalFromWorldPosition(float wx, float wy, float wz)
    {
        return new Vector3(
            ((int)Math.Floor(wx)) & 15,
            ((int)Math.Floor(wy)) & 15,
            ((int)Math.Floor(wz)) & 15
        );
    }

/// <summary>
/// Converts the given world coordinates into the chunk's local coordinates.
/// </summary>
/// <param name="worldPos"></param>
/// <returns><b>Vector3</b> of the converted coordinates.</returns>
    public Vector3 LocalFromWorldPosition(Vector3 worldPos)
    {
        return new Vector3(
            ((int)Math.Floor(worldPos.X)) & 15,
            ((int)Math.Floor(worldPos.Y)) & 15,
            ((int)Math.Floor(worldPos.Z)) & 15
        );
    }

/// <summary>
/// Converts the given local coordinates of the chunk into world coordinates.
/// </summary>
/// <param name="lx"></param>
/// <param name="ly"></param>
/// <param name="lz"></param>
/// <returns><b>Vector3</b> of the converted coordinates.</returns>
    public Vector3 WorldFromLocalPosition(int lx, int ly, int lz)
    {
        return new Vector3(
            GridPosition.X * 16 + lx,
            GridPosition.Y * 16 + ly,
            GridPosition.Z * 16 + lz
        );
    }

/// <summary>
/// Converts the given local coordinates of the chunk into world coordinates.
/// </summary>
/// <param name="localPos"></param>
/// <returns><b>Vector3</b> of the converted coordinates.</returns>
    public Vector3 WorldFromLocalPosition(Vector3 localPos)
    {
        return new Vector3(
            GridPosition.X * 16 + localPos.X,
            GridPosition.Y * 16 + localPos.Y,
            GridPosition.Z * 16 + localPos.Z
        );
    }

    private int GetIndexFromLocalPos(int lx, int ly, int lz)
    {
        return lx + (ly * ChunkSizeX) + (lz * ChunkSizeX * ChunkSizeY);
    }

    private int GetIndexFromLocalPos(Vector3 localPos)
    {
        return (int)localPos.X + ((int)localPos.Y * ChunkSizeX) + ((int)localPos.Z * ChunkSizeX * ChunkSizeY);
    }

    private int GetIndexFromWorldPos(int wx, int wy, int wz)
    {
        return GetIndexFromWorldPos(new Vector3(wx, wy, wz));
    }

    private int GetIndexFromWorldPos(Vector3 worldPos)
    {
        var localVec = LocalFromWorldPosition(worldPos);

        return GetIndexFromLocalPos((int)localVec.X, (int)localVec.Y, (int)localVec.Z);
    }

/// <summary>
/// Sends the given vertices and indices to the GPU by setting them to their respective buffers.
/// </summary>
/// <param name="graphicsDevice"></param>
/// <param name="vertices"></param>
/// <param name="indices"></param>
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

        VertexBuffer.SetData(vertices.ToArray());
        IndexBuffer.SetData(indices.ToArray());
    }

/// <summary>
/// Renders the chunk.
/// </summary>
/// <param name="graphicsDevice"></param>
/// <param name="camera"></param>
    public void Draw(GraphicsDevice graphicsDevice, Camera camera)
    {
        if (VertexBuffer == null || TriangleCount == 0 || IndexBuffer == null || IndexBuffer.IndexCount == 0) return;

        BasicEffect effect = EffectHandler.MainEffect;

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
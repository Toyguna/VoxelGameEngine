using System;
using System.Collections.Concurrent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameEngine3D;

public class World
{
    public readonly int TILE_REACH_DISTANCE = 20;
    public readonly float RAY_STEPSIZE = 0.05f;

    private GraphicsDevice _graphicsDevice;
    private MeshBufferPool _meshBuffer;

    private ConcurrentDictionary<Vector3, Chunk> chunks;

    public World(GraphicsDevice gd, MeshBufferPool meshBufferPool)
    {
        _graphicsDevice = gd;
        _meshBuffer = meshBufferPool;

        Initialize();
    }

    private void Initialize()
    {
        chunks = new ConcurrentDictionary<Vector3, Chunk>();
    }

/// <summary>
/// Clears the <b>chunks</b> dictionary.
/// </summary>
    public void ClearWorld()
    {
        chunks.Clear();
    }

/// <summary>
/// Adds the chunk into the <b>chunks</b> dictionary.
/// </summary>
/// <param name="chunk"></param>
    public void AddChunk(Chunk chunk)
    {
        if (chunk == null) return;
        chunks.TryAdd(chunk.GridPosition, chunk);
    }

/// <summary>
/// Gets the chunk count.
/// </summary>
/// <returns>Count of the <b>chunks</b> dictionary.</returns>
    public int GetChunkCount()
    {
        return chunks.Count;
    }

/// <summary>
/// Converts world coordinates to chunk grid coordinates.
/// </summary>
/// <param name="wx"></param>
/// <param name="wy"></param>
/// <param name="wz"></param>
/// <returns><b>Vector3</b> of the converted chunk grid coordinates.</returns>
    public Vector3 ChunkGridFromWorld(int wx, int wy, int wz)
    {
        return new Vector3(
            (int)Math.Floor(wx / 16.0f),
            (int)Math.Floor(wy / 16.0f),
            (int)Math.Floor(wz / 16.0f)
        );
    }

/// <summary>
/// Gets the chunk at the given grid coordinates.
/// </summary>
/// <param name="cx"></param>
/// <param name="cy"></param>
/// <param name="cz"></param>
/// <returns><b>Chunk</b> at the grid coordinates, <b>null</b> if no chunk exists.</returns>
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

/// <summary>
/// Gets the chunk at the given world coordinates.
/// </summary>
/// <param name="wx"></param>
/// <param name="wy"></param>
/// <param name="wz"></param>
/// <returns><b>Chunk</b> at the world coordinates, <b>null</b> if no chunk exists.</returns>
    public Chunk GetChunkAtWorld(int wx, int wy, int wz)
    {
        Vector3 chunkGridPos = ChunkGridFromWorld(wx, wy, wz);

        Chunk chunk_out;
        chunks.TryGetValue(chunkGridPos, out chunk_out);

        return chunk_out; 
    }

/// <summary>
/// Gets the tile at the given world coordinates.
/// </summary>
/// <param name="worldpos"></param>
/// <returns><b>WorldTile</b> at the world coordinates, <b>null</b> if no tile exists.</returns>
    public WorldTile GetTileAtWorld(Vector3 worldpos)
    {
        return GetTileAtWorld(
            (int)Math.Floor(worldpos.X), (int)Math.Floor(worldpos.Y), (int)Math.Floor(worldpos.Z)
        );
    }

/// <summary>
/// Gets the tile at the given world coordinates.
/// </summary>
/// <param name="wx"></param>
/// <param name="wy"></param>
/// <param name="wz"></param>
/// <returns><b>WorldTile</b> at the world coordinates, <b>null</b> if no tile exists.</returns>
    public WorldTile GetTileAtWorld(int wx, int wy, int wz)
    {
        Chunk chunk = GetChunkAtWorld(wx, wy, wz);
        if (chunk == null) return null;

        return chunk.GetTileAtWorld(wx, wy, wz);
    }

/// <summary>
/// Destroys the tile at the given world coordinates.
/// </summary>
/// <param name="worldPos"></param>
/// <returns><b>true</b> on success, <b>false</b> otherwise.</returns>
    public bool DestroyTileAtWorld(Vector3 worldPos)
    {
        return DestroyTileAtWorld((int)Math.Floor(worldPos.X), (int)Math.Floor(worldPos.Y), (int)Math.Floor(worldPos.Z));
    }

/// <summary>
/// Destroys the tile at the given world coordinates.
/// </summary>
/// <param name="worldPos"></param>
/// <returns><b>true</b> on success, <b>false</b> otherwise.</returns>
    public bool DestroyTileAtWorld(int wx, int wy, int wz)
    {
        Chunk chunk = GetChunkAtWorld(wx, wy, wz);
        if (chunk == null) return false;

        chunk.DestroyTileAtLocal(chunk.LocalFromWorldPosition(wx, wy, wz));
        UpdateChunkMesh(chunk);

        return true;
    }

    public bool PlaceTileAtWorld(int wx, int wy, int wz, WorldTile tile)
    {
        Chunk chunk = GetChunkAtWorld(wx, wy, wz);
        if (chunk == null) return false;

        chunk.SetTileAtWorld(wx, wy, wz, tile);
        UpdateChunkMesh(chunk);

        return true;
    }

/// <summary>
/// Gets the <b>chunks</b> dictionary.
/// </summary>
/// <returns><b>ConcurrentDictionary</b> chunks.</returns>
    public ConcurrentDictionary<Vector3, Chunk> GetChunks()
    {
        return chunks;
    }

/// <summary>
/// Renders the world.
/// </summary>
/// <param name="camera"></param>
    public void Draw(Camera camera)
    {
        foreach (var item in chunks)
        {
            Chunk chunk;
            chunks.TryGetValue(item.Key, out chunk);
            if (chunk == null) continue;

            if (!chunk.ShouldDraw(camera.GetChunkGridPosition(), camera.RenderDistance)) continue;

            chunk.Draw(_graphicsDevice, camera);
        }

        DrawTileOutline(camera);
    }

/// <summary>
/// Draws an outline around where the mouse cursor is on.
/// </summary>
/// <param name="camera"></param>
    public void DrawTileOutline(Camera camera)
    {
        Vector3? targetPos = MouseToWorldPosition(Mouse.GetState().Position, camera);
        if (targetPos == null) return;

        WorldTile tile = GetTileAtWorld((Vector3)targetPos);
        if (tile == null) return;

        var effect = EffectHandler.LineEffect;

        Matrix worldMatrix = Matrix.CreateScale(1.008f) * 
            Matrix.CreateTranslation(tile.Position.X - 0.004f, tile.Position.Y - 0.004f, tile.Position.Z - 0.004f);

        effect.World = worldMatrix;
        effect.View = camera.ViewMatrix;
        effect.Projection = camera.ProjectionMatrix;
        
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();

            _graphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.LineList,
                EffectHandler.outlineVertices, 
                0, 8,                    
                EffectHandler.boxIndices, 
                0, EffectHandler.boxIndices.Length / 2  
            );
        }
    }

/// <summary>
/// Updates the given chunk's mesh.
/// </summary>
/// <param name="chunk"></param>
    public void UpdateChunkMesh(Chunk chunk)
    {
        if (chunk == null) return;

        _meshBuffer.Clear();
        ChunkMeshBuilder.BuildMesh(chunk, chunk.ParentWorld, _meshBuffer.VertexScratchpad, _meshBuffer.IndexScratchpad);
        chunk.SendToGPU(_graphicsDevice, _meshBuffer.VertexScratchpad, _meshBuffer.IndexScratchpad);

        chunk.MeshBuilt = true;
    }

/// <summary>
/// Unprojects the mouse cursor's position and casts a ray to a maximum distance of the constant <b>TILE_REACH_DISTANCE</b>,
/// returning the voxel's world coordinates on hit.
/// </summary>
/// <param name="mousePos"></param>
/// <param name="camera"></param>
/// <returns><b>Vector3</b> of the hit voxel's world position, <b>null</b> if no voxel was hit.</returns>
    public Vector3? MouseToWorldPosition(Point mousePos, Camera camera)
    {
        Vector3 nearPoint = _graphicsDevice.Viewport.Unproject(
            new Vector3(mousePos.X, mousePos.Y, 0f),
            camera.ProjectionMatrix,
            camera.ViewMatrix,
            camera.WorldMatrix
        );

        Vector3 farPoint = _graphicsDevice.Viewport.Unproject(
            new Vector3(mousePos.X, mousePos.Y, 1f),
            camera.ProjectionMatrix,
            camera.ViewMatrix,
            camera.WorldMatrix
        );

        Vector3 rayDir = farPoint - nearPoint;
        rayDir.Normalize();

        Ray ray = new Ray(nearPoint, rayDir);

        Vector3 prevPos;

        for (float distance = 0; distance < TILE_REACH_DISTANCE; distance += RAY_STEPSIZE)
        {
            prevPos = ray.Position;

            ray.Position += rayDir * RAY_STEPSIZE;

            if (GetTileAtWorld(ray.Position) != null)
            {
                return ray.Position;
            }
        }

        return null;
    }

    public Vector3? MouseToTileFace(Point mousePos, Camera camera)
    {
        Vector3 nearPoint = _graphicsDevice.Viewport.Unproject(
            new Vector3(mousePos.X, mousePos.Y, 0f),
            camera.ProjectionMatrix,
            camera.ViewMatrix,
            camera.WorldMatrix
        );

        Vector3 farPoint = _graphicsDevice.Viewport.Unproject(
            new Vector3(mousePos.X, mousePos.Y, 1f),
            camera.ProjectionMatrix,
            camera.ViewMatrix,
            camera.WorldMatrix
        );

        Vector3 rayDir = farPoint - nearPoint;
        rayDir.Normalize();

        Ray ray = new Ray(nearPoint, rayDir);

        Vector3 prevPos;

        for (float distance = 0; distance < TILE_REACH_DISTANCE; distance += RAY_STEPSIZE)
        {
            prevPos = ray.Position;

            ray.Position += rayDir * RAY_STEPSIZE;

            if (GetTileAtWorld(ray.Position) != null)
            {
                Vector3 hitTile = new Vector3(
                    (int)Math.Floor(ray.Position.X),
                    (int)Math.Floor(ray.Position.Y),
                    (int)Math.Floor(ray.Position.Z)
                );

                Vector3 prevTile = new Vector3(
                    (int)Math.Floor(prevPos.X),
                    (int)Math.Floor(prevPos.Y),
                    (int)Math.Floor(prevPos.Z)
                );

                Vector3 rawNormal = new Vector3(
                    prevTile.X - hitTile.X,
                    prevTile.Y - hitTile.Y,
                    prevTile.Z - hitTile.Z
                );

                Vector3 faceNormal = Vector3.Zero;

                float absX = Math.Abs(rawNormal.X);
                float absY = Math.Abs(rawNormal.Y);
                float absZ = Math.Abs(rawNormal.Z);

                if (absX > absY && absX > absZ)
                {
                    faceNormal.X = Math.Sign(rawNormal.X);
                }
                else if (absY > absX && absY > absZ)
                {
                    faceNormal.Y = Math.Sign(rawNormal.Y);
                }
                else if (absZ > absX && absZ > absY)
                {
                    faceNormal.Z = Math.Sign(rawNormal.Z);
                }
                else
                {
                    if (Math.Abs(rayDir.X) > Math.Abs(rayDir.Y) && Math.Abs(rayDir.X) > Math.Abs(rayDir.Z))
                        faceNormal.X = -Math.Sign(rayDir.X);
                    else if (Math.Abs(rayDir.Y) > Math.Abs(rayDir.X) && Math.Abs(rayDir.Y) > Math.Abs(rayDir.Z))
                        faceNormal.Y = -Math.Sign(rayDir.Y);
                    else
                        faceNormal.Z = -Math.Sign(rayDir.Z);
                }

                return faceNormal;
            }
        }

        return null;
    }

    public RaycastHit MouseToWorldRay(Point mousePos, Camera camera)
    {
        Vector3 nearPoint = _graphicsDevice.Viewport.Unproject(
            new Vector3(mousePos.X, mousePos.Y, 0f),
            camera.ProjectionMatrix,
            camera.ViewMatrix,
            camera.WorldMatrix
        );

        Vector3 farPoint = _graphicsDevice.Viewport.Unproject(
            new Vector3(mousePos.X, mousePos.Y, 1f),
            camera.ProjectionMatrix,
            camera.ViewMatrix,
            camera.WorldMatrix
        );

        Vector3 rayDir = farPoint - nearPoint;
        rayDir.Normalize();

        Ray ray = new Ray(nearPoint, rayDir);

        Vector3 prevPos;

        for (float distance = 0; distance < TILE_REACH_DISTANCE; distance += RAY_STEPSIZE)
        {
            prevPos = ray.Position;

            ray.Position += rayDir * RAY_STEPSIZE;

            if (GetTileAtWorld(ray.Position) != null)
            {
                Vector3 hitTile = new Vector3(
                    (int)Math.Floor(ray.Position.X),
                    (int)Math.Floor(ray.Position.Y),
                    (int)Math.Floor(ray.Position.Z)
                );

                Vector3 prevTile = new Vector3(
                    (int)Math.Floor(prevPos.X),
                    (int)Math.Floor(prevPos.Y),
                    (int)Math.Floor(prevPos.Z)
                );

                Vector3 rawNormal = new Vector3(
                    prevTile.X - hitTile.X,
                    prevTile.Y - hitTile.Y,
                    prevTile.Z - hitTile.Z
                );

                Vector3 faceNormal = Vector3.Zero;

                float absX = Math.Abs(rawNormal.X);
                float absY = Math.Abs(rawNormal.Y);
                float absZ = Math.Abs(rawNormal.Z);

                if (absX > absY && absX > absZ)
                {
                    faceNormal.X = Math.Sign(rawNormal.X);
                }
                else if (absY > absX && absY > absZ)
                {
                    faceNormal.Y = Math.Sign(rawNormal.Y);
                }
                else if (absZ > absX && absZ > absY)
                {
                    faceNormal.Z = Math.Sign(rawNormal.Z);
                }
                else
                {
                    if (Math.Abs(rayDir.X) > Math.Abs(rayDir.Y) && Math.Abs(rayDir.X) > Math.Abs(rayDir.Z))
                        faceNormal.X = -Math.Sign(rayDir.X);
                    else if (Math.Abs(rayDir.Y) > Math.Abs(rayDir.X) && Math.Abs(rayDir.Y) > Math.Abs(rayDir.Z))
                        faceNormal.Y = -Math.Sign(rayDir.Y);
                    else
                        faceNormal.Z = -Math.Sign(rayDir.Z);
                }

                return new RaycastHit(ray.Position, faceNormal);
            }
        }

        return null;
    }

/// <summary>
/// Updates the world per game time.
/// </summary>
/// <param name="gameTime"></param>
/// <param name="camera"></param>
    public void Update(GameTime gameTime, Camera camera)
    {
        foreach (Chunk chunk in chunks.Values)
        {
            if (!chunk.ShouldDraw(camera.GetChunkGridPosition(), camera.RenderDistance)) continue;

            if (chunk.GenerationCompleted && !chunk.MeshBuilt)
            {
                UpdateChunkMesh(chunk);
            }
        }
    }
}
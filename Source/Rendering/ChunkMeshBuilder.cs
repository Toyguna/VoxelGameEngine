using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine3D;

public class ChunkMeshBuilder
{
    private static readonly int[,] directions = new int[,]
    {
        {  0,  1,  0 }, // 0: Top (+Y)
        {  0, -1,  0 }, // 1: Bottom (-Y)
        { -1,  0,  0 }, // 2: Left (-X)
        {  1,  0,  0 }, // 3: Right (+X)
        {  0,  0, -1 }, // 4: Front (-Z)
        {  0,  0,  1 }  // 5: Back (+Z)
    };

    public static void BuildMesh(Chunk chunk, World world, List<VertexPositionColorNormalTexture> vertices, List<int> indices)
    {
        vertices.Clear();
        indices.Clear();
        int vertexOffset = 0;

        for (int x = 0; x < chunk.ChunkSizeX; x++)
        {
            for (int y = 0; y < chunk.ChunkSizeY; y++)
            {
                for(int z = 0; z < chunk.ChunkSizeZ; z++)
                {
                    WorldTile tile = chunk.GetTileAtLocal(x, y, z);
                    if (tile == null) continue;
                    
                    for (int faceIndex = 0; faceIndex < 6; faceIndex++)
                    {
                        int nx = x + directions[faceIndex, 0];
                        int ny = y + directions[faceIndex, 1];
                        int nz = z + directions[faceIndex, 2];

                        bool shouldRender = false;

                        // check if neighbouring face is on the adjacent chunk, if so get the tile and check if null
                        if (nx < 0 || nx >= chunk.ChunkSizeX ||
                            ny < 0 || ny >= chunk.ChunkSizeY ||
                            nz < 0 || nz >= chunk.ChunkSizeZ )
                        {
                            Vector3 worldPos = new Vector3(
                                chunk.WorldPosition.X + nx,
                                chunk.WorldPosition.Y + ny,
                                chunk.WorldPosition.Z + nz
                            );

                            if (world.GetTileAtWorld(worldPos) == null) shouldRender = true;
                        }
                        else
                        {
                            if (chunk.GetTileAtLocal(nx, ny, nz) == null) shouldRender = true;
                        }

                        if (shouldRender)
                        {
                            int textureIndex = tile.TileData.TextureIndex;

                            if (tile.TileData.Model.ModelType == TileModelType.TEXTURE_PER_FACE)
                            {
                                textureIndex = TextureHandler.GetIndexFromTextureName(tile.TileData.Model.TextureFaces[faceIndex]);
                            }

                            AddFace(x, y, z, faceIndex, textureIndex, tile.TileColor, vertices, indices, ref vertexOffset);
                        }
                    }
                }
            }
        }
        
    }

    private static void AddFace(
        int x, int y, int z, 
        int face, int textureIndex, Color faceColor,
        List<VertexPositionColorNormalTexture> vertices, 
        List<int> indices, 
        ref int offset)
    {
        // face vertex positions relative to tile origin
        Vector3 v0 = Vector3.Zero; 
        Vector3 v1 = Vector3.Zero;
        Vector3 v2 = Vector3.Zero;
        Vector3 v3 = Vector3.Zero;
        Vector3 normal = Vector3.Up;

        switch(face)
        {
            case 0: // Top (+Y)
                v0 = new Vector3(x,     y + 1, z);
                v1 = new Vector3(x + 1, y + 1, z);
                v2 = new Vector3(x + 1, y + 1, z + 1);
                v3 = new Vector3(x,     y + 1, z + 1);
                normal = Vector3.Up;
                break;
            case 1: // Bottom (-Y)
                v0 = new Vector3(x,     y,     z + 1);
                v1 = new Vector3(x + 1, y,     z + 1);
                v2 = new Vector3(x + 1, y,     z);
                v3 = new Vector3(x,     y,     z);
                normal = Vector3.Down;
                break;
            case 2: // Left (-X)
                v0 = new Vector3(x,     y + 1, z);
                v1 = new Vector3(x,     y + 1, z + 1);
                v2 = new Vector3(x,     y,     z + 1);
                v3 = new Vector3(x,     y,     z);
                normal = Vector3.Left;
                break;
            case 3: // Right (+X)
                v0 = new Vector3(x + 1, y + 1, z + 1);
                v1 = new Vector3(x + 1, y + 1, z);
                v2 = new Vector3(x + 1, y,     z);
                v3 = new Vector3(x + 1, y,     z + 1);
                normal = Vector3.Right;
                break;
            case 4: // Front (-Z)
                v0 = new Vector3(x + 1, y + 1, z);
                v1 = new Vector3(x,     y + 1, z);
                v2 = new Vector3(x,     y,     z);
                v3 = new Vector3(x + 1, y,     z);
                normal = Vector3.Forward;
                break; 
            case 5: // Back (+Z)
                v0 = new Vector3(x,     y + 1, z + 1);
                v1 = new Vector3(x + 1, y + 1, z + 1);
                v2 = new Vector3(x + 1, y,     z + 1);
                v3 = new Vector3(x,     y,     z + 1);
                normal = Vector3.Backward;
                break;
        }

        // texture
        float uSize = 1.0f / TextureHandler.AtlasGridWidth;
        float vSize = 1.0f / TextureHandler.AtlasGridHeight;
        Vector2 gridPos = TextureHandler.ConvertIndexToGrid(textureIndex);

        float uStart = gridPos.X * uSize;
        float vStart = gridPos.Y * vSize;

        Vector2 uv0 = new Vector2(uStart, vStart);
        Vector2 uv1 = new Vector2(uStart + uSize, vStart);
        Vector2 uv2 = new Vector2(uStart + uSize, vStart + vSize);
        Vector2 uv3 = new Vector2(uStart, vStart + vSize);

        vertices.Add(new VertexPositionColorNormalTexture(v0, faceColor, normal, uv0));
        vertices.Add(new VertexPositionColorNormalTexture(v1, faceColor, normal, uv1));
        vertices.Add(new VertexPositionColorNormalTexture(v2, faceColor, normal, uv2));
        vertices.Add(new VertexPositionColorNormalTexture(v3, faceColor, normal, uv3));

        indices.Add(offset + 0);
        indices.Add(offset + 1);
        indices.Add(offset + 2);
        
        indices.Add(offset + 2);
        indices.Add(offset + 3);
        indices.Add(offset + 0);

        offset += 4;
    }
}
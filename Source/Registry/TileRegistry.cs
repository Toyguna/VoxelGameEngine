using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GameEngine3D;

public static class TileRegistry
{
    private static Dictionary<uint, BasicTileData> tileDict = new Dictionary<uint, BasicTileData>();

    public static readonly BasicTileData WATER = RegisterTile(new Identifier("game", "water"));
    public static readonly BasicTileData SAND = RegisterTile(new Identifier("game", "sand"));
    public static readonly BasicTileData STONE = RegisterTile(new Identifier("game", "stone"));
    public static readonly BasicTileData DIRT = RegisterTile(new Identifier("game", "dirt"));
    public static readonly BasicTileData GRASS = RegisterTile(new Identifier("game", "grass"),
        new TileModel(TileModelType.TEXTURE_PER_FACE)
        .SetFaceTextures(
            [TileModelFace.FRONT, TileModelFace.BACK, TileModelFace.RIGHT, TileModelFace.LEFT, TileModelFace.TOP, TileModelFace.BOTTOM],
            ["grass_side", "grass_side", "grass_side", "grass_side", "grass_top", "grass_bottom"]
        )
    );

    public static void Initialize()
    {
        
    }
    
    public static BasicTileData RegisterTile(Identifier id, TileModel model = null)
    {
        BasicTileData tileData = new BasicTileData(id, id.Path, model);

        int index = IdRegistry.GetIndexOfId(id);

        if (index == -1) return null;

        tileDict.Add((uint)index, tileData);

        return tileData;
    }

    public static void AddTexturesToTiles()
    {
        foreach (BasicTileData tileData in tileDict.Values)
        {
            tileData.GetTextureIndex();
        }
    }
}
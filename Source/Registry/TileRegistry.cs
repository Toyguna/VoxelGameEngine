using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GameEngine3D;

public static class TileRegistry
{
    private static Dictionary<uint, TileData> tileDict = new Dictionary<uint, TileData>();

    public static readonly TileData WATER = RegisterTile(new Identifier("game", "water"));
    public static readonly TileData SAND = RegisterTile(new Identifier("game", "sand"));
    public static readonly TileData STONE = RegisterTile(new Identifier("game", "stone"));
    public static readonly TileData DIRT = RegisterTile(new Identifier("game", "dirt"));
    public static readonly TileData GRASS = RegisterTile(new Identifier("game", "grass"),
        new TileModel(TileModelType.TEXTURE_PER_FACE)
        .SetFaceTextures(
            [TileModelFace.FRONT, TileModelFace.BACK, TileModelFace.RIGHT, TileModelFace.LEFT, TileModelFace.TOP, TileModelFace.BOTTOM],
            ["grass_side", "grass_side", "grass_side", "grass_side", "grass_top", "grass_bottom"]
        )
    );

    public static void Initialize()
    {
        
    }
    
    public static TileData RegisterTile(Identifier id, TileModel model = null)
    {
        TileData tileData = new TileData(id, id.Path, model);

        int index = IdRegistry.GetIndexOfId(id);

        if (index == -1) return null;

        tileDict.Add((uint)index, tileData);

        return tileData;
    }

    public static void AddTexturesToTiles()
    {
        foreach (TileData tileData in tileDict.Values)
        {
            tileData.GetTextureIndex();
        }
    }
}
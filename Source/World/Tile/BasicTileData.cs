using System;
using Microsoft.Xna.Framework;

namespace GameEngine3D;

public class BasicTileData
{
    public Identifier TileId { get; private set; }
    public string TextureName { get; private set; }
    public int TextureIndex { get; private set; }
    public TileModel Model { get; private set; }

    public BasicTileData(Identifier identifier, string textureName, TileModel tileModel)
    {
        TileId = identifier;
        TextureName = textureName;

        Model = tileModel ?? new TileModel(
            TileModelType.SAME_TEXTURE
        );
    }

    public void GetTextureIndex()
    {
        TextureIndex = TextureHandler.GetIndexFromTextureName(TextureName);
    }
}
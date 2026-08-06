#nullable enable

using Microsoft.Xna.Framework;

namespace GameEngine3D;

public class TileModel
{
    public string[] TextureFaces { get; private set; } = new string[6];
    public TileModelType ModelType { get; private set; }
    public Color TileColor { get; private set; }

    public TileModel(TileModelType modelType, Color tileColor)
    {
        ModelType = modelType;
        TileColor = tileColor;
    }

    public TileModel(TileModelType modelType)
    {
        ModelType = modelType;
        TileColor = Color.White;
    }

    public TileModel SetFaceTexture(TileModelFace face, string textureIndex)
    {
        TextureFaces[(int)face] = textureIndex;

        return this;
    }

    public TileModel SetFaceTextures(TileModelFace[] faces, string[] textureIndices)
    {
        for (int i = 0; i < faces.Length; i++)
        {
            TextureFaces[(int)faces[i]] = textureIndices[i];  
        }

        return this;
    }
}
using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine3D;

public static class TextureHandler
{
    public static readonly int AtlasGridWidth = 16;
    public static readonly int AtlasGridHeight = 16;
    public static readonly int TextureWidth = 16;
    public static readonly int TextureHeight = 16;
    public static readonly int BytePerPixel = 4;
    
    private static readonly string TEXTURE_PATH = "../../../Content/Textures";
    private static readonly bool ATLAS_DEBUG = true;

    public static Texture2D TextureAtlas { get; private set; }

    private static GraphicsDevice _graphicsDevice;
    private static string[] TextureIds = new string[AtlasGridWidth * AtlasGridHeight];


    public static void Initialize(GraphicsDevice gd)
    {
        _graphicsDevice = gd;
    }

    public static void LoadContent()
    {
        CreateAtlas();
    }

/// <summary>
/// Gets the texture data in bytes from the atlas.
/// </summary>
/// <param name="index"></param>
/// <returns><b>byte[]</b> of the texture's byte data.</returns>
    public static byte[] GetTextureFromAtlas(int index)
    {
        byte[] texture = new byte[TextureWidth * TextureHeight * BytePerPixel];
        TextureAtlas.GetData(texture, index, texture.Length);

        return texture;
    }

/// <summary>
/// Converts the given grid coordinates on the atlas to the texture index.
/// </summary>
/// <param name="x"></param>
/// <param name="y"></param>
/// <returns><b>int</b> index of the texture in the grid coordinates.</returns>
    public static int ConvertGridToIndex(int x, int y)
    {
        return x + (y * TextureHeight);
    }

/// <summary>
/// Converts the given texture index to grid coordinates on the atlas.
/// </summary>
/// <param name="index"></param>
/// <returns><b>Vector2</b> grid coordinates of the given index.</returns>
    public static Vector2 ConvertIndexToGrid(int index)
    {
        return new Vector2(
            index % AtlasGridWidth,
            index / AtlasGridHeight
        );
    }

/// <summary>
/// Gets the texture name of the given texture index.
/// </summary>
/// <param name="index"></param>
/// <returns><b>string</b> texture name of given index.</returns>
    public static string GetTextureNameFromIndex(int index)
    {
        return TextureIds[index];
    }

/// <summary>
/// Gets the texture index of the given texture name.
/// </summary>
/// <param name="name"></param>
/// <returns><b>int</b> index of given texture name.</returns>
    public static int GetIndexFromTextureName(string name)
    {
        for (int x = 0; x < AtlasGridWidth; x++)
        {
            for (int y = 0; y < AtlasGridHeight; y++)
            {
                int index = ConvertGridToIndex(x, y);
                string texture = TextureIds[index];
                if (texture == name)
                {
                    return index;
                }
            }
        }

        return -1;
    }

/// <summary>
/// Creates a texture atlas from the found texture files.
/// Exports the atlas if the flag <b>ATLAS_DEBUG</b> is set to true.
/// </summary>
    public static void CreateAtlas()
    {
        Console.WriteLine("[TextureHandler] Creating Texture Atlas:");
        TextureAtlas = new Texture2D(_graphicsDevice, TextureWidth * AtlasGridWidth, TextureHeight * AtlasGridHeight);

        Color[] blankPixels = new Color[TextureAtlas.Width * TextureAtlas.Height];
        TextureAtlas.SetData(blankPixels);

        string[] files = Directory.GetFiles("../../../Content/Textures");
        
        Console.WriteLine($"[TextureHandler] - Found {files.Length} files.");

        int gridx = 0;
        int gridy = 0;
    
        Console.WriteLine("[TextureHandler] - Adding textures to Texture Atlas...");
        foreach (string file in files)
        {
            string name = Path.GetFileNameWithoutExtension(file);
            string nameExt = Path.GetFileName(file);
            Texture2D texture2d = Texture2D.FromFile(_graphicsDevice, $"{TEXTURE_PATH}/{nameExt}");
            byte[] textureData = new byte[TextureWidth * TextureHeight * BytePerPixel];
            texture2d.GetData(textureData);

            int posx = gridx * TextureWidth;
            int posy = gridy * TextureHeight;
            Rectangle rect = new Rectangle(posx, posy, TextureWidth, TextureHeight);

            TextureAtlas.SetData(0, rect, textureData, 0, textureData.Length);
            Console.WriteLine($"[TextureHandler]    | Added texture {name} to position ({posx}, {posy}).");

            TextureIds[ConvertGridToIndex(gridx, gridy)] = name;

            gridx++;
            if (gridx >= AtlasGridWidth)
            {
                gridx = 0;
                gridy++;
                
                if (gridy >= AtlasGridHeight)
                {
                    Console.WriteLine("[TextureHandler] /!\\ Texture index has surpassed height of the Texture Atlas, stopping...");
                    break;
                }
            }
        }
        Console.WriteLine("[TextureHandler] Texture Atlas created.");

        if (ATLAS_DEBUG)
        {
            Console.WriteLine("[TextureHandler] Exporting Texture Atlas...");
            using (Stream stream = File.OpenWrite("D:/Coding/MonoGame/Projects/GameEngine3D/Resources/TextureAtlas/TextureAtlas.png"))
            {
                TextureAtlas.SaveAsPng(stream, TextureAtlas.Width, TextureAtlas.Height);

                stream.Close();
            }
            Console.WriteLine("[TextureHandler] Texture Atlas saved.");
        }
    }
}
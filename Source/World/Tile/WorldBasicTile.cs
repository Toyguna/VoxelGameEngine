using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine3D;

public class WorldBasicTile
{
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public Vector3 Scale { get; set; } = Vector3.One;

    public Color TileColor { get; set;} = Color.White;

    public BasicTileData TileData { get; private set; }

    public WorldBasicTile(BasicTileData tileData)
    {
        TileData = tileData;

        Initialize();
    }

    public WorldBasicTile(BasicTileData tileData, Vector3 position)
    {
        TileData = tileData;
        Position = position;
        
        Initialize();
    }

    public WorldBasicTile(BasicTileData tileData, Vector3 position, Vector3 scale)
    {
        TileData = tileData;
        Position = position;
        Scale = scale;
        
        Initialize();
    }

    private void Initialize()
    {
        TileColor = TileData.Model.TileColor;
    }
}
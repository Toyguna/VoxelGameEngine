using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine3D;

public class Entity
{
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }

    public EntityData Data { get; private set; }
    
    public World CurrentWorld { get; set; }

    public Entity(Vector3 position, EntityData entityData, World world)
    {
        Position = position;
        Data = entityData;
        CurrentWorld = world;
    }

    public virtual void Update(GameTime gameTime)
    {
        
    }

    public virtual void Draw(GraphicsDevice graphicsDevice, Camera camera)
    {
        
    }
}
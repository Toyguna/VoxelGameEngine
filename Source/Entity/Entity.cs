using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine3D;

public class Entity
{
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }

    public EntityData Data { get; private set; }

    public Entity(Vector3 position, EntityData entityData)
    {
        Position = position;
        Data = entityData;
    }

    public void Draw(GraphicsDevice graphicsDevice, Camera camera)
    {
        
    }
}
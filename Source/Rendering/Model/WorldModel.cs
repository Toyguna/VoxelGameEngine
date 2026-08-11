using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine3D;

public class WorldModel
{
    public Vector3 Position = new Vector3(0, 5, 0);
    public Vector3 Rotation;
    public Vector3 Scale;

    public Model Model;

    public WorldModel(Model model)
    {
        Model = model;
    }

    public void Draw(Camera camera)
    {
        foreach (ModelMesh mesh in Model.Meshes)
        {
            foreach (BasicEffect effect in mesh.Effects)
            {
                effect.World = 
                Matrix.CreateRotationX(Rotation.X) *
                Matrix.CreateRotationY(Rotation.Y) *
                Matrix.CreateRotationZ(Rotation.Z) * 
                Matrix.CreateTranslation(Position);
                effect.View = camera.ViewMatrix;
                effect.Projection = camera.ProjectionMatrix;
            }

            mesh.Draw();
        }
    }
}
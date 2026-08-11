using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine3D;

public static class ModelHandler
{
    public static Model TestRig;
    public static Model Wow;

    private static ContentManager _contentManager;

    public static void Initialize(ContentManager cm)
    {
        _contentManager = cm;
    }

    public static void LoadModels()
    {
        TestRig = _contentManager.Load<Model>("Models/test_rig");
        Wow = _contentManager.Load<Model>("Models/wow");
    }

    public static void Draw(Camera camera)
    {
        
    }
}
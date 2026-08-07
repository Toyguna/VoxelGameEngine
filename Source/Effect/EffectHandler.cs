using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine3D;

public static class EffectHandler
{
    public static BasicEffect MainEffect;
    public static BasicEffect LineEffect;

    private static GraphicsDevice _graphicsDevice;

    public static VertexPositionColor[] outlineVertices = {
        new VertexPositionColor(new Vector3(0, 0, 0), Color.Black),
        new VertexPositionColor(new Vector3(1, 0, 0), Color.Black),
        new VertexPositionColor(new Vector3(1, 0, 1), Color.Black),
        new VertexPositionColor(new Vector3(0, 0, 1), Color.Black),
        new VertexPositionColor(new Vector3(0, 1, 0), Color.Black),
        new VertexPositionColor(new Vector3(1, 1, 0), Color.Black),
        new VertexPositionColor(new Vector3(1, 1, 1), Color.Black),
        new VertexPositionColor(new Vector3(0, 1, 1), Color.Black)
    };
    public static int[] boxIndices =
    {
        0,1, 1,2, 2,3, 3,0,
        4,5, 5,6, 6,7, 7,4, 
        0,4, 1,5, 2,6, 3,7
    };

    public static void Initialize(GraphicsDevice gd)
    {
        _graphicsDevice = gd;

        MainEffect = new BasicEffect(_graphicsDevice)
        {
            VertexColorEnabled = true,
            LightingEnabled = true
        };
        
        MainEffect.DirectionalLight0.Enabled = true;
        MainEffect.DirectionalLight0.DiffuseColor = new Vector3(1f, 1f, 1f);
        MainEffect.DirectionalLight0.Direction = new Vector3(-1.0f, -1.0f, -1.0f);

        MainEffect.DirectionalLight1.Enabled = true;
        MainEffect.DirectionalLight1.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);
        MainEffect.DirectionalLight1.Direction = new Vector3(1.0f, 1.0f, 1.0f);

        LineEffect = new BasicEffect(_graphicsDevice)
        {
            VertexColorEnabled = true,
            LightingEnabled = false,
            TextureEnabled = false
        };
    }

}
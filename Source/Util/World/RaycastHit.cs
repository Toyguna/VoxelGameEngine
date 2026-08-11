using Microsoft.Xna.Framework;

namespace GameEngine3D;

public class RaycastHit
{
    public Vector3 HitPos { get; private set; }
    public Vector3 HitNormal { get; private set; }

    public RaycastHit(Vector3 hitPos, Vector3 hitNormal)
    {
        HitPos = hitPos;
        HitNormal = hitNormal;
    }
}
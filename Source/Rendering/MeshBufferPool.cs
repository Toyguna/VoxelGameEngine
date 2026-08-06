using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine3D;

public class MeshBufferPool
{
    public readonly List<VertexPositionColorNormalTexture> VertexScratchpad = new List<VertexPositionColorNormalTexture>(16384);
    public readonly List<int> IndexScratchpad = new List<int>(24576);

    public void Clear()
    {
        VertexScratchpad.Clear();
        IndexScratchpad.Clear();
    }
}
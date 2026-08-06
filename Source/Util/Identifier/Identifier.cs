namespace GameEngine3D;

public class Identifier
{
    public string Namespace { get; private set; }
    public string Path { get; private set; }
    public uint Index { get; private set; }

    public Identifier(string namesp, string path)
    {
        Namespace = namesp;
        Path = path;
        Index = IdRegistry.CreateTileIndex(this);
    }
}
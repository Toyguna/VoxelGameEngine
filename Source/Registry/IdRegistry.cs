using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine3D;

public static class IdRegistry
{
    private static uint _lastIndex = 0;
    private static Identifier _emptyId;
    private static Dictionary<uint, Identifier> idDict;


    public static void Initialize()
    {
        idDict = new Dictionary<uint, Identifier>();
        _emptyId = new Identifier("", "");
    }

    public static uint CreateTileIndex(Identifier id)
    {
       uint index = _lastIndex;
       _lastIndex++;

        if (!idDict.ContainsValue(id))
        {
            idDict.Add(index, id);
        }

       return index;
    }

    public static Identifier EmptyIdentifier()
    {
        return _emptyId;
    }

    public static int GetIndexOfId(Identifier id)
    {
        foreach (var item in idDict)
        {
            if (item.Value == id)
            {
                return (int)item.Value.Index;
            }
        }

        return -1;
    }

    public static Identifier GetIdOfIndex(uint index)
    {
        Identifier id;
        idDict.TryGetValue(index, out id);

        return id;
    }
}
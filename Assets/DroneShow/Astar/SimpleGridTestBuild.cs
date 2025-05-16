using UnityEngine;

public static class SimpleGridTestBuilder
{
    public static void BuildSimpleTestGrid(ChunkedGrid grid)
    {
        int width = 40;
        int height = 6;
        int depth = 40;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    Vector3Int cell = new Vector3Int(x, y, z);
                    Node node = grid.GetNode(cell); // Automatically creates the chunk and node
                    node.Walkable = true;
                }
            }
        }

        Debug.Log($"[BuildSimpleTestGrid] Created {width * height * depth} walkable nodes.");
    }
}

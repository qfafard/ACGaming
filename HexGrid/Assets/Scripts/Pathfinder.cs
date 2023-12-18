using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using VInspector;

public class Pathfinder : MonoBehaviour
{
    private Vector2Int[,] _grid;
    public HexGridLayout hexGridLayout;
    public Vector2Int first;
    public Vector2Int last;
    [SerializeField] private float timeToReset = 2f;
    public void Awake()
    {
        hexGridLayout = GetComponent<HexGridLayout>();
        _grid = new Vector2Int[hexGridLayout.GetGridSize().x, hexGridLayout.GetGridSize().y];
    }

    [Button]
    private void TestPathFinder()
    {
        var path = FindPath(first, last);
        try
        {
            Debug.Log($"Path length: {path.Count}");
        } catch (Exception e)
        {
            Debug.Log("No path found");
            return;
        }
        ColorPath(path);
        StartCoroutine(ResetTiles(path, timeToReset));
    }
    
    private void ColorPath(List<Vector2Int> path)
    {

        foreach (var x in path)
        {
            var foundGO = hexGridLayout.GetTileFromCoordinate(x);
            if (foundGO == null)
            {
                continue;
            }
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            {
                color = Color.green
            };
            foundGO.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }
    }
    
    private IEnumerator ResetTiles(List<Vector2Int> path, float delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (var x in path)
        {
            var foundGO = hexGridLayout.GetTileFromCoordinate(x);
            var hexRenderer = foundGO.GetComponent<HexRenderer>();
            
            //var tileBiome = hexGridLayout.GetBiomes().Find(biome => hexRenderer.height <= biome.maxHeight);
            
            //foundGO.GetComponent<MeshRenderer>().sharedMaterial = tileBiome.mat;
        }
    }

    private List<Vector2Int> GetHexNeighbors(Vector2Int hex)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        var q = hex.x;
        var r = hex.y;
        
        var isEven = hex.x % 2 == 0;
        
        if(isEven)
        {
            //Top
            neighbors.Add(new Vector2Int(q, r - 1));
            
            //Top right
            neighbors.Add(new Vector2Int(q + 1, r - 1));
            
            //Bottom right
            neighbors.Add(new Vector2Int(q + 1, r));
            
            //Bottom
            neighbors.Add(new Vector2Int(q, r + 1));
            
            //Bottom left
            neighbors.Add(new Vector2Int(q - 1, r));
            
            //Top left
            neighbors.Add(new Vector2Int(q - 1, r - 1));
        }
        else
        {
            //Top
            neighbors.Add(new Vector2Int(q, r - 1));
            
            //Top right
            neighbors.Add(new Vector2Int(q + 1, r));
            
            //Bottom right
            neighbors.Add(new Vector2Int(q + 1, r + 1));
            
            //Bottom
            neighbors.Add(new Vector2Int(q, r + 1));
            
            //Bottom left
            neighbors.Add(new Vector2Int(q - 1, r + 1));
            
            //Top left
            neighbors.Add(new Vector2Int(q - 1, r));
        }

        return neighbors;
    }

    public int CubeDistance(Vector3 a, Vector3 b)
    {
        return (int)((Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y) + Math.Abs(a.z - b.z)) / 2);
    }

    private int OddROffsetDistance(Vector2Int a, Vector2Int b)
    {
        Vector2Int ac = new Vector2Int(a.x - (a.y - (a.y & 1)) / 2, a.y);
        Vector2Int bc = new Vector2Int(b.x - (b.y - (b.y & 1)) / 2, b.y);
        return (Math.Abs(ac.x - bc.x) + Math.Abs(ac.x + ac.y - bc.x - bc.y) + Math.Abs(ac.y - bc.y)) / 2;
    }

    private int Heuristic(Vector2Int a, Vector2Int b)
    {
        return OddROffsetDistance(a, b);
    }

    public Vector3 HexToCube(Vector2Int hex)
    {
        int x = hex.x;
        int z = hex.y;
        int y = -x - z;
        return new Vector3(x, y, z);
    }

    private List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
    {
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, float> costSoFar = new Dictionary<Vector2Int, float>();
        PriorityQueue<Vector2Int> frontier = new PriorityQueue<Vector2Int>();
        frontier.Enqueue(start, 0);

        cameFrom[start] = start;
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();

            if (current == end)
            {
                // Reconstruct the path
                List<Vector2Int> path = new List<Vector2Int>();
                while (current != start)
                {
                    path.Add(current);
                    current = cameFrom[current];
                }
                path.Add(start); // optional
                path.Reverse();
                return path;
            }

            // Check all six directions
            foreach (Vector2Int next in GetHexNeighbors(current))
            {
                if (next.x >= 0 && next.x < _grid.GetLength(0) && next.y >= 0 && next.y < _grid.GetLength(1))
                {
                    float newCost = costSoFar[current] + ((next.x != current.x && next.y != current.y) ? 1 : 1.5f); // Diagonal cost is 1, horizontal and vertical cost is 1.5
                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        int priority = (int)(10 * (newCost + Heuristic(next, end)));
                        frontier.Enqueue(next, priority);
                        cameFrom[next] = current;
                    }
                }
            }
        }

        return null; // Path not found
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HexGridLayout : MonoBehaviour
{
    [SerializeField] private Vector2Int gridSize;
    [SerializeField] protected List<Biome> biomes;

    [SerializeField] private float scale = 1f;
    [SerializeField] private string seed;

    [Header("Hex Settings")] 
    [SerializeField] private float innerSize = 1f;
    [SerializeField] private float outerSize = 1.5f;
    [SerializeField] private float height = 1f;

    public Vector2Int GetGridSize()
    {
        return gridSize;
    }
    
    virtual public void GenerateMap()
    {
        RefreshAllChildren();
        var noiseMap = GenerateNoiseMap(gridSize.x, gridSize.y, scale);
        
        for (var y = 0; y < gridSize.y; y++)
        {
            for (var x = 0; x < gridSize.x; x++)
            {
                var tile = new GameObject($"Hex {x},{y}", typeof(HexRenderer))
                {
                    transform =
                    {
                        position = GetPositionForHexFromCoordinate(new Vector2Int(x, y))
                    }
                };

                var hexRenderer = tile.GetComponent<HexRenderer>();
                hexRenderer.flatTopEdge = true;
                hexRenderer.innerSize = innerSize;
                hexRenderer.outerSize = outerSize;
                hexRenderer.height = height;  
                hexRenderer.coordinate = new Vector2Int(x, y);
                hexRenderer.DrawMesh();
                tile.transform.SetParent(transform, true);
                hexRenderer.SetMaterial(biomes[0].mat);
                
                /*var tileBiome = biomes.Find(biome => hexRenderer.height <= biome.maxHeight);

                if (tileBiome != null)
                {
                    hexRenderer.SetMaterial(tileBiome.mat);
                    hexRenderer.DrawMesh();
                    tile.transform.SetParent(transform, true);
                }
                else
                {
                    Destroy(tile);
                }*/
            }
        }
    }

    #region Unity Methods

    // private void OnEnable()
    // {
    //     GenerateMap();
    // }
    //
    // private void OnValidate()
    // {
    //     if (Application.isPlaying)
    //         GenerateMap();
    // }

    #endregion
    
    #region Helper Methods

    private void RefreshAllChildren()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public List<HexRenderer> GetHexesWithinRadiusOf(HexRenderer center, int radius)
    {
        List<HexRenderer> results = new List<HexRenderer>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        Vector2Int centerCoord = center.coordinate;
        queue.Enqueue(centerCoord);
        visited.Add(centerCoord);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            HexRenderer currentTile = GetTileFromCoordinate(current);
            if (currentTile != null)
            {
                results.Add(currentTile);
            }

            if (OddROffsetDistance(centerCoord, current) < radius)
            {
                foreach (Vector2Int neighbor in GetHexNeighbors(current))
                {
                    if (!visited.Contains(neighbor))
                    {
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                    }
                }
            }
        }

        return results;
    }
    
    private int OddROffsetDistance(Vector2Int coord1, Vector2Int coord2)
    {
        // Convert odd-r offset to cube coordinates
        int x1 = coord1.x - (coord1.y - (coord1.y & 1)) / 2;
        int z1 = coord1.y;
        int y1 = -x1 - z1;

        int x2 = coord2.x - (coord2.y - (coord2.y & 1)) / 2;
        int z2 = coord2.y;
        int y2 = -x2 - z2;

        // Calculate the distance
        return (Math.Abs(x1 - x2) + Math.Abs(y1 - y2) + Math.Abs(z1 - z2)) / 2;
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

    
    protected float[,] GenerateNoiseMap(int width, int height, float scale)
    {
        float[,] noiseMap = new float[width, height];

        // Generate offsets from the seed
        System.Random prng = new System.Random(seed.GetHashCode());
        float offsetX = prng.Next(-100000, 100000);
        float offsetY = prng.Next(-100000, 100000);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float sampleX = x / scale + offsetX;
                float sampleY = y / scale + offsetY;

                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);

                noiseMap[x, y] = perlinValue;
            }
        }

        return noiseMap;
    }
    
    public HexRenderer GetTileFromCoordinate(Vector2Int coordinate)
    {
        Vector3 targetPosition = GetPositionForHexFromCoordinate(coordinate);
        foreach (Transform child in transform)
        {
            if (child.position == targetPosition)
            {
                return child.gameObject.GetComponent<HexRenderer>();
            }
        }

        return null; // Return null if no matching tile is found
    }

    private Vector3 GetPositionForHexFromCoordinate(Vector2Int coordinate)
    {
        var column = coordinate.x;
        var row = coordinate.y;
        float width;
        float height;
        float xPosition;
        float yPosition;
        bool shouldOffset;
        float horizontalDistance;
        float verticalDistance;
        float offset;
        float size = outerSize;


        shouldOffset = (column % 2) == 0;
        width = 2f * size;
        height = Mathf.Sqrt(3) * size;

        horizontalDistance = width * 0.75f;
        verticalDistance = height;

        offset = (shouldOffset) ? height / 2 : 0;

        xPosition = (column * horizontalDistance);
        yPosition = (row * verticalDistance) - offset;


        return new Vector3(xPosition, 0, -yPosition);
    }

    #endregion
}
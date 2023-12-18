using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HexGridLayout : MonoBehaviour
{
    [SerializeField] private Vector2Int gridSize;
    [SerializeField] private List<Biome> biomes;
    
    [SerializeField] private float scale = 1f;
    [SerializeField] private string seed;
    
    [Header("Hex Settings")] 
    [SerializeField] private float innerSize = 1f;
    [SerializeField] private float outerSize = 1.5f;
    [SerializeField] private float height = 1f;
    [SerializeField] private bool flatTopEdge;

    public bool FlatTopEdge()
    {
        return(flatTopEdge);
    }
    
    public Vector2Int GetGridSize()
    {
        return gridSize;
    }
    
    public List <Biome> GetBiomes()
    {
        return biomes;
    }
    
    
    private void OnEnable()
    {
        LayoutGrid();
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
            LayoutGrid();
    }

    void RefreshAllChildren()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    float[,] GenerateNoiseMap(int width, int height, float scale)
    {
        float[,] noiseMap = new float[width, height];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float sampleX = x / scale;
                float sampleY = y / scale;

                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                
                noiseMap[x, y] = perlinValue;
            }
        }

        return noiseMap;
    }
    
    public GameObject GetTileFromCoordinate(Vector2Int coordinate)
    {
        Vector3 targetPosition = GetPositionForHexFromCoordinate(coordinate);
        foreach (Transform child in transform)
        {
            if (child.position == targetPosition)
            {
                return child.gameObject;
            }
        }
        return null; // Return null if no matching tile is found
    }
    
    void LayoutGrid()
    {
        RefreshAllChildren();
        float[,] noiseMap = GenerateNoiseMap(gridSize.x, gridSize.y, scale);
        for(var y = 0; y < gridSize.y; y++)
        {
            for(var x = 0; x < gridSize.x; x++)
            {
                GameObject tile = new GameObject($"Hex {x},{y}", typeof(HexRenderer));
                tile.transform.position = GetPositionForHexFromCoordinate(new Vector2Int(x, y));

                HexRenderer hexRenderer = tile.GetComponent<HexRenderer>();
                hexRenderer.flatTopEdge = flatTopEdge;
                hexRenderer.innerSize = innerSize;
                hexRenderer.outerSize = outerSize;
                hexRenderer.height = height * noiseMap[x, y]; // Adjust height based on noise map
                float tileHeight = hexRenderer.height; 
                // Determine the biome based on the height
                Biome tileBiome = biomes.Find(biome => tileHeight <= biome.maxHeight);

                if (tileBiome != null)
                {
                    if (tileBiome.peakHeight != 0)
                    {
                        hexRenderer.hasPeak = true;
                        tile.transform.position += Vector3.up * tileBiome.debugAddHeight;
                        hexRenderer.peakHeight = tileBiome.peakHeight;
                    }
                    hexRenderer.SetMaterial(tileBiome.mat);
                    hexRenderer.DrawMesh();

                    tile.transform.SetParent(transform, true);
                }
                else
                {
                    Destroy(tile);
                }
                
            }
        }
    }
    
    Vector3 GetPositionForHexFromCoordinate(Vector2Int coordinate)
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

        if (!flatTopEdge)
        {
            shouldOffset = (row % 2) == 0;
            width = Mathf.Sqrt(3) * size;
            height = 2f * size;

            horizontalDistance = width;
            verticalDistance = height * 0.75f;

            offset = (shouldOffset) ? width / 2 : 0;
            
            xPosition = (column * horizontalDistance) + offset;
            yPosition = row * verticalDistance;
        }
        else
        {
            shouldOffset = (column % 2) == 0;
            width = 2f * size;
            height = Mathf.Sqrt(3) * size;

            horizontalDistance = width * 0.75f;
            verticalDistance = height;

            offset = (shouldOffset) ? height / 2 : 0;
            
            xPosition = (column * horizontalDistance);
            yPosition = (row * verticalDistance) - offset;
        }

        return new Vector3(xPosition, 0, -yPosition);
    }

}
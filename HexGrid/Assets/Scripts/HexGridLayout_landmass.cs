using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;
using Random = UnityEngine.Random;

public class HexGridLayoutLandmass : HexGridLayout
{
    public int x, y, zradius;
    public int maxNumOfIslands, minNumOfIslands, minIslandSize, maxIslandSize;
    [Button]
    public override void GenerateMap()
    {
        base.GenerateMap();

        GenerateIslands();
    }

    public void Awake()
    {
        GenerateMap();
    }

    private void GenerateIslands()
    {
        int numSplats = Random.Range(minNumOfIslands, maxNumOfIslands); 
        for(int i = 0; i < numSplats; i++)
        {
            int range = Random.Range(minIslandSize, maxIslandSize);
            int y = Random.Range(range, GetGridSize().x - range);
            int x = Random.Range(range, GetGridSize().y - range);
            
            ElevateArea(x, y, range);
        }
    }

    [Button]
    public void Raise()
    {
        GenerateIslands();
    }
    
    public void ElevateArea(int q, int r, int range, float centerHeight = 1.5f)
    {
        HexRenderer center = GetTileFromCoordinate(new Vector2Int(q, r)).GetComponent<HexRenderer>();
        List<HexRenderer> tiles = GetHexesWithinRadiusOf(center, range);
        var noiseMap = GenerateNoiseMap(GetGridSize().x, GetGridSize().y, 4);

        for (var index = 0; index < tiles.Count; index++)
        {
            if (tiles[index].height < 0)
            {
                tiles[index].SetHeight(0);
            }
            var coord = tiles[index].coordinate;
            tiles[index].SetHeight(tiles[index].height + noiseMap[coord.x, coord.y]);
            tiles[index].SetMaterial(biomes[1].mat);
        }
    }
}
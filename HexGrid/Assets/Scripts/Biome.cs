using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class Biome
{
    public string name;
    public Material mat;
    [FormerlySerializedAs("lowestHeight")] public double maxHeight;
    public float peakHeight;
    public float debugAddHeight;
    public Biome(string name, Material color, double heightRange, float peakHeight, float debugAddHeight)
    {
        this.name = name;
        this.mat = color;
        this.maxHeight = heightRange;
        this.peakHeight = peakHeight;
        this.debugAddHeight = debugAddHeight;
    }
}

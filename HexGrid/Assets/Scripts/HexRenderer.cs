using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Experimental.Rendering.Universal;

public struct Face
{
    public List<Vector3> vertices { get; }
    public List<int> triangles { get; }
    public List<Vector2> uvs { get; }

    public Face(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        this.vertices = vertices;
        this.triangles = triangles;
        this.uvs = uvs;
    }
}

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class HexRenderer : MonoBehaviour
{
    private Mesh m_mesh;
    public MeshFilter m_meshFilter;
    public MeshRenderer m_meshRenderer;

    private List<Face> m_faces;

    public Material material;
    public float innerSize = 1f;
    public float outerSize = 1.5f;
    public float height = 1f;
    public bool flatTopEdge;
    public Vector2Int coordinate;

    void Awake()
    {
        m_meshFilter = GetComponent<MeshFilter>();
        m_meshRenderer = GetComponent<MeshRenderer>();

        m_mesh = new Mesh();
        m_mesh.name = "Hex";

        m_meshFilter.mesh = m_mesh;
        m_meshRenderer.sharedMaterial = material;
    }


    public void SetMaterial(Material material)
    {
        m_meshRenderer.sharedMaterial = material;
    }
    
    public void SetHeight(float newHeight)
    {
        height = newHeight;
        DrawMesh();
    }
    

    private void OnEnable()
    {
        DrawMesh();
    }

    public void OnValidate()
    {
        if (Application.isPlaying)
            DrawMesh();
    }

    public void DrawMesh()
    {
        DrawFaces();
        CombineFaces();
    }

    private void CombineFaces()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for (var i = 0; i < m_faces.Count; i++)
        {
            vertices.AddRange(m_faces[i].vertices);
            uvs.AddRange(m_faces[i].uvs);

            var vertexOffset = (4 * i);
            foreach (var tri in m_faces[i].triangles)
            {
                triangles.Add(tri + vertexOffset);
            }
        }

        m_mesh.vertices = vertices.ToArray();
        m_mesh.triangles = triangles.ToArray();
        m_mesh.uv = uvs.ToArray();
        m_mesh.RecalculateNormals();
    }

    private void DrawFaces()
    {
        m_faces = new List<Face>();

        for (int point = 0; point < 6; point++)
        {
            m_faces.Add(CreateFace(innerSize, outerSize, height / 2f, height / 2f, point));
        }

        for (int point = 0; point < 6; point++)
        {
            m_faces.Add(CreateFace(innerSize, outerSize, -height / 2f, -height / 2f, point, true));
        }

        for (int point = 0; point < 6; point++)
        {
            m_faces.Add(CreateFace(outerSize, outerSize, height / 2f, -height / 2f, point, true));
        }

        for (int point = 0; point < 6; point++)
        {
            m_faces.Add(CreateFace(innerSize, innerSize, height / 2f, -height / 2f, point));
        }
    }

    private Face CreateFace(float innerRad, float outerRad, float heightA, float heightB, int point,
        bool reverse = false)
    {
        Vector3 pointA = GetPoint(innerRad, heightB, point);
        Vector3 pointB = GetPoint(innerRad, heightB, (point < 5) ? point + 1 : 0);
        Vector3 pointC = GetPoint(outerRad, heightA, (point < 5) ? point + 1 : 0);
        Vector3 pointD = GetPoint(outerRad, heightA, point);

        List<Vector3> vertices = new List<Vector3>() { pointA, pointB, pointC, pointD };
        List<int> triangles = new List<int>() { 0, 1, 2, 2, 3, 0 };
        List<Vector2> uvs = new List<Vector2>()
            { new(0, 0), new(1, 0), new(1, 1), new(0, 1) };
        
        if (reverse)
        {
            vertices.Reverse();
        }

        return new Face(vertices, triangles, uvs);
    }

    protected Vector3 GetPoint(float size, float height, int index)
    {
        float angle_deg = flatTopEdge ? 60 * index : (60 * index) - 30;
        float angle_rad = Mathf.PI / 180 * angle_deg;
        return new Vector3(size * Mathf.Cos(angle_rad), height, size * Mathf.Sin(angle_rad));
    }

    public static float Distance(HexRenderer centerHex, HexRenderer tile)
    {
        return
            Mathf.Max(
                Mathf.Abs(centerHex.coordinate.x - tile.coordinate.x),
                Mathf.Abs(centerHex.coordinate.y - tile.coordinate.y),
                Mathf.Abs(centerHex.coordinate.sqrMagnitude - tile.coordinate.sqrMagnitude)
            );

    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] int Width = 50;
    [SerializeField] int Length = 50;

    [SerializeField] float perlinFrequencyX = 0.1f;
    [SerializeField] float perlinFrequencyZ = 0.1f;
    [SerializeField] float perlinNoiseStrength = 7f;

    enum TerrainStyle
    {
        TerrainColour,
        BlackToWhite,
        WhiteToBlack,
    }

    [SerializeField] TerrainStyle terrainStyle;

    Gradient TerrainGradient;
    Gradient BlackToWhiteGradient;
    Gradient WhiteToBlackGradient;

    Vector3[] vertices;
    int[] tris;
    Vector2[] uvs;
    Color[] colours;

    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    NavMeshSurface navMeshSurface;
    MeshCollider meshCollider;

    float minHeight = 0;
    float maxHeight = 0;

    void Start()
    {
        mesh = new Mesh();
        mesh.name = "Procedural Terrain";
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        meshRenderer = GetComponent<MeshRenderer>();
        Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
        meshRenderer.material = mat;

        navMeshSurface = GetComponent<NavMeshSurface>();

        meshCollider = GetComponent<MeshCollider>();

        #region Terrain Gradient Code

        GradientColorKey[] colorKeyTerrain = new GradientColorKey[8];
        colorKeyTerrain[0].color = new Color(0, 0.086f, 0.35f, 1);
        colorKeyTerrain[0].time = 0.0f;

        colorKeyTerrain[1].color = new Color(0, 0.135f, 1, 1);
        colorKeyTerrain[1].time = 0.082f;

        colorKeyTerrain[2].color = new Color(0, 0.735f, 1, 1);
        colorKeyTerrain[2].time = 0.26f;

        colorKeyTerrain[3].color = new Color(1, 0.9f, 0.5f, 1);
        colorKeyTerrain[3].time = 0.31f;

        colorKeyTerrain[4].color = new Color(0.06f, 0.31f, 0, 1);
        colorKeyTerrain[4].time = 0.45f;

        colorKeyTerrain[5].color = new Color(0.31f, 0.195f, 0.11f, 1);
        colorKeyTerrain[5].time = 0.59f;

        colorKeyTerrain[6].color = new Color(0.41f, 0.41f, 0.41f, 1);
        colorKeyTerrain[6].time = 0.79f;

        colorKeyTerrain[7].color = new Color(1, 1, 1, 1);
        colorKeyTerrain[7].time = 1.0f;

        GradientAlphaKey[] alphaKeyTerrain = new GradientAlphaKey[2];

        alphaKeyTerrain[0].alpha = 1.0f;
        alphaKeyTerrain[0].time = 0.0f;
        alphaKeyTerrain[1].alpha = 1.0f;
        alphaKeyTerrain[1].time = 1.0f;

        TerrainGradient = new Gradient();

        TerrainGradient.SetKeys(colorKeyTerrain, alphaKeyTerrain);

        #endregion

        #region Black-To-White Gradient Code

        GradientColorKey[] colorKeyBTW = new GradientColorKey[2];

        colorKeyBTW[0].color = new Color(0, 0, 0, 1);
        colorKeyBTW[0].time = 0.0f;

        colorKeyBTW[1].color = new Color(1, 1, 1, 1);
        colorKeyBTW[1].time = 1;

        GradientAlphaKey[] alphaKeyBTW = new GradientAlphaKey[2];

        alphaKeyBTW[0].alpha = 1.0f;
        alphaKeyBTW[0].time = 0.0f;

        alphaKeyBTW[1].alpha = 1.0f;
        alphaKeyBTW[1].time = 1.0f;

        BlackToWhiteGradient = new Gradient();

        BlackToWhiteGradient.SetKeys(colorKeyBTW, alphaKeyBTW);

        #endregion

        #region White-To-Black Gradient Code

        GradientColorKey[] colorKeyWTB = new GradientColorKey[2];

        colorKeyWTB[0].color = new Color(1, 1, 1, 1);
        colorKeyWTB[0].time = 0.0f;

        colorKeyWTB[1].color = new Color(0, 0, 0, 1);
        colorKeyWTB[1].time = 1f;

        GradientAlphaKey[] alphaKeyWTB = new GradientAlphaKey[2];

        alphaKeyWTB[0].alpha = 1.0f;
        alphaKeyWTB[0].time = 0.0f;

        alphaKeyWTB[1].alpha = 1.0f;
        alphaKeyWTB[1].time = 1.0f;

        WhiteToBlackGradient= new Gradient();

        WhiteToBlackGradient.SetKeys(colorKeyWTB, alphaKeyWTB);

        #endregion

        GenerateMeshData();

        CreateTerrain();
    }

    void GenerateMeshData()
    {
        vertices = new Vector3[(Width + 1) * (Length + 1)];

        int i  = 0;

        for(int z = 0; z <= Length; z++) 
        {
            for (int x = 0; x <= Width; x++) 
            {
                float y = Mathf.PerlinNoise(x * perlinFrequencyX, z * perlinFrequencyZ) * perlinNoiseStrength;

                vertices[i] = new Vector3(x, 0, z);

                if (y > maxHeight)
                {
                    maxHeight= y;
                }
                if(y < minHeight)
                {
                    minHeight= y;
                }

                i++;
            }
        }

        tris = new int[Width * Length * 6];

        int currentTrianglePoint = 0;

        int currentVertexPoint = 0;

        for (int z = 0; z < Length; z++) 
        {
            for (int x = 0; x < Width; x++) 
            {
                tris[currentTrianglePoint + 0] = currentVertexPoint + 0;
                tris[currentTrianglePoint + 1] = currentVertexPoint + Width + 1;
                tris[currentTrianglePoint + 2] = currentVertexPoint + 1;
                tris[currentTrianglePoint + 3] = currentVertexPoint + 1;
                tris[currentTrianglePoint + 4] = currentVertexPoint + Width + 1;
                tris[currentTrianglePoint + 5] = currentVertexPoint + Width + 2;

                currentVertexPoint++;
                currentTrianglePoint += 6;
            }
            currentVertexPoint++;
        }

        uvs = new Vector2[vertices.Length];

        i = 0;
        for (int z = 0; z <= Length; z++) 
        {
            for (int x =0; x < Width; x++) 
            {
                uvs[i] = new Vector2((float)x / Width, (float)z / Length);
                i++;
            }
        }

        colours = new Color[vertices.Length];
        i = 0;
        for (int z = 0; z <= Length; z++)
        {
            for (int x = 0; x <= Width; x++)
            {
                float height = Mathf.InverseLerp(minHeight, maxHeight, vertices[i].y);

                switch (terrainStyle) 
                {
                    case TerrainStyle.TerrainColour:
                        colours[i] = TerrainGradient.Evaluate(height); 
                        break;

                    case TerrainStyle.BlackToWhite:
                        colours[i] = BlackToWhiteGradient.Evaluate(height);
                        break;

                    case TerrainStyle.WhiteToBlack:
                        colours[i] = WhiteToBlackGradient.Evaluate(height);
                        break;
                }
                i++;
            }
        }
    }

    void CreateTerrain()
    {
        mesh.Clear();

        mesh.vertices = vertices;

        mesh.triangles = tris;

        mesh.uv = uvs;

        mesh.colors = colours;

        mesh.RecalculateBounds();

        meshCollider.sharedMesh = mesh;

        navMeshSurface.BuildNavMesh();
    }
}

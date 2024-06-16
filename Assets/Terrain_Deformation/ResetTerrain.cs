using UnityEngine;

public class ResetTerrain : MonoBehaviour
{
    private Terrain terrain;
    private float[,] originalHeights;

    private void Awake()
    {
        
    }
    void Start()
    {
        terrain = GetComponent<Terrain>();
        SaveOriginalHeights();


    }

    void SaveOriginalHeights()
    {
        originalHeights = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);
    }

    private void OnDisable()
    {
        ResetTerrainToOriginal();
    }
    public void ResetTerrainToOriginal()
    {
        terrain.terrainData.SetHeights(0, 0, originalHeights);
    }
    void ResetTerrainToZeroHeight()
    {
        int resolution = terrain.terrainData.heightmapResolution;
        float[,] zeroHeights = new float[resolution, resolution];

        terrain.terrainData.SetHeights(0, 0, zeroHeights);
    }
}

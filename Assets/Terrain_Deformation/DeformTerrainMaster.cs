using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeformTerrainMaster : MonoBehaviour
{
    #region Variables

    [Header("Sphere - (CONFIG)")]
    [Tooltip("Объект взаимодействия")]
    public GameObject mySphere;
    [Tooltip("Collider")]
    public Collider mySphereCollider;
    public GranulePool granulPool;

    [Header("Sphere - System Info")]
    public float mass;

    public Vector3 centerGridSphereHeight;
    private Vector3 centerGridSphere;

    private PhysicalFootprint footprintSphere;

    [Header("Granul Parametrs")]
    public GameObject granul;


    // Terrain Properties
    private Terrain terrain;
    private Collider terrain_collider;
    public TerrainData terrain_data;
    private Vector3 terrain_size;
    private int heightmap_width;
    private int heightmap_height;
    private float[,] heightmap_data;
    private float[,] heightmap_data_constant;
    private float[,] heightmap_data_filtered;
    private float lenghtCellX;
    private float lenghtCellZ;
    Rigidbody body;

    private List<Vector2> neighbors4 = new List<Vector2>()
    {   new Vector2(0, -1),
        new Vector2(-1, 0),
        new Vector2(1, 0),
        new Vector2(0, 1)
    };
    #endregion

    void Start()
    {
        
        GranulManager.Granuled.AddListener(SetGranul);
        if (!terrain)
        {
            terrain = Terrain.activeTerrain;
            Debug.Log("[INFO] Main terrain: " + terrain.name);
        }

        terrain_collider = terrain.GetComponent<Collider>();
        body = mySphere.GetComponent<Rigidbody>();
       // Physics.IgnoreCollision(mySphereCollider, terrain_collider);
      //  DisableCollidersInChildren(mySphere, terrain_collider);

        terrain_data = terrain.terrainData;
        terrain_size = terrain_data.size;
        heightmap_width = terrain_data.heightmapResolution;
        heightmap_height = terrain_data.heightmapResolution;
        heightmap_data = terrain_data.GetHeights(0, 0, heightmap_width, heightmap_height);
        heightmap_data_constant = terrain_data.GetHeights(0, 0, heightmap_width, heightmap_height);
        heightmap_data_filtered = terrain_data.GetHeights(0, 0, heightmap_width, heightmap_height);
        footprintSphere = GetComponent<PhysicalFootprint>();

        heightmap_width = terrain_data.heightmapResolution;
        heightmap_height = terrain_data.heightmapResolution;

        lenghtCellX = TerrainSize().x / (GridSize().x - 1);
        lenghtCellZ = TerrainSize().z / (GridSize().z - 1);

        mass = mySphere.GetComponent<Rigidbody>().mass;

       
    }


    public void FixedUpdate()
    {
        footprintSphere.CallFootprint(mySphere.transform.position.x, mySphere.transform.position.z);
    }

   

    public Vector3 Get3(int x, int z)
    {
        return new Vector3(x, Get(x, z), z);
    }
    public Vector3 Get3(float x, float z)
    {
        return new Vector3(x, Get(x, z), z);
    }
    public Vector3 GetInterp3(float x, float z)
    {
        return new Vector3(x, GetInterp(x, z), z);
    }

    
    public float Get(float x, float z)
    {
        return Get((int)x, (int)z);
    }
    public float Get(int x, int z)
    {
        x = (x + heightmap_width) % heightmap_width;
        z = (z + heightmap_height) % heightmap_height;
        return heightmap_data[z, x] * terrain_data.heightmapScale.y;
    }
    


    public float Get(Vector2 loc)
    {
        int x = (int)loc.x;
        int z = (int)loc.y;

        return Get((int)x, (int)z);
    }


    public float[,] GetHeightmap()
    {
      
        return heightmap_data;
    }

    
    public float GetConstant(int x, int z)
    {
        x = (x + heightmap_width) % heightmap_width;
        z = (z + heightmap_height) % heightmap_height;
        return heightmap_data_constant[z, x] * terrain_data.heightmapScale.y;
    }
    public float GetConstant(float x, float z)
    {
        return GetConstant((int)x, (int)z);
    }

   
    public float[,] GetConstantHeightmap()
    {
        
        return heightmap_data_constant;
    }
  
    public float GetFiltered(int x, int z)
    {
        x = (x + heightmap_width) % heightmap_width;
        z = (z + heightmap_height) % heightmap_height;
        return heightmap_data_filtered[z, x] * terrain_data.heightmapScale.y;
    }
    public float GetFiltered(float x, float z)
    {
        return GetFiltered((int)x, (int)z);
    }

   
    public float[,] GetFilteredHeightmap()
    {
        // IMPORTANT: When getting a value, must be multiplied by terrain_data.heightmapScale.y!
        return heightmap_data_filtered;
    }

    public float GetInterp(float x, float z)
    {
        return terrain_data.GetInterpolatedHeight(x / heightmap_width,
                                                  z / heightmap_height);
    }
    public float GetSteepness(float x, float z)
    {
        return terrain_data.GetSteepness(x / heightmap_width,
                                         z / heightmap_height);
    }
    public Vector3 GetNormal(float x, float z)
    {
        return terrain_data.GetInterpolatedNormal(x / heightmap_width,
                                                  z / heightmap_height);
    }

    //      Setters       //
    // ================== //

    // Given one node of the heightmap, set the height
    public void Set(int x, int z, float val, GranulInfo ginf)
    {
        x = (x + heightmap_width) % heightmap_width;
        z = (z + heightmap_height) % heightmap_height;
        heightmap_data[z, x] = val / terrain_data.heightmapScale.y;
        CreateGranul(ginf);
      
    }
    public void Set(int x, int z, float val)
    {
        x = (x + heightmap_width) % heightmap_width;
        z = (z + heightmap_height) % heightmap_height;
        heightmap_data[z, x] = val / terrain_data.heightmapScale.y;
    }

    public void Set(float x, float z, float val, GranulInfo ginf)
    {
        Set((int)x, (int)z, val, ginf);
    }
    public void Set(float x, float z, float val)
    {
        Set((int)x, (int)z, val);
    }
    public void SetGranul(Vector3 pos, float radius,  GameObject granul)
    {
        granulPool.ReturnGranule(granul);
        pos = World2Grid(pos);
        Vector2 ij = new Vector2(pos.x, pos.z);
        float Y = Get(ij) + radius / 5;
        Set(ij.x, ij.y, Y);  
        for (int i = 0; i < 4; i++)
        {    
            Vector2 ii = new Vector2(ij.x, ij.y) + neighbors4[i]; 
            float Y1 = Get(ii);
            if (Y>=Y1) Set(ii.x, ii.y, Y1 + radius /5);
            else Set(ij.x, ij.y, Y);
        }
    }
    //      Sphere Methods       //
    // ========================== //
    public Vector3 GetSphereSpeed(Vector3 point)
    {
        
        //return sphereSpeed;
        return body.GetPointVelocity(point);
    }


    public void CreateGranul(GranulInfo ginf)
    {
        if (ginf.volume == 0) return;
        float radius = Mathf.Pow((3f * ginf.volume) / (4f * Mathf.PI), 1f / 3f);
        if (radius > 0.1f)
        {
            int remainder = (int)(radius / 0.1f);
            for (int i = 0; i < remainder; i++)
            {
                Vector3 new_point = ginf.inst_point + 0.1f * i * Vector3.up;
                GameObject _granul = granulPool.GetGranule(); //Instantiate(granul, new_point, Quaternion.identity);
                _granul.transform.position = new_point;
                _granul.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                var granul_cont = _granul.GetComponent<GranulCont>();
                if (granul_cont.isRock) _granul.transform.localScale *= 5f;
                granul_cont.radius = 0.1f;
                granul_cont.my_obj = mySphere;
               
            }
        }
        else
        {
            GameObject _granul = granulPool.GetGranule(); //Instantiate(granul, new_point, Quaternion.identity);
            _granul.transform.position = ginf.inst_point;
            _granul.transform.localScale = new Vector3(radius * 2f, radius * 2f, radius * 2f);
            var granul_cont = _granul.GetComponent<GranulCont>();
            _granul.GetComponent<Rigidbody>().mass = (float)PhysicalFootprint.granDensity * ginf.volume;
            granul_cont.radius = radius;
            if (granul_cont.isRock) _granul.transform.localScale *= 5f;
            granul_cont.my_obj = mySphere;
            
        }
      
    }

    //      Terrain Methods       //
    // ========================== //

    public Vector3 GetNormal(Vector2 loc)
    {
        var east = new Vector2(1, 0);
        var west = new Vector2(1, 0);
        var north = new Vector2(1, 0);
        var south = new Vector2(1, 0);

        Vector3 off = new Vector3(1, 1, 0);

        var hE = Get(loc + east);  // east
        var hW = Get(loc - west);  // west
        var hN = Get(loc + north);  // north
        var hS = Get(loc - south);  // south

        var vector = new Vector3(hW - hE, 2 * lenghtCellX, hS - hN);

        var res = Vector3.Normalize(vector);
        return res;
    }

    public float GetLenghtCellX()
    {
        return lenghtCellX;
    }

    public float GetLenghtCellZ()
    {
        return lenghtCellZ;
    }

    // Get dimensions of the heightmap grid
    public Vector3 GridSize()
    {
        return new Vector3(heightmap_width, 0.0f, heightmap_height);
    }

    // Get real dimensions of the terrain (World Space)
    public Vector3 TerrainSize()
    {
        return terrain_size;
    }

    // Get terrain data
    public TerrainData GetTerrainData()
    {
        return terrain_data;
    }

    // Convert from Grid Space to World Space
    public Vector2 Grid2World(Vector2 grid)
    {
        return new Vector2(grid.x * terrain_data.heightmapScale.x,
                           grid.y * terrain_data.heightmapScale.z);
    }
    public Vector3 Grid2World(Vector3 grid)
    {
        return new Vector3(grid.x * terrain_data.heightmapScale.x,
                           grid.y,
                           grid.z * terrain_data.heightmapScale.z);
    }

    public Vector3 Grid2World(float x, float y, float z)
    {
        return Grid2World(new Vector3(x, y, z));
    }

    public Vector3 Grid2World(float x, float z)
    {
        return Grid2World(x, 0.0f, z);
    }

    // Convert from World Space to Grid Space
    public Vector3 World2Grid(Vector3 grid)
    {
        return new Vector3(grid.x / terrain_data.heightmapScale.x,
                           grid.y,
                           grid.z / terrain_data.heightmapScale.z);
    }

    public Vector3 World2Grid(float x, float y, float z)
    {
        return World2Grid(new Vector3(x, y, z));
    }

    public Vector3 World2Grid(float x, float z)
    {
        return World2Grid(x, 0.0f, z);
    }

    // Reset to flat terrain
    public void Reset()
    {
        for (int z = 0; z < heightmap_height; z++)
        {
            for (int x = 0; x < heightmap_width; x++)
            {
                heightmap_data[z, x] = 0;
            }
        }

        Save();
    }

    // Smooth terrain
    public void AverageSmooth()
    {
        for (int z = 10; z < heightmap_height - 10; z++)
        {
            for (int x = 10; x < heightmap_width - 10; x++)
            {
                float n = 2.0f * 2 + 1.0f;
                float sum = 0;
                for (int szi = -2; szi <= 2; szi++)
                {
                    for (int sxi = -2; sxi <= 2; sxi++)
                    {
                        sum += heightmap_data[z + szi, x + sxi];
                    }
                }

                heightmap_data[z, x] = sum / (n * n);
            }
        }

        Save();
    }
    public void DisableCollidersInChildren(GameObject parent, Collider terrainCollider)
    {
        // Получаем все коллайдеры в дочерних объектах, включая потомков
        Collider[] childColliders = parent.GetComponentsInChildren<Collider>();

        // Отключаем физику коллайдеров
        foreach (Collider childCollider in childColliders)
        {
            Physics.IgnoreCollision(childCollider, terrainCollider);
        }
    }
    // Calculate Kernel
    public static float[,] CalculateKernel(int length, float sigma)
    {
        float[,] Kernel = new float[length, length];
        float sumTotal = 0f;

        int kernelRadius = length / 2;
        double distance = 0f;

        float calculatedEuler = 1.0f / (2.0f * (float)Math.PI * sigma * sigma);

        for (int idY = -kernelRadius; idY <= kernelRadius; idY++)
        {
            for (int idX = -kernelRadius; idX <= kernelRadius; idX++)
            {
                distance = ((idX * idX) + (idY * idY)) / (2 * (sigma * sigma));

                Kernel[idY + kernelRadius, idX + kernelRadius] = calculatedEuler * (float)Math.Exp(-distance);

                sumTotal += Kernel[idY + kernelRadius, idX + kernelRadius];
            }
        }

        for (int y = 0; y < length; y++)
        {
            for (int x = 0; x < length; x++)
            {
                Kernel[y, x] = Kernel[y, x] * (1.0f / sumTotal);
            }
        }

        return Kernel;
    }

    // Gaussian Filter (Custom Kernel)
    public void GaussianBlurCustom()
    {
        float[,] kernel = CalculateKernel(3, 1f);

        for (int z = 10; z < heightmap_height - 10; z++)
        {
            for (int x = 10; x < heightmap_width - 10; x++)
            {

                heightmap_data[z, x] =
                    kernel[0, 0] * heightmap_data[z - 1, x - 1]
                    + kernel[0, 1] * heightmap_data[z - 1, x]
                    + kernel[0, 2] * heightmap_data[z - 1, x + 1]
                    + kernel[1, 0] * heightmap_data[z, x - 1]
                    + kernel[1, 1] * heightmap_data[z, x]
                    + kernel[1, 2] * heightmap_data[z, x + 1]
                    + kernel[2, 0] * heightmap_data[z + 1, x - 1]
                    + kernel[2, 1] * heightmap_data[z + 1, x]
                    + kernel[2, 2] * heightmap_data[z + 1, x + 1];
            }
        }

        Save();
    }

    // Gaussian Blur 3x3
    public void GaussianBlur3()
    {
        for (int z = 10; z < heightmap_height - 10; z++)
        {
            for (int x = 10; x < heightmap_width - 10; x++)
            {

                heightmap_data[z, x] =
                    heightmap_data[z - 1, x - 1]
                    + 2 * heightmap_data[z - 1, x]
                    + 1 * heightmap_data[z - 1, x + 1]
                    + 2 * heightmap_data[z, x - 1]
                    + 4 * heightmap_data[z, x]
                    + 2 * heightmap_data[z, x + 1]
                    + 1 * heightmap_data[z + 1, x - 1]
                    + 2 * heightmap_data[z + 1, x]
                    + 1 * heightmap_data[z + 1, x + 1];

                heightmap_data[z, x] *= 1.0f / 16.0f;

            }
        }

        Save();

    }

    // Gaussian Blur 5x5
    public void GaussianBlur5()
    {
        for (int z = 10; z < heightmap_height - 10; z++)
        {
            for (int x = 10; x < heightmap_width - 10; x++)
            {

                heightmap_data[z, x] =
                    heightmap_data[z - 2, x - 2]
                    + 4 * heightmap_data[z - 2, x - 1]
                    + 6 * heightmap_data[z - 2, x]
                    + heightmap_data[z - 2, x + 2]
                    + 4 * heightmap_data[z - 2, x + 1]
                    + 4 * heightmap_data[z - 1, x + 2]
                    + 16 * heightmap_data[z - 1, x + 1]
                    + 4 * heightmap_data[z - 1, x - 2]
                    + 16 * heightmap_data[z - 1, x - 1]
                    + 24 * heightmap_data[z - 1, x]
                    + 6 * heightmap_data[z, x - 2]
                    + 24 * heightmap_data[z, x - 1]
                    + 6 * heightmap_data[z, x + 2]
                    + 24 * heightmap_data[z, x + 1]
                    + 36 * heightmap_data[z, x]
                    + heightmap_data[z + 2, x - 2]
                    + 4 * heightmap_data[z + 2, x - 1]
                    + 6 * heightmap_data[z + 2, x]
                    + heightmap_data[z + 2, x + 2]
                    + 4 * heightmap_data[z + 2, x + 1]
                    + 4 * heightmap_data[z + 1, x + 2]
                    + 16 * heightmap_data[z + 1, x + 1]
                    + 4 * heightmap_data[z + 1, x - 2]
                    + 16 * heightmap_data[z + 1, x - 1]
                    + 24 * heightmap_data[z + 1, x];

                heightmap_data[z, x] *= 1.0f / 256.0f;

            }
        }

        Save();

    }

   
    // Register changes made to the terrain
    public void Save()
    {
        terrain_data.SetHeights(0, 0, heightmap_data);
    }

    // Get and set active brushes
    private void OnCollisionEnter(Collision collision)
    {
        
    }

    private void OnCollisionStay(Collision collision)
    {
        ;
    }

    private void OnCollisionExit(Collision collision)
    {
       
    }

    private void OnTriggerEnter(Collider other)
    {
        //Physics.IgnoreCollision(mySphereCollider, terrain_collider);
    }

    private void OnTriggerStay(Collider other)
    {
        ;
    }

    private void OnTriggerExit(Collider other)
    {
        ;
    }
}

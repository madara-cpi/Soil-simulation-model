using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static UnityEngine.ParticleSystem;

public class PhysicalFootprint : MonoBehaviour
{
    #region Variables
    const double CH_C_DEG_TO_RAD = Math.PI / 180;
    const double CH_C_RAD_TO_DEG = 180 / Math.PI;


    [Header("Terrain Deformation - (SET UP)")]
    [Header("Grids - (SET UP)")]
    [Range(0, 20)] public int gridSize = 15;
    [Range(0f, 10f)] public float rayDistance = 0.01f;
    [Range(0f, 1f)] public float offsetRay = 0.5f;

    private float gridStepX;
    float gridStepY;
    float gridStepZ;


    [Header("Grids - Debug")]
    [Space(20)]
    public bool showGridDebugSphere = false;
    public bool showGridBumpDebug = false;

    [Header("Grids - Number of hits")]
    public int counterHitsSphere;
    private Dictionary<Vector2, HitRecord> hitPositionsSphere = new Dictionary<Vector2, HitRecord>();
    private Dictionary<Vector2,float> allRays = new Dictionary<Vector2, float>();
    public int neighbourCellsSphere;
    public List<Vector3> neighboursPositionsSphere = new List<Vector3>();

    [Header("Grids - Contact Area Feet-Ground")]
    public float areaCell;
    public float areaTotal = 0f;
    public float areaTotalSphere = 0f;
    public float neighbourAreaTotalSphere;


    public static Dictionary<GameObject, Vector3> totalForces = new Dictionary<GameObject, Vector3>();

    // Terrain Data
    protected DeformTerrainMaster terrain;
    private Vector3 heightmapSize;
    private Vector3 terrainSize;
    private TerrainData terrainData;
    private float[,] heightmap_data_filtered;

    // Body Properties
    private float mass;

    private Collider mySphereCollider;
    public GameObject mySphere;

    //Node data
    private NodeRecord nodeRecord;
    private Dictionary<Vector2, NodeRecord> m_grid_map = new Dictionary<Vector2, NodeRecord>();
    private List<Vector2> modifiedNodes = new List<Vector2>();
    public static double Bekker_Kphi = 2e6;
    public static double Bekker_Kc = 2e4;
    public static double Bekker_n = 1.1;
    public static double Mohr_cohesion = 20;
    public static double Mohr_mu = 20;
    public static double Janosi_shear = 0.01;
    public static double elastic_K = 5e7;
    public static double damping_R = 100;
    public static double granDensity = 1400f;

    private static float heightdatascale;

    // Bulldozing effects
    public static double flowFactor = 1.1f;
    public static bool useCohesion = false;

    #endregion
    #region Contact Patch Data
    public struct HitRecord
    {
        public Rigidbody contactable;  // pointer to hit object
        public Vector3 abs_point;      // hit point, expressed in global frame
        public int patch_id;           // index of associated patch id
    };

    public List<Vector2> neighbors4 = new List<Vector2>()
    {   new Vector2(0, -1),
        new Vector2(-1, 0),
        new Vector2(1, 0),
        new Vector2(0, 1)
    };

    public struct ContactPatchRecord
    {
        public List<Vector2> points;   // points in contact patch (in reference plane)
        public List<Vector2> nodes;    // grid nodes in the contact patch
        public double area;            // contact patch area
        public double perimeter;       // contact patch perimeter
        public double oob;             // approximate value of b
    };
    #endregion
    #region Body Properties

    public float Mass
    {
        get { return mass; }
        set { mass = value; }
    }



    public Collider MySphereCollider
    {
        get { return mySphereCollider; }
        set { mySphereCollider = value; }
    }

    public GameObject MySphere
    {
        get { return mySphere; }
        set { mySphere = value; }
    }

    #endregion
    #region Granul Parametrs
   
    Dictionary<Vector2, GranulInfo> granuls = new Dictionary<Vector2, GranulInfo>();

    #endregion
    #region Terrain Properties

    public TerrainData TerrainData
    {
        get { return terrainData; }
        set { terrainData = value; }
    }
    public Vector3 TerrainSize
    {
        get { return terrainSize; }
        set { terrainSize = value; }
    }

    public Vector3 HeightmapSize
    {
        get { return heightmapSize; }
        set { heightmapSize = value; }
    }

    public float[,] HeightMapFiltered
    {
        get { return heightmap_data_filtered; }
        set { heightmap_data_filtered = value; }

    }

    #endregion

    void Start()
    {
        Mohr_mu = Math.Tan(Mohr_mu * CH_C_DEG_TO_RAD);
        terrain = GetComponent<DeformTerrainMaster>();
        Vector3 heightmapScale = GetComponent<Terrain>().terrainData.heightmapScale;
        // Шаг сетки террейна по x
        gridStepX = heightmapScale.x;
     
        heightdatascale = heightmapScale.y;
        // Шаг сетки террейна по y (высоте)
        gridStepY = heightmapScale.y;

        // Шаг сетки террейна по z
        gridStepZ = heightmapScale.z;

        MySphere = terrain.mySphere;
        MySphereCollider = terrain.mySphereCollider;
        Mass = terrain.mass;
        mass = CalculateTotalMass(MySphere.transform);
        Debug.Log("Total mass = " + mass);
        HeightmapSize = terrain.GridSize();
        TerrainSize = terrain.TerrainSize();
        areaCell = terrain.GetLenghtCellX() * terrain.GetLenghtCellZ();
        Debug.Log( Bekker_Kphi + " " + Bekker_Kc + " " + Bekker_n + " " + Mohr_cohesion + " " + Mohr_mu + " " + Janosi_shear + " " + elastic_K + " " + damping_R + " " + granDensity);
  
    }

 

    public void CallFootprint(float x, float z)
    {     
        Vector3 gridSphere = terrain.World2Grid(x, z);
        DrawFootprint((int)gridSphere.x, (int)gridSphere.z);
        terrain.Save();
    }
    public void DrawFootprint(int x, int z)
    {
        granuls.Clear();
        counterHitsSphere = 0;
        neighbourCellsSphere = 0;

        hitPositionsSphere.Clear();

        neighboursPositionsSphere.Clear();
        allRays.Clear();

        //    Инициализация области контакта     //
        // =============================== //
        if (mySphere.GetComponent<Rigidbody>().velocity.magnitude < 0.8f) return;
        for (float zi = -gridSize; zi <= gridSize; zi++)
        {
            for (float xi = -gridSize; xi <= gridSize; xi++)
            {
                Vector3 rayGridSphere = new Vector3(x + xi, terrain.Get(x + xi, z + zi) - offsetRay, z + zi);
                Vector3 rayGridWorldSphere = terrain.Grid2World(rayGridSphere);
                RaycastHit sphereHit;
                Ray upRaySphere = new Ray(rayGridWorldSphere, Vector3.up);
                float ray_height = rayGridWorldSphere.y+offsetRay;
                Vector2 ij = new Vector2(rayGridSphere.x, rayGridSphere.z);
                if (!allRays.ContainsKey(ij)) allRays.Add(ij, ray_height);
               
                if (Physics.Raycast(upRaySphere, out sphereHit, rayDistance))
                {
                    if (!m_grid_map.ContainsKey(ij))
                    {
                        var normal = terrain.GetNormal(ij);
                        var Y = terrain.Get(x + xi, z + zi);
                        nodeRecord = new NodeRecord(Y, Y, normal);
                        m_grid_map.Add(ij, nodeRecord);
                    }
    
                    counterHitsSphere++;
                    HitRecord record = new HitRecord();
                    record.contactable = sphereHit.collider.attachedRigidbody;
                    record.abs_point = sphereHit.point;
                    record.patch_id = -1;
                    hitPositionsSphere.Add(ij, record);
                    if (showGridDebugSphere)
                        Debug.DrawRay(rayGridWorldSphere, Vector3.up * rayDistance, Color.blue);
                }
                else
                {
                    if (showGridDebugSphere)
                        Debug.DrawRay(rayGridWorldSphere, Vector3.up * rayDistance, Color.red);    
                }
            }
        }

        // Расчет пятна контакта

        int numContactPatches = 0;
        ContactPatchRecord contactPatch = new ContactPatchRecord();
        contactPatch.points = new List<Vector2>();
        contactPatch.nodes = new List<Vector2>();
        contactPatch.area = 0;
        contactPatch.perimeter = 0;
        contactPatch.oob = 0;
        if (hitPositionsSphere.Count != 0)
        {
            var first = hitPositionsSphere.First();
            HitRecord hitRecord = first.Value;
            hitRecord.patch_id = numContactPatches++;
            Vector2 recordKey = first.Key;
            hitPositionsSphere[recordKey] = hitRecord;
            contactPatch.nodes.Add(recordKey);
            Vector2 globalIj = terrain.Grid2World(recordKey);
            contactPatch.points.Add(globalIj);

            Queue<Vector2> todoQueue = new Queue<Vector2>();
            todoQueue.Enqueue(first.Key);
            while (todoQueue.TryDequeue(out Vector2 cycleTodo))
            {
                int crtPatchId = hitPositionsSphere[cycleTodo].patch_id;
                for (int k = 0; k < 4; k++)
                {
                    Vector2 nbrIj = cycleTodo + neighbors4[k];
                    if (!hitPositionsSphere.ContainsKey(nbrIj))
                    {
                        continue;
                    };
                    if (hitPositionsSphere[nbrIj].patch_id != -1)
                    {
                        continue;
                    }
                    if (hitPositionsSphere.TryGetValue(nbrIj, out HitRecord value))
                    {
                        value.patch_id = crtPatchId;
                    }

                    hitPositionsSphere[nbrIj] = value;

                    contactPatch.nodes.Add(nbrIj);
                    Vector2 globalNbrij = terrain.Grid2World(nbrIj);
                    contactPatch.points.Add(globalNbrij);
                    todoQueue.Enqueue(nbrIj);
                }
            }
        }
        //Расчет площади и периметра каждого пятна
        if (contactPatch.points.Count != 0)
        {
            List<Vector2> points = contactPatch.points;
            int n = points.Count;
            if (n > 0)
            {
                double area;
                double perimeter;
                switch (n)
                {
                    case 1:
                        contactPatch.perimeter = 0;
                        contactPatch.area = 0;
                        contactPatch.oob = 0;
                        break;
                    case 2:
                        perimeter = (points[1] - points[0]).magnitude;
                        contactPatch.perimeter = perimeter;
                        contactPatch.area = 0;
                        contactPatch.oob = 0;
                        break;
                    case 3:
                        area = 0.5 * Math.Abs(SignedArea(points[0], points[1], points[2]));
                        perimeter = (points[1] - points[0]).magnitude +
                            (points[2] - points[1]).magnitude +
                            (points[0] - points[2]).magnitude;

                        contactPatch.perimeter = perimeter;
                        contactPatch.area = area;
                        contactPatch.oob = perimeter/ (2* area);

                        if (area == 0)
                        {
                            contactPatch.oob = 0;
                        }
                        break;
                    default:
                        ComputeJarvis(points, n, out area, out perimeter);
                        area *= 0.5;
                        
                        contactPatch.perimeter = perimeter;
                        contactPatch.area = area;
                        contactPatch.oob = perimeter / (2 * area);

                        if (area == 0)
                        {
                            contactPatch.oob = 0;
                        }
                        break;

                }
            }
        }
        totalForces.Clear();
        modifiedNodes.Clear();
       // Debug.Log(hitPositionsSphere.Count);
        // Физические расчеты
        foreach (var hit in hitPositionsSphere)
        {
            GameObject hitObject = hit.Value.contactable.gameObject;

            Vector2 ij = hit.Key;
        
            NodeRecord nr = m_grid_map[ij];
            float ca = nr.normal.y;
            nr.hit_level = hit.Value.abs_point.y;
            float p_hit_offset = ca * (nr.level_initial - nr.hit_level);
           
            nr.sigma = elastic_K * (p_hit_offset - nr.sinkage_plastic);
           
            if (nr.sigma < 0)
            {
                nr.sigma = 0;
                continue;
            }
           
            Vector3 objectSpeed = terrain.GetSphereSpeed(hit.Value.abs_point); // hit.Value.contactable.GetPointVelocity(hit.Value.abs_point);
       
            modifiedNodes.Add(ij);

            // Расчет нормали и тангенциального направления
           
            Vector3 N = nr.normal;
            float Vn =  Vector3.Dot(objectSpeed, N);
            Vector3 T = -(objectSpeed - (Vn * N));
            T = Vector3.Normalize(T);
            // Обновление общего погружения и текущего уровня для компоненты связи
            nr.sinkage = p_hit_offset;
            nr.level = nr.hit_level;
          
            // Аккумулирование касательной деформации
            nr.kshear += Vector3.Dot(objectSpeed, -T) * Time.deltaTime;
          
            // Пластическая коррекция 
            if (nr.sigma > nr.sigma_yield)
            {
                // sigma - вертикальное (нормальное) напряжение
                nr.sigma = (Bekker_Kc * contactPatch.oob + Bekker_Kphi) * Math.Pow(nr.sinkage, Bekker_n);
                // предельное нормального напряжения для устойчивости почвы
                nr.sigma_yield = nr.sigma;
                double old_sinkage_plastic = nr.sinkage_plastic;
                // Рассчет пластичной деформации
                nr.sinkage_plastic = nr.sinkage - nr.sigma / elastic_K;
                // шаг пластического течения
                nr.step_plastic_flow = (nr.sinkage_plastic - old_sinkage_plastic) / Time.fixedDeltaTime;
            }

            nr.sigma += -Vn * damping_R;
            // Рассчет максимального касательного напряжения
            double tau_max = Mohr_cohesion + nr.sigma * Mohr_mu;
            // Рассчет  касательного напряжения
            nr.tau = tau_max * (1.0 - Math.Exp(-(nr.kshear / Janosi_shear)));

            // Нормальная и тангенциальная силы
            Vector3 Fn = N * areaCell * (float)nr.sigma;
            Vector3 Ft = T * areaCell * (float)nr.tau;
            Vector3 resaltF = Fn + Ft;

            if (totalForces.ContainsKey(hitObject))
            {
                totalForces[hitObject] += resaltF;
            }
            else
            {
                totalForces[hitObject] = resaltF;
            }

            // предел разрушения для создания гранул

            float threhold = (float)(nr.step_plastic_flow * nr.sinkage);
            
            
            if (threhold >= flowFactor && (objectSpeed.x>0.1f || objectSpeed.z>0.1f))
            {
                Vector2 direct_vec = TransformVector(objectSpeed.normalized);
                Vector3 creationDirection = (objectSpeed + nr.normal).normalized*areaCell*2;
                float curr_height = allRays[ij];
                float _next_height = curr_height;
                Vector2 ii = ij + direct_vec;
                allRays.TryGetValue(ii * 2, out _next_height);
                float vol = nr.sinkage * areaCell/3;
               
                if (curr_height > _next_height) _next_height = curr_height;
                Vector3 point = new Vector3(hit.Value.abs_point.x+ creationDirection.x, _next_height + 4*vol, hit.Value.abs_point.z+creationDirection.z);
                if ((hit.Value.abs_point - point).magnitude <= vol) vol = 0;
                GranulInfo ginf = new GranulInfo(vol, point,ii);
                granuls[ij] = ginf;
    
            }

            hit.Value.contactable.AddForceAtPosition(resaltF, hit.Value.abs_point, ForceMode.Force);
          
            // Обновление структур
            m_grid_map[ij] = nr;

        }

        // Обновление высот террейна
        foreach (var ij in modifiedNodes)
        {
            NodeRecord nr = m_grid_map[ij];
            GranulInfo value;
            granuls.TryGetValue(ij, out value);
            // update vertex coordinates in heightmap_data
            terrain.Set(ij.x, ij.y, nr.level, value);
            nr.normal = terrain.GetNormal(ij);
        }
    }

    public Vector2 TransformVector(Vector3 v)
    {
        float maxVal = Mathf.Max(Mathf.Abs(v.x), Mathf.Abs(v.z));  
        float sign = (v.x < 0 || v.z < 0) ? -1 : 1;  

        Vector2 result = new Vector3(
            (v.x == sign * maxVal) ? sign : 0,
            (v.z == sign * maxVal) ? sign : 0
        );  

        return result;
    }

    public double SignedArea(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p2.y - p1.y) * (p3.x - p2.x) - (p2.x - p1.x) * (p3.y - p2.y);
    }

    public void ComputeJarvis(List<Vector2> points, int n, out double area, out double perimeter)
    {
  
        bool[] added = Enumerable.Repeat(false, points.Count).ToArray();
        area = 0;
        perimeter = 0;

        int addCount = 0;

        int first = 0;
        for (int i = 1; i < points.Count; i++)
        {
            double dx = points[i].x - points[first].x;
            if (dx < 0)
                first = i;
            else if (dx == 0 && points[i].y < points[first].y)
                first = i;
        }
        addCount++;

        int crt = first;
        do
        { 
            int next = 0;
            while (next == crt || added[next])
                next = (next + 1) % n;
            for (int i = 0; i < n; i++)
            {
                if (Orientation(points[crt], points[i], points[next]) == -1)
                {
                    next = i;
                }
            }
            for (int i = 0; i < n; i++)
            {
                if (i != crt && i != next && !added[i] && Orientation(points[crt], points[i], points[next]) == 0 &&
                    InBetween(points[crt], points[i], points[next]))
                {
                    added[i] = true;
                    addCount++;
                }
            }
            added[next] = true;
            addCount++;
            perimeter += (points[next] - points[crt]).magnitude;
            area += SignedArea(points[next], points[crt], Vector2.zero);

            if (addCount > n + 1)
            {
                Debug.Log("ERROR in ConvexHull::ComputeJarvis: infinite loop");
                return;
            }

            crt = next;
        } while (crt != first);
    }

 
    public int Orientation(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        double eps = 1e-10;
        double val = SignedArea(p1, p2, p3);
        if (Math.Abs(val) < eps)
            return 0;              
        return (val > 0) ? 1 : -1;  
    }

   
    bool InBetween(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        bool a = (p2.x >= p1.x && p2.x <= p3.x) || (p2.x <= p1.x && p2.x >= p3.x);
        bool b = (p2.y >= p1.y && p2.y <= p3.y) || (p2.y <= p1.y && p2.y >= p3.y);
        return a && b;
    }
    private float CalculateTotalMass(Transform currentTransform)
    {
        float totalMass = 0f;

        Rigidbody rb = currentTransform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            totalMass += rb.mass;
        }

        foreach (Transform child in currentTransform)
        { 
            totalMass += CalculateTotalMass(child);
        }

        return totalMass;
    }
    public static double GetMu()
    {
        return Math.Tan(Mohr_mu * CH_C_DEG_TO_RAD);
    }
    public static double GetCohesion()
    {
        return Mohr_cohesion;
    }
    public static double GetShear()
    {
        return Janosi_shear;
    }
}

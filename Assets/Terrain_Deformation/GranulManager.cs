using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public abstract class GranulManager : MonoBehaviour
{
    public static UnityEvent<Vector3, float, GameObject> Granuled = new UnityEvent<Vector3,float, GameObject>();
    
    public static void SetGranuled(Vector3 pos, float height, GameObject granul)
    {
        Granuled?.Invoke(pos,height, granul);
    }
}

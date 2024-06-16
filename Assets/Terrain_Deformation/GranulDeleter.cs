using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GranulDeleter : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
     
     GranulManager.SetGranuled(other.transform.position, other.GetComponent<GranulCont>().radius*2f, other.gameObject);
    }
}

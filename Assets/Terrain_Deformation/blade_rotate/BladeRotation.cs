using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BladeRotation : MonoBehaviour
{
    public GameObject mainBody;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var mainRotate = mainBody.GetComponent<Transform>().rotation;
        this.transform.rotation = mainRotate;
    }
}

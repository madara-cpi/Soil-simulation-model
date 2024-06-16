using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GranulInfo
{
    public float volume;
    public Vector3 inst_point;
    public Vector2 next_vert;

    public GranulInfo(float v, Vector3 ip, Vector2 next)
    {
        this.volume = v;
        this.inst_point = ip;
        this.next_vert = next;
    }
}

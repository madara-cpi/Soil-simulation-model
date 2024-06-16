using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct NodeRecord
{
    public float level_initial;      // initial node level (relative to SCM frame)
    public float level;              // current node level (relative to SCM frame)
    public float hit_level;          // ray hit level (relative to SCM frame)
    public Vector3 normal;         // normal of undeformed terrain (in SCM frame)
    public float sinkage;            // along local normal direction
    public double sinkage_plastic;    // along local normal direction
    public double sinkage_elastic;    // along local normal direction
    public double sigma;              // along local normal direction
    public double sigma_yield;        // along local normal direction
    public double kshear;             // along local tangent direction
    public double tau;                // along local tangent direction
    public bool erosion;              // for bulldozing
    public double massremainder;      // for bulldozing
    public double step_plastic_flow;  // for bulldozing
                                      //public NodeRecord() : this(0, 0, default(Vector3)) { }
 
    public NodeRecord(float init_level, float level, Vector3 n)
    {
        this.level_initial = init_level;
        this.level = level;
        this.hit_level = 1e9f;
        this.normal = n;
        this.sinkage = init_level - level;
        this.sinkage_plastic = 0;
        this.sinkage_elastic = 0;
        this.sigma = 0;
        this.sigma_yield = 0;
        this.kshear = 0;
        this.tau = 0;
        this.erosion = false;
        this.massremainder = 0;
        this.step_plastic_flow = 0;
        
    }
}








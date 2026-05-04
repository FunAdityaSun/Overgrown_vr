using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AIAgentConfig : ScriptableObject
{
    public Transform destination;
    public Animator animator;
    public AIStateID newState;
    public float wanderRadius;
    public float minDistance;
    public float maxDistance;
    public float avoidAngle;
    public LayerMask occlusionLayers;
   
}

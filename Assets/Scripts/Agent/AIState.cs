using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum AIStateID { 
    Wander,
    Plant,
    PlantWander
}

public interface AIState
{
    AIStateID GetID();
    void Enter(AIAgent agent);
    void Update(AIAgent agent, float deltaTime);
    void Exit(AIAgent agent);
}

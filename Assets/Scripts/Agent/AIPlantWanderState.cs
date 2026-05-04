using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Fusion;

public class AIPlantWanderState : AIState
{
    private NavMeshAgent _agent;

    public void Enter(AIAgent agent)
    {
        _agent = agent.gameObject.GetComponent<NavMeshAgent>();
        //_agent.destination = FindRandomPath(agent);
    }

    public void Exit(AIAgent agent)
    {

    }

    public AIStateID GetID()
    {
        return AIStateID.PlantWander;
    }

    public void Update(AIAgent agent, float deltaTime)
    {
        if (!agent.enabled)
        {
            return;
        }

        if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            //_agent.destination = FindRandomPath(agent);
        }
        
    }

    private Vector3 FindRandomPath(AIAgent agent)
    {
        for (int i = 0; i < 7; i++)
        {
            Vector3 destination = new Vector3(agent.gameObject.transform.position.x + Random.Range(-1, 1), agent.gameObject.transform.position.y, agent.gameObject.transform.position.z + Random.Range(-1, 1));
            RaycastHit hitInfo = new RaycastHit();

            if (!Physics.Raycast(agent.gameObject.transform.position, destination, out hitInfo, destination.magnitude, agent.config.occlusionLayers))
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(destination, out hit, 2.0f, NavMesh.AllAreas))
                {
                    return destination;
                }
            }
        }
       return agent.gameObject.transform.position;
    }
}

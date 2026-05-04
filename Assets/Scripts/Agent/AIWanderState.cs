using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Fusion;

public class AIWanderState : AIState
{
    private NavMeshAgent _agent;
    private AISensor _sensor;
    private float _scanTimer;

    public void Enter(AIAgent agent)
    {
        _agent = agent.gameObject.GetComponent<NavMeshAgent>();
        _sensor = agent.gameObject.GetComponent<AISensor>();
        _agent.destination = FindRandomPath(agent);
    }

    public void Exit(AIAgent agent)
    {

    }

    public AIStateID GetID()
    {
        return AIStateID.Wander;
    }

    public void Update(AIAgent agent, float deltaTime)
    {
        if (!agent.enabled)
        {
            return;
        }

        if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            _agent.destination = FindRandomPath(agent);
        }

        _scanTimer -= deltaTime;

        if (_scanTimer < 0)
        {
            _scanTimer = 0.1f;
            _sensor.Scan();
            GameObject[] player = _sensor.Filter(new GameObject[1], "Player");
            if (player[0] != null)
            {
                Debug.Log("Saw Player");
                agent.ChangeState(AIStateID.Plant);
            }
        }
        
    }

    private Vector3 FindRandomPath(AIAgent agent)
    {
        for (int i = 0; i < 7; i++)
        {
            Vector3 destination = new Vector3(agent.gameObject.transform.position.x + Random.Range(-agent.config.wanderRadius, agent.config.wanderRadius), agent.gameObject.transform.position.y, agent.gameObject.transform.position.z + Random.Range(-agent.config.wanderRadius, agent.config.wanderRadius));
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

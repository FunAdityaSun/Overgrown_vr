using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIWaitPlantState : AIState
{
    private NavMeshAgent _agent;
    private CustomerNPC _customerNPC;

    public void Enter(AIAgent agent)
    {
        _agent = agent.gameObject.GetComponent<NavMeshAgent>();

        _customerNPC = agent.gameObject.GetComponentInChildren<CustomerNPC>();

        _customerNPC.Order();

        GameObject desti = GameObject.FindWithTag("Table");
        if (desti != null)
        {
            Vector3 destiCenter = desti.transform.position;
            Vector2 randomCircle = Random.insideUnitCircle * 1.5f;
            Vector3 randomDesti = new Vector3(destiCenter.x + randomCircle.x, destiCenter.y, destiCenter.z + randomCircle.y);
            _agent.destination = randomDesti;
        }
        // _agent.destination = GameObject.FindWithTag("Table").transform.position;
    }

    public void Exit(AIAgent agent)
    {

    }

    public AIStateID GetID()
    {
        return AIStateID.Plant;
    }

    public void Update(AIAgent agent, float deltaTime)
    {
        if (!agent.enabled)
        {
            return;
        }

        if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            agent.ChangeState(AIStateID.PlantWander);
        }
        
    }
}

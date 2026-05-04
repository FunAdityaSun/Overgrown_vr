using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Fusion;

public class AIAgent : NetworkBehaviour
{
    public AIStateMachine stateMachine;
    public AIStateID initialState;
    public AIAgentConfig config;

   
    // Start is called before the first frame update
    void Start()
    {
        stateMachine = new AIStateMachine(this);
        stateMachine.RegisterState(new AIWanderState());
        stateMachine.RegisterState(new AIWaitPlantState());
        stateMachine.RegisterState(new AIPlantWanderState());
        stateMachine.ChangeState(initialState);
    }

    // Update is called once per frame
    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
        {
            return;
        }
        stateMachine.Update(Runner.DeltaTime);
    }

 
    public void ChangeState(AIStateID newState)
    {
        stateMachine.ChangeState(newState);
    }
    
}

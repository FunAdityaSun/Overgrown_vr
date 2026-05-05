using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AIManager : NetworkBehaviour
{
    [SerializeField]
    private NetworkPrefabRef aiPrefab;

    [Networked] public int NPCsToWin { get; set; }
    // Dictionary of spawned user prefabs, to destroy them on disconnection
    private Dictionary<int, NetworkObject> _spawnedUsers = new Dictionary<int, NetworkObject>();

    public int FirstSpawnTime = 10;
    public int AISpawnTime = 10;

    [SerializeField] private GameObject winCanvasPrefab;
    [SerializeField] private GameObject loseCanvasPrefab;

    public override void Spawned()
    {
        if (!HasStateAuthority)
        {
            return;
        }
        var task = SpawnAI();
    }

    async Task SpawnAI()
    {
        if (aiPrefab != null)
        {
            for (int i = 0; i < NPCsToWin; i++)
            {
                if (i == 0)
                {
                    await Task.Delay(FirstSpawnTime*1000);
                }
                else
                {
                    await Task.Delay(AISpawnTime * 1000);
                }
                Vector3 spawnPosition = new Vector3(0, 1f, -2);

                NetworkObject networkPlayerObject = NetworkManager.Instance.Runner.Spawn(aiPrefab, spawnPosition, Quaternion.identity);

                _spawnedUsers.Add(i, networkPlayerObject);               
            }
        }
    }

    public void Despawn(NetworkObject obj)
    {
        NPCsToWin--;
        if (NPCsToWin <= 0)
        {
            RPC_Win();
        }
        NetworkManager.Instance.Runner.Despawn(obj);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_Win()
    {
        RaycastScript[] objs = GameObject.FindObjectsByType<RaycastScript>(FindObjectsSortMode.None);
        foreach (RaycastScript obj in objs)
        {
            GameObject can = Instantiate(winCanvasPrefab, obj.gameObject.transform);
            can.transform.position = obj.gameObject.transform.position + obj.gameObject.transform.forward * 0.25f;
            can.transform.rotation = obj.gameObject.transform.rotation;
        }
        var task = GoMainMenu();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Lose()
    {
        RaycastScript[] objs = GameObject.FindObjectsByType<RaycastScript>(FindObjectsSortMode.None);
        foreach(RaycastScript obj in objs)
        {
            GameObject can =Instantiate(loseCanvasPrefab, obj.gameObject.transform);
            can.transform.position = obj.gameObject.transform.position + obj.gameObject.transform.forward * 0.25f;
            can.transform.rotation = obj.gameObject.transform.rotation;
        }
        
        var task = GoMainMenu();
    }

    async Task GoMainMenu()
    {
        GameObject.FindAnyObjectByType<RaycastScript>().FreezePlayer(true);
        await Task.Delay(5000);

        if (Runner != null)
        {
            await Runner.Shutdown();
        }

        if (NetworkManager.Instance != null)
        {
            Destroy(NetworkManager.Instance.gameObject);
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(0);

        while (!asyncLoad.isDone)
        {
            await Task.Yield();
        }
    }

}
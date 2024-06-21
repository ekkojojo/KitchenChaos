using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlatesCounter : BaseCounter
{
    [SerializeField] KitchenObjectSO plateKitchenObject;

    public event EventHandler OnPlateSpawn;
    public event EventHandler OnPlateRemove;

    float spawnPlateTimer;
    float spawnPlateTimerMax = 4f;
    int plateAmount;
    int plateAmountMax = 4;
    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        spawnPlateTimer += Time.deltaTime;
        if(spawnPlateTimer > spawnPlateTimerMax)
        {
            spawnPlateTimer = 0;
            if (KitchenGameManager.Instance.IsGamePlaying() && plateAmount < plateAmountMax)
            {
                SpawnPlateServerRpc();
            }
            
        }
    }
    [ServerRpc]
    void SpawnPlateServerRpc()
    {
        SpawnPlateClientRpc();
    }
    [ClientRpc]
    void SpawnPlateClientRpc()
    {
        plateAmount++;

        OnPlateSpawn?.Invoke(this, EventArgs.Empty);
    }
    public override void Interact(Player player)
    {
        if (!player.hasKitchenObject())
        {
            if(plateAmount > 0)
            {
                KitchenObject.SpwanKitchenObject(plateKitchenObject, player);

                InteractLogicServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void InteractLogicServerRpc()
    {
        InteractLogicClientRpc();
    }
    [ClientRpc]
    void InteractLogicClientRpc()
    {
        plateAmount--;

        OnPlateRemove?.Invoke(this, EventArgs.Empty);
    }
}

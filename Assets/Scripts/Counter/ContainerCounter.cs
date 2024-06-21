using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class ContainerCounter : BaseCounter
{
    [SerializeField] KitchenObjectSO kitchenObjectOS;

    public event EventHandler OnPlayerGrabObject;
    public override void Interact(Player player)
    {
        if(!player.hasKitchenObject()) 
        {
            KitchenObject.SpwanKitchenObject(kitchenObjectOS, player);

            InteractLogicServerRpc();
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
        OnPlayerGrabObject?.Invoke(this, EventArgs.Empty);
    }
}

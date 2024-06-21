using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BaseCounter : NetworkBehaviour, IKitchenObjectParent
{
    public static event EventHandler OnAnyObjectPlacedHere;
    public static void ResetStaticData()
    {
        OnAnyObjectPlacedHere = null;
    }
    [SerializeField] Transform spawnPosition;

    KitchenObject kitchenObject;
    public virtual void Interact(Player player)
    {
        Debug.Log("interact");
    }

    public virtual void InteractAlternative(Player player)
    {
        Debug.Log("interact_F");
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;

        if(kitchenObject != null )
        {
            OnAnyObjectPlacedHere?.Invoke(this, EventArgs.Empty);
        }
    }
    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    public Transform GetSpawnPosition()
    {
        return spawnPosition;
    }

    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }

    public bool hasKitchenObject()
    {
        return kitchenObject != null;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static StoveCounter;
using static UnityEngine.CullingGroup;

public class CuttingCounter : BaseCounter,IHasProgressBar
{
    public static event EventHandler OnAnyCut;
    new public static void ResetStaticData()
    {
        OnAnyCut = null;
    }
    public event EventHandler<IHasProgressBar.OnProgressChangeEventArgs> OnProgressChange;

    public event EventHandler OnCut; 

    [SerializeField] CuttingRecipeSO[] CuttingRecipeArray;

    int cutProgress = 0;
    public override void Interact(Player player)
    {
        if (player.hasKitchenObject())
        {
            if (!hasKitchenObject())
            {
                if (hasRecipeWithInput(player.GetKitchenObject().getKitchenObjectSO()))
                {
                    KitchenObject kitchenObject = player.GetKitchenObject();
                    kitchenObject.SetKitchenObjectParent(this);

                    InteractLogicPlaceObjectOnCounterServerRpc();
                }   
            }
            else
            {
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().getKitchenObjectSO()))
                    {
                        //GetKitchenObject().DestroySelf();
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                    }
                }
            }
        }
        else
        {
            if (hasKitchenObject()) 
            {
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void InteractLogicPlaceObjectOnCounterServerRpc()
    {
        InteractLogicPlaceObjectOnCounterClientRpc();
    }

    [ClientRpc]
    void InteractLogicPlaceObjectOnCounterClientRpc()
    {
        cutProgress = 0;

        OnProgressChange?.Invoke(this, new IHasProgressBar.OnProgressChangeEventArgs
        {
            progress = 0
        });
    }

    public override void InteractAlternative(Player player)
    {
        if (hasKitchenObject() && hasRecipeWithInput(GetKitchenObject().getKitchenObjectSO()))
        {
            CutObjectServerRpc();
            testCuttingProgressServerRpc();
        }
    }

    [ServerRpc(RequireOwnership =false)]
    void CutObjectServerRpc()
    {
        CutObjectClientRpc();
    }

    [ClientRpc]
    void CutObjectClientRpc()
    {
        cutProgress++;

        OnCut?.Invoke(this, EventArgs.Empty);
        OnAnyCut?.Invoke(this, EventArgs.Empty);

        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().getKitchenObjectSO());

        OnProgressChange?.Invoke(this, new IHasProgressBar.OnProgressChangeEventArgs
        {
            progress = (float)cutProgress / cuttingRecipeSO.cuttingProgressMax
        });

        //testCuttingProgressServerRpc();
    }

    [ServerRpc(RequireOwnership =false)]
    void testCuttingProgressServerRpc()
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().getKitchenObjectSO());
        if (cutProgress >= cuttingRecipeSO.cuttingProgressMax)
        {
            KitchenObjectSO kitchenObjectSO = GetOutputForInput(GetKitchenObject().getKitchenObjectSO());

            //GetKitchenObject().DestroySelf();
            KitchenObject.DestroyKitchenObject(GetKitchenObject());

            KitchenObject.SpwanKitchenObject(kitchenObjectSO, this);

        }
    }

    bool hasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        CuttingRecipeSO cuttingRecipeSO1 = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
        return cuttingRecipeSO1 != null;
    }

    KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO)
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
        if(cuttingRecipeSO != null)
        {
            return cuttingRecipeSO.output;
        }
        else
        {
            return null;
        }
    }

    CuttingRecipeSO GetCuttingRecipeSOWithInput(KitchenObjectSO kitchenObjectSO)
    {
        foreach (CuttingRecipeSO cuttingRecipeSO in CuttingRecipeArray)
        {
            if (cuttingRecipeSO.input == kitchenObjectSO)
            {
                return cuttingRecipeSO;
            }
        }
        return null;
    }
}

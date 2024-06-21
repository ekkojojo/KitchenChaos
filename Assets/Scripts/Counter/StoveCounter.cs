using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static CuttingCounter;

public class StoveCounter : BaseCounter,IHasProgressBar
{
    public event EventHandler<IHasProgressBar.OnProgressChangeEventArgs> OnProgressChange;

    public event EventHandler<OnStateChangeEventArgs> OnStateChanged;
    public class OnStateChangeEventArgs : EventArgs
    {
        public FryState State;
    }
    public enum FryState
    {
        Idle,
        Frying,
        Fried,
        Burned
    }

    [SerializeField] FryingRecipeSO[] FryingRecipeSOArray;
    [SerializeField] BurningRecipeSO[] BurningRecipeSOArray;
    FryingRecipeSO FryingRecipeSO;
    BurningRecipeSO BurningRecipeSO;
    NetworkVariable<FryState> currentState=new(FryState.Idle);
    NetworkVariable<float> fryingTimer = new(0f);
    NetworkVariable<float> burningTimer = new(0f);

    public override void OnNetworkSpawn()
    {
        fryingTimer.OnValueChanged += FryingTimer_OnValueChanged;
        burningTimer.OnValueChanged += BurningTimer_OnValueChanged;
        currentState.OnValueChanged += currentState_OnValueChanged;
    }

    void FryingTimer_OnValueChanged(float preValue,float newValue)
    {
        float fryingTimerMax = FryingRecipeSO != null ? FryingRecipeSO.fryingTimerMax : 1f;

        OnProgressChange?.Invoke(this, new IHasProgressBar.OnProgressChangeEventArgs
        {
            progress = fryingTimer.Value / fryingTimerMax
        });
    }

    void BurningTimer_OnValueChanged(float preValue, float newValue)
    {
        float burningTimerMax = BurningRecipeSO != null ? BurningRecipeSO.burningTimerMax : 1f;

        OnProgressChange?.Invoke(this, new IHasProgressBar.OnProgressChangeEventArgs
        {
            progress = burningTimer.Value / burningTimerMax
        });
    }

    void currentState_OnValueChanged(FryState preState,FryState newState)
    {
        OnStateChanged?.Invoke(this, new OnStateChangeEventArgs
        {
            State = currentState.Value
        });

        if(currentState.Value==FryState.Burned||currentState.Value==FryState.Idle)
        {
            OnProgressChange?.Invoke(this, new IHasProgressBar.OnProgressChangeEventArgs
            {
                progress = 0f
            });
        }
    }

    private void Update()
    {
        if(!IsServer)
        {
            return;
        }

        if (hasKitchenObject())
        {
            switch (currentState.Value)
            {
                case FryState.Idle:
                    break;
                case FryState.Frying:
                    fryingTimer.Value += Time.deltaTime;

                    if (fryingTimer.Value > FryingRecipeSO.fryingTimerMax)
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());

                        KitchenObject.SpwanKitchenObject(FryingRecipeSO.output, this);

                        currentState.Value = FryState.Fried;
                        burningTimer.Value = 0f;
                        SetBurningRecipeSOClientRpc(
                            KitchenGameMultiplayer.Instance.GetKitcheObjectSOIndex(GetKitchenObject().getKitchenObjectSO())
                            );
                    }
                    break;
                case FryState.Fried:
                    burningTimer.Value += Time.deltaTime;

                    if (burningTimer.Value > BurningRecipeSO.burningTimerMax)
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());

                        KitchenObject.SpwanKitchenObject(BurningRecipeSO.output, this);

                        currentState.Value = FryState.Burned;   
                    }
                    break;
                case FryState.Burned:
                    break;
            }
        }
        
    }
    public override void Interact(Player player)
    {
        if (player.hasKitchenObject())
        {
            if (!hasKitchenObject())//将东西放入counter
            {
                if (hasRecipeWithInput(player.GetKitchenObject().getKitchenObjectSO()))
                {
                    KitchenObject kitchenObject = player.GetKitchenObject();
                    kitchenObject.SetKitchenObjectParent(this);

                    InteractLogicPlaceObjectOnCounterServerRpc(
                        KitchenGameMultiplayer.Instance.GetKitcheObjectSOIndex(kitchenObject.getKitchenObjectSO())
                    );
                }
            }
            else//从counter取东西到盘子里
            {
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().getKitchenObjectSO()))
                    {
                        //GetKitchenObject().DestroySelf();
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());

                        SetStateIdleServerRpc();
                    }
                }
            }
        }
        else
        {
            if (hasKitchenObject())//直接手抓，干净又卫生
            {
                GetKitchenObject().SetKitchenObjectParent(player);

                SetStateIdleServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership =false)]
    void SetStateIdleServerRpc()
    {
        currentState.Value = FryState.Idle;
    }

    [ServerRpc(RequireOwnership =false)]
    void InteractLogicPlaceObjectOnCounterServerRpc(int kitchenObjectSOIndex)
    {
        fryingTimer.Value = 0f;

        currentState.Value = FryState.Frying;

        SetFryingRecipeSOClientRpc(kitchenObjectSOIndex);
    }

    [ClientRpc]
    void SetFryingRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO=KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);

        FryingRecipeSO = GetFryingRecipeSOWithInput(kitchenObjectSO);
    }

    [ClientRpc]
    void SetBurningRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);

        BurningRecipeSO = GetBurningRecipeSOWithInput(kitchenObjectSO);
    }

    bool hasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
        return fryingRecipeSO != null;
    }

    KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO)
    {
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
        if (fryingRecipeSO != null)
        {
            return fryingRecipeSO.output;
        }
        else
        {
            return null;
        }
    }

    FryingRecipeSO GetFryingRecipeSOWithInput(KitchenObjectSO kitchenObjectSO)
    {
        foreach (FryingRecipeSO fryingRecipeSO in FryingRecipeSOArray)
        {
            if (fryingRecipeSO.input == kitchenObjectSO)
            {
                return fryingRecipeSO;
            }
        }
        return null;
    }

    BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectSO kitchenObjectSO)
    {
        foreach (BurningRecipeSO burningRecipeSO in BurningRecipeSOArray)
        {
            if (burningRecipeSO.input == kitchenObjectSO)
            {
                return burningRecipeSO;
            }
        }
        return null;
    }

    public bool IsFried()
    {
        return currentState.Value == FryState.Fried;
    }
}

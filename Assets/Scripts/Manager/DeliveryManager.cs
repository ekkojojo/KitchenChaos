using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DeliveryManager : NetworkBehaviour
{
    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeFailed;
    public event EventHandler OnRecipeSuccess;
    public static DeliveryManager Instance {  get; private set; }

    List<RecipeSO> waitingRecipeSOList;

    [SerializeField] RecipeListSO RecipeListSO;

    float spawnRecipeTimer = 4;
    float spawnRecipeTimerMax = 4;
    int waitingRecipeMax = 4;
    int successfulRecipesAmount;
    private void Awake()
    {
        Instance = this;

        waitingRecipeSOList = new List<RecipeSO>();
    }

    private void Update()
    {
        if(!IsServer)
        {
            return;
        }

        if(KitchenGameManager.Instance.IsGamePlaying()) 
        {
            spawnRecipeTimer -= Time.deltaTime;
            if (spawnRecipeTimer <= 0f)
            {
                spawnRecipeTimer = spawnRecipeTimerMax;

                if (waitingRecipeSOList.Count < waitingRecipeMax)
                {
                    int recipeSOIndex = UnityEngine.Random.Range(0, RecipeListSO.m_Recipes.Count);
                    //RecipeSO recipeSO = RecipeListSO.m_Recipes[recipeSOIndex];
                    //Debug.Log(recipeSO.recipeName);
                    SpawnNewWaitingRecipeClientRpc(recipeSOIndex);

                }
            }
        }
        
    }
    [ClientRpc]
    void SpawnNewWaitingRecipeClientRpc(int recipeSOIndex)
    {
        RecipeSO recipeSO = RecipeListSO.m_Recipes[recipeSOIndex];
        waitingRecipeSOList.Add(recipeSO);

        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }

    public void DeliveryRecipe(PlateKitchenObject plateKitchenObject)
    {
        for(int i = 0; i < waitingRecipeSOList.Count; i++) 
        {
            RecipeSO waitingRecipeSO = waitingRecipeSOList[i];

            if (waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count)
            {
                //has the same number of ingredients
                bool platesContentMatchesRecipe = true;
                foreach(KitchenObjectSO recipeKitchenObject in waitingRecipeSO.kitchenObjectSOList)
                {
                    //cycling through all ingredients in the plate and recipe
                    bool ingredientFound = false;
                    foreach(KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList())
                    {
                        if (plateKitchenObjectSO == recipeKitchenObject)
                        {
                            ingredientFound = true;
                            break;
                        }
                    }
                    if(!ingredientFound)
                    {
                        platesContentMatchesRecipe = false;
                    }
                }
                if (platesContentMatchesRecipe)
                {
                    //Debug.Log("match");
                    DeliveryCorrectRecipeServerRpc(i);
                    return;
                }
            }
        }
        //Debug.Log("not match");
        DeliveryIncorrectRecipeServerRpc();
    }
    [ServerRpc(RequireOwnership =false)]
    void DeliveryIncorrectRecipeServerRpc()
    {
        DeliveryIncorrectRecipeClientRpc();
    }
    [ClientRpc]
    void DeliveryIncorrectRecipeClientRpc()
    {
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }
    [ServerRpc(RequireOwnership = false)]
    void DeliveryCorrectRecipeServerRpc(int RecipeSOListIndex)
    {
        DeliveryCorrectRecipeClientRpc(RecipeSOListIndex);
    }
    [ClientRpc]
    void DeliveryCorrectRecipeClientRpc(int RecipeSOListIndex)
    {
        waitingRecipeSOList.RemoveAt(RecipeSOListIndex);
        successfulRecipesAmount++;

        OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
        OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
    }
    public List<RecipeSO> GetWaitingRecipeSOList() 
    {
        return waitingRecipeSOList;
    }

    public int GetSuccessfulRecipesAmount()
    {
        return successfulRecipesAmount;
    }
}

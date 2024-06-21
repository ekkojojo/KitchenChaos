using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateCompleteVisual : MonoBehaviour
{
    [Serializable]
    public struct KitchenObjectSO_GameObject
    {
        public GameObject gameObject;
        public KitchenObjectSO kitchenObjectSO;
    }

    [SerializeField]PlateKitchenObject kitchenObject;

    [SerializeField]List<KitchenObjectSO_GameObject> kitchenObjectSOGameObjectList;

    private void Start()
    {
        kitchenObject.OnIngredientAdded += KitchenObject_OnIngredientAdded;

        foreach (KitchenObjectSO_GameObject objectSO_GameObject in kitchenObjectSOGameObjectList)
        {
            objectSO_GameObject.gameObject.SetActive(false);
        }
    }

    private void KitchenObject_OnIngredientAdded(object sender, PlateKitchenObject.OnIngredientAddedEventArgs e)
    {
        foreach(KitchenObjectSO_GameObject objectSO_GameObject in kitchenObjectSOGameObjectList)
        {
            if(objectSO_GameObject.kitchenObjectSO == e.kitchenObjectSO)
            {
                objectSO_GameObject.gameObject.SetActive(true);
            }
        }
    }
}

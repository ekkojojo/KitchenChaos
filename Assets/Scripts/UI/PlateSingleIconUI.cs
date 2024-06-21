using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlateSingleIconUI : MonoBehaviour
{
    [SerializeField] Image Image;
    public void SetKitchenObjectSO(KitchenObjectSO kitchenObjectSO)
    {
        Image.sprite = kitchenObjectSO.Sprite;
    }
}

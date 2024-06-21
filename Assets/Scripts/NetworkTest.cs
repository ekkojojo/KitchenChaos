using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkTest : MonoBehaviour
{
    [SerializeField] Button hostButton;
    [SerializeField] Button clientButton;
    private void Awake()
    {
        hostButton.onClick.AddListener(() =>
        {
            print("host");
            KitchenGameMultiplayer.Instance.StartHost();
            gameObject.SetActive(false);
        });
        clientButton.onClick.AddListener(() =>
        {
            print("cliet");
            KitchenGameMultiplayer.Instance.StartClient();
            gameObject.SetActive(false);
        });
    }
}

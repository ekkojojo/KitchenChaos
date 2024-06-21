using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreatUI : MonoBehaviour
{
    [SerializeField] Button closeButton;
    [SerializeField] Button creatPublicButton;
    [SerializeField] Button creatPrivateButton;
    [SerializeField] TMP_InputField lobbyNameInputField;

    private void Awake()
    {
        closeButton.onClick.AddListener(() =>
        {
            Hide();
        });
        creatPublicButton.onClick.AddListener(() =>
        {
            KitchenGameLobby.Instance.CreatLobby(lobbyNameInputField.text, false);
        });
        creatPrivateButton.onClick.AddListener(() =>
        {
            KitchenGameLobby.Instance.CreatLobby(lobbyNameInputField.text, true);
        });
    }

    private void Start()
    {
        Hide() ;
    }

    void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
}

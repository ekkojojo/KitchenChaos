using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] Button creatGameButton;
    [SerializeField] Button quickJoinGameButton;
    [SerializeField] Button mainMenuButton;
    [SerializeField] Button JoinCodeButton;
    [SerializeField] TMP_InputField joinCodeInputField;
    [SerializeField] LobbyCreatUI lobbyCreatUI;

    private void Awake()
    {
        creatGameButton.onClick.AddListener(() =>
        {
            //KitchenGameLobby.Instance.CreatLobby("LobbyName",false);
            lobbyCreatUI.Show();
            /*KitchenGameMultiplayer.Instance.StartHost();
            Loader.LoadNetwork(Loader.Scene.CharacterSelectScene);*/
        });
        quickJoinGameButton.onClick.AddListener(() =>
        {
            
            KitchenGameLobby.Instance.QuickJoin();
            //KitchenGameMultiplayer.Instance.StartClient();
        });
        mainMenuButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.MainMenuScene);
            KitchenGameLobby.Instance.LeaveLobby();
        });
        JoinCodeButton.onClick.AddListener(() =>
        {
            KitchenGameLobby.Instance.JoinWithCode(joinCodeInputField.text);
        });
    }
}

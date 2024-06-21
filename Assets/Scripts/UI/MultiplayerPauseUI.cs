using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerPauseUI : MonoBehaviour
{
    private void Start()
    {
        KitchenGameManager.Instance.OnMultiplayerGamePaused += KitchenManager_OnMultiplayerGamePaused;
        KitchenGameManager.Instance.OnMultiplayerGameUnPaused += KitchenManager_OnMultiplayerGameUnPaused;

        Hide();
    }

    private void KitchenManager_OnMultiplayerGameUnPaused(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void KitchenManager_OnMultiplayerGamePaused(object sender, System.EventArgs e)
    {
        Show();
    }

    void Show()
    {
        gameObject.SetActive(true);
    }
    void Hide()
    {
        gameObject.SetActive(false);
    }
}

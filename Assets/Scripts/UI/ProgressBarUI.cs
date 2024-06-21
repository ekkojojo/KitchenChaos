using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
{
    [SerializeField] Image bar;

    [SerializeField] GameObject hasProgressGameObject;
    IHasProgressBar hasProgress;

    private void Start()
    {
        hasProgress = hasProgressGameObject.GetComponent<IHasProgressBar>();
        if(hasProgress == null) 
        {
            Debug.Log("Do not have implement IHasProgress");
        }
        hasProgress.OnProgressChange += HasProgressBar_OnProgressChange;

        bar.fillAmount = 0;
        Hide();
    }

    private void HasProgressBar_OnProgressChange(object sender, IHasProgressBar.OnProgressChangeEventArgs e)
    {
        bar.fillAmount = e.progress;
        if (e.progress == 0f || e.progress == 1f)
        {
            Hide();
        }
        else
        {
            Show();
        }
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

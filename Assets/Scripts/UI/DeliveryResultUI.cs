using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryResultUI : MonoBehaviour
{
    [SerializeField] Image bg;
    [SerializeField] Image Icon;
    [SerializeField] TextMeshProUGUI messageText;
    [SerializeField] Color successColor;
    [SerializeField] Color failedColor;
    [SerializeField] Sprite successSprite;
    [SerializeField] Sprite failedSprite;
    Animator animator;
    const string FLASH = "Flash";
    private void Start()
    {
        animator = GetComponent<Animator>();
        DeliveryManager.Instance.OnRecipeSuccess += Instance_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFailed += Instance_OnRecipeFailed;

        Hide();
    }

    private void Instance_OnRecipeFailed(object sender, System.EventArgs e)
    {
        Show();
        animator.SetTrigger(FLASH);
        bg.color = failedColor;
        Icon.sprite = failedSprite;
        messageText.text = "DELIVERY\nFAILED";
    }

    private void Instance_OnRecipeSuccess(object sender, System.EventArgs e)
    {
        Show() ;
        animator.SetTrigger(FLASH);
        bg.color = successColor;
        Icon.sprite = successSprite;
        messageText.text = "DELIVERY\nSUCCESS";
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

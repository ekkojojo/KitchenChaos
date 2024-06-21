using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveCounterVisual : MonoBehaviour
{
    [SerializeField] GameObject stoveGameObject;
    [SerializeField] GameObject particleGameObject;

    [SerializeField] StoveCounter StoveCounter;
    private void Start()
    {
        StoveCounter.OnStateChanged += StoveCounter_OnStateChanged;
    }

    private void StoveCounter_OnStateChanged(object sender, StoveCounter.OnStateChangeEventArgs e)
    {
        bool showVisual = e.State == StoveCounter.FryState.Frying || e.State == StoveCounter.FryState.Fried;
        stoveGameObject.SetActive(showVisual);
        particleGameObject.SetActive(showVisual);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounterVisual : MonoBehaviour
{
    const string CUT = "Cut";
    Animator Animator;
    [SerializeField] CuttingCounter cuttingCounter;
    private void Awake()
    {
        Animator = GetComponent<Animator>();

    }
    private void Start()
    {
        cuttingCounter.OnCut += CuttingCounter_OnCut;
    }

    private void CuttingCounter_OnCut(object sender, System.EventArgs e)
    {
        Animator.SetTrigger(CUT);
    }

}

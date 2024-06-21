using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerCounterVisual : MonoBehaviour
{
    const string OPEN_CLOSE = "OpenClose";
    Animator Animator;
    [SerializeField] ContainerCounter containerCounter;
    private void Awake()
    {
        Animator = GetComponent<Animator>();

    }
    private void Start()
    {
        containerCounter.OnPlayerGrabObject += ContainerCounter_OnPlayerGrabObject;
    }

    private void ContainerCounter_OnPlayerGrabObject(object sender, System.EventArgs e)
    {
        Animator.SetTrigger(OPEN_CLOSE);
    }

}

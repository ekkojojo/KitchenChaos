using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{
    const string IS_WALKING = "IsWalking";
    Animator Animator;
    [SerializeField] Player player;
    private void Awake()
    {
        Animator = GetComponent<Animator>();

    }

    private void Update()
    {
        if(!IsOwner)
        {
            return;
        }
        Animator.SetBool(IS_WALKING, player.IsWalking());
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }
    public event EventHandler OnInteractAction;
    public event EventHandler OnInteractAlternativeAction;
    public event EventHandler OnPauseAction;

    private InputControls inputActions;
    private void Awake()
    {
        Instance = this;
        inputActions = new InputControls();
        inputActions.Player.Enable();

        inputActions.Player.Interact.performed += Interact_Performed;
        inputActions.Player.InteractAlternative.performed += InteractAlternative_performed;
        inputActions.Player.Pause.performed += Pause_performed;
    }
    private void OnDestroy()
    {
        inputActions.Player.Interact.performed -= Interact_Performed;
        inputActions.Player.InteractAlternative.performed -= InteractAlternative_performed;
        inputActions.Player.Pause.performed -= Pause_performed;

        inputActions.Dispose();
    }
    private void Pause_performed(InputAction.CallbackContext context)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void InteractAlternative_performed(InputAction.CallbackContext obj)
    {
        OnInteractAlternativeAction?.Invoke(this, EventArgs.Empty);
    }

    private void Interact_Performed(InputAction.CallbackContext context)
    {
        OnInteractAction?.Invoke(this,EventArgs.Empty);
    }

    public Vector2 GetMovementVector2Normalized()
    {
        Vector2 inputVector2 = inputActions.Player.Move.ReadValue<Vector2>();
        /*if (Input.GetKey(KeyCode.W))
        {
            inputVector2.y += 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            inputVector2.y -= 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            inputVector2.x += 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            inputVector2.x -= 1;
        }*/
        inputVector2 = inputVector2.normalized;
        //print(inputVector2);
        return inputVector2;
    }
}
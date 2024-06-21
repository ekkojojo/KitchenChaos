using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour,IKitchenObjectParent
{
    public static Player LocalInstance { get; private set; }

    public static event EventHandler OnAnyPlayerSpawn;
    public static event EventHandler OnAnyPickSomething;
    public static void ResetStaticData()
    {
        OnAnyPlayerSpawn = null;
    }
    public event EventHandler OnPickSomething;
    public event EventHandler<OnSelectedCounterChangedEventArg> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArg : EventArgs
    {
        public BaseCounter selectedCounter;
    }

    [SerializeField] MeshRenderer headMeshRenderer;
    [SerializeField] MeshRenderer bodyMeshRenderer;
    Material material;

    [SerializeField] float moveSpeed = 7f;
    //[SerializeField] GameInput gameInput;
    [SerializeField] LayerMask layerMask;
    [SerializeField] List<Vector3> spawnPositionList;

    [SerializeField] Transform holdPosition;
    KitchenObject kitchenObject;

    bool isWalkingn;
    Vector3 lastInteractDir;
    BaseCounter selectedCounter;

    private void Start()
    {
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        GameInput.Instance.OnInteractAlternativeAction += GameInput_OnInteractAlternativeAction;

        material = new(headMeshRenderer.material);
        headMeshRenderer.material = material;
        bodyMeshRenderer.material = material;

        //PlayerData playerData = KitchenGameMultiplayer.Instance.GetPlayerDateFromClientID(OwnerClientId);
        int index= KitchenGameMultiplayer.Instance.GetPlayerDateIndexFromClientID(OwnerClientId);
        SetPlayerColor(KitchenGameMultiplayer.Instance.GetPlayerColor(index));
        //print(KitchenGameMultiplayer.Instance.GetPlayerColor(playerData.ColorID));
    }
    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            LocalInstance = this;
        }

        transform.position = spawnPositionList[(int)OwnerClientId];

        OnAnyPlayerSpawn?.Invoke(this, EventArgs.Empty);

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientID)
    {
        if (clientID == OwnerClientId && hasKitchenObject())
        {
            KitchenObject.DestroyKitchenObject(GetKitchenObject());
        }
    }

    private void GameInput_OnInteractAlternativeAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        if (selectedCounter != null)
        {
            selectedCounter.InteractAlternative(this);
        }
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        if (selectedCounter != null)
        {
            selectedCounter.Interact(this);
        }
    }

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        /*if (!KitchenGameManager.Instance.IsGamePlaying())
        {
            return ;
        }*/
        HandleMovement();
        //HandleMovementAuth();
        HandleInteractions();
    }

    void HandleMovementAuth()
    {
        Vector2 inputVector2 = GameInput.Instance.GetMovementVector2Normalized();
        HandleMovementServerRpc(inputVector2);
    }
    [ServerRpc(RequireOwnership =false)]
    void HandleMovementServerRpc(Vector2 inputVector2)
    {
        //Vector2 inputVector2 = GameInput.Instance.GetMovementVector2Normalized();
        Vector3 moveDir = new(inputVector2.x, 0, inputVector2.y);

        float moveDistance = moveSpeed * Time.deltaTime;
        float playerRadius = .7f;
        float playerHeight = 2f;
        bool canMove = !Physics.CapsuleCast(transform.position,
            transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);

        if (!canMove)
        {
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = moveDir.x != 0 && !Physics.CapsuleCast(transform.position,
            transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance);

            if (canMove)
            {
                moveDir = moveDirX;
            }
            else
            {
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = moveDir.z != 0 && !Physics.CapsuleCast(transform.position,
                transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance);

                if (canMove)
                {
                    moveDir = moveDirZ;
                }

            }
        }

        if (canMove) transform.position += moveDir * moveDistance;
        isWalkingn = (moveDir != Vector3.zero);

        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
    }
    void HandleMovement()
    {
        Vector2 inputVector2 = GameInput.Instance.GetMovementVector2Normalized();
        Vector3 moveDir = new(inputVector2.x, 0, inputVector2.y);

        float moveDistance = moveSpeed * Time.deltaTime;
        float playerRadius = .7f;
        float playerHeight = 2f;
        bool canMove = !Physics.CapsuleCast(transform.position,
            transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);

        if (!canMove)
        {
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = moveDir.x!=0&&!Physics.CapsuleCast(transform.position,
            transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance);

            if (canMove)
            {
                moveDir = moveDirX;
            }
            else
            {
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = moveDir.z != 0 && !Physics.CapsuleCast(transform.position,
                transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance);

                if (canMove)
                {
                    moveDir = moveDirZ;
                }

            }
        }

        if (canMove) transform.position += moveDir * moveDistance;
        isWalkingn = (moveDir != Vector3.zero);

        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
    }

    void HandleInteractions()
    {
        Vector2 inputVector2 = GameInput.Instance.GetMovementVector2Normalized();
        Vector3 moveDir = new(inputVector2.x, 0, inputVector2.y);

        if(moveDir!=Vector3.zero)
        {
            lastInteractDir = moveDir;
        }

        float interactDistance = 2f;
        if(Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, layerMask))
        {
            if(raycastHit.transform.TryGetComponent(out BaseCounter baseCounter))
            {
                //clearCounter.Interact();
                if(baseCounter != selectedCounter)
                {
                    SetSelectedCounter(baseCounter);
                }
            }
            else
            {
                SetSelectedCounter(null);
            }
        }
        else
        {
            SetSelectedCounter(null);
        }
        //Debug.Log(selectedCounter);
    }

    void SetSelectedCounter(BaseCounter clearCounter)
    {
        this.selectedCounter = clearCounter;

        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArg
        {
            selectedCounter = selectedCounter
        });
    }

    public bool IsWalking()
    {
        return isWalkingn;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;

        if(kitchenObject != null)
        {
            OnPickSomething?.Invoke(this, EventArgs.Empty);
            OnAnyPickSomething?.Invoke(this, EventArgs.Empty);
        }
    }
    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    public Transform GetSpawnPosition()
    {
        return holdPosition;
    }

    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }

    public bool hasKitchenObject()
    {
        return kitchenObject != null;
    }
    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }

    public void SetPlayerColor(Color color)
    {
        material.color = color;
    }
}

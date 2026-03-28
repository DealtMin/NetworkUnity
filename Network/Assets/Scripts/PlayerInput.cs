using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInput : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private InputActionAsset inputActions;
        
    private InputActionMap _playerActionMap;
    private InputAction _attackAction;
    public Action OnAttackPressed;
  
        
    private void Awake()
    {
        InitializeInput();
    }
        
    private void OnEnable()
    {
        EnableInput();
    }
        
    private void OnDisable()
    {
        DisableInput();
    }

    private void InitializeInput()
    {            
        _playerActionMap = inputActions.FindActionMap("Player");
        _attackAction = _playerActionMap.FindAction("Attack");
        _attackAction.performed += OnAttack;
    }
        
    private void EnableInput()
    {
        _playerActionMap?.Enable();
    }
        
    public void DisableInput()
    {
        _playerActionMap?.Disable();
    }
    private void OnDestroy()
    { 
        if (_attackAction != null)
        {
            _attackAction.performed -= OnAttack;
        }
    }
        
    private void OnAttack(InputAction.CallbackContext context)
    {
        OnAttackPressed?.Invoke();
    }

    public InputActionAsset GetInputActionAsset()
    {
        return inputActions;
    }
        
    public InputActionMap GetPlayerActionMap()
    {
        return _playerActionMap;
    }
}



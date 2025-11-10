using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    GameObject _player;

    InputAction moveAction;

    [SerializeField] private float _movementSpeed;
    [SerializeField] private float _playerMovBounds;

    void Start()
    {
        _player = this.gameObject;
        moveAction = InputSystem.actions.FindAction("Move");
    }

    void Update()
    {
        Vector2 _moveInput = moveAction.ReadValue<Vector2>();
        Movement(_moveInput);
    }

    private void Movement(Vector3 input)
    {
        // Modifies the players transform to move
        Vector3 tempPos = _player.transform.position;
        tempPos.x += input.x * _movementSpeed * Time.deltaTime;

        // Clamps the players transform within the bounds of the game
        tempPos.x = Mathf.Clamp(tempPos.x, -_playerMovBounds, _playerMovBounds);
        _player.transform.position = tempPos;
    }
}
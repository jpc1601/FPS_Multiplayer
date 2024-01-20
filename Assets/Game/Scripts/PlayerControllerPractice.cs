using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerControllerPractice : MonoBehaviour
{
    public Transform viewPoint;
    public Transform groundCheckPoint;
    public float mouseSensitivity = 1f;
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 12f;
    public float gravityMode = 2.5f;
    public bool invertLook;
    
    private bool _isGround;
    private float _viewPointX;
    private float _activeSpeed;
    private CharacterController _character;
    private Transform _camera;
    private Vector3 _activeDirection;

    private void Awake()
    {
        _character = GetComponent<CharacterController>();
        if (Camera.main != null) _camera = Camera.main.transform;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        //View Rotation
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        Vector2 viewDirection = new Vector2(mouseX, mouseY) * mouseSensitivity;
        
        var playerRotation = transform.rotation;
        playerRotation = Quaternion.Euler(playerRotation.eulerAngles.x, playerRotation.eulerAngles.y + viewDirection.x, playerRotation.eulerAngles.z);
        transform.rotation = playerRotation;

        _viewPointX += viewDirection.y;
        _viewPointX = Mathf.Clamp(_viewPointX, -60f, 60f);

        var viewPointRotation = viewPoint.rotation;
        viewPointRotation = Quaternion.Euler(invertLook ? -_viewPointX : _viewPointX, viewPointRotation.eulerAngles.y, viewPointRotation.eulerAngles.z);
        viewPoint.rotation = viewPointRotation;
        
        //Player Movement
        float forwardDirection = Input.GetAxis("Vertical");
        float sideDirection = Input.GetAxis("Horizontal");
        
        Vector3 moveDirection = new Vector3(sideDirection, 0f, forwardDirection);
        Transform playerTransform = transform;
        
        _activeSpeed = Input.GetKeyDown(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        float playerY = _activeDirection.y; 
        _activeDirection = (playerTransform.forward * moveDirection.z + playerTransform.right * moveDirection.x).normalized * _activeSpeed;
        _activeDirection.y = playerY;
        
        if (_character.isGrounded)
            _activeDirection.y = 0f;

        _isGround = Physics.Raycast(groundCheckPoint.position, Vector3.down, 0.25f, 1 << 6);
        if (Input.GetButtonDown("Jump") && _isGround)
            _activeDirection.y = jumpForce;
        
        _activeDirection.y += Physics.gravity.y * Time.deltaTime * gravityMode;
        _character.Move(_activeDirection * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Escape))
            Cursor.lockState = CursorLockMode.None;
        else if (Cursor.lockState == CursorLockMode.None)
            if (Input.GetMouseButtonDown(0))
                Cursor.lockState = CursorLockMode.Locked;
    }

    private void LateUpdate()
    {
        _camera.position = viewPoint.position;
        _camera.rotation = viewPoint.rotation;
    }
}

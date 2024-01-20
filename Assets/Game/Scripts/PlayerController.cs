using UnityEngine;
public class PlayerController : MonoBehaviour
{
    public GameObject bulletImpactPrefab;
    public Transform viewPoint;
    public Transform groundCheckPoint;
    public LayerMask groundLayer;
    public float mouseSensitivity = 1f;
    public bool invertLook;
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 12f;
    public float gravityMode = 2.5f;

    private bool _isGrounded;
    private float _verticalRotation;
    private float _activeSpeed;
    private Vector3 _moveDirection;
    private Vector3 _movement;

    private float _shotCounter;
    private float _muzzleCounter;

    public float muzzleDisplayTime;
    public float maxHeat = 10f;
    public float coolDownRate = 4f;
    public float overHeatCoolDownRate = 5f;

    public Gun[] allGuns;

    private float _heatCounter;
    private bool _isOverHeated;
    private int _selectedGun;

    private CharacterController _characterController;
    private UIController _uiController;
    private Camera _camera;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _uiController = FindObjectOfType<UIController>();
        _camera = Camera.main;

        _uiController.weaponTemperature.maxValue = maxHeat;

    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        
        var targetTransform = SpawnManager.Instance.GetPlayerTransform();
        var playerTransform = transform;
        playerTransform.position = targetTransform.position;
        playerTransform.rotation = targetTransform.rotation;
        SwitchGun();
    }

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Mouse X");
        float vertical = Input.GetAxisRaw("Mouse Y");
        
        Vector2 inputVector = new Vector2(horizontal, vertical) * mouseSensitivity;

        //Horizontal View
        Quaternion rotation = transform.rotation;
        rotation = Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y + inputVector.x, rotation.eulerAngles.z);
        transform.rotation = rotation;

        //Vertical View
        _verticalRotation += inputVector.y;
        _verticalRotation = Mathf.Clamp(_verticalRotation, -60f, 60f);
        
        Quaternion viewPortRotation = viewPoint.rotation;
        viewPortRotation = Quaternion.Euler( invertLook ? -_verticalRotation : _verticalRotation, viewPortRotation.eulerAngles.y, viewPortRotation.eulerAngles.z);
        viewPoint.rotation = viewPortRotation;

        //Player Movement
        float forwardDir = Input.GetAxis("Vertical");
        float sideDir = Input.GetAxis("Horizontal");

        _moveDirection = new Vector3(sideDir, 0f, forwardDir);
        Transform playerTransform = transform;
        _activeSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        float movY = _movement.y;
        _movement = (playerTransform.forward * _moveDirection.z + playerTransform.right * _moveDirection.x).normalized * _activeSpeed;
        _movement.y = movY;
        
        if (_characterController.isGrounded)
            _movement.y = 0;

        _isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, 0.25f, groundLayer);
        // Debug.DrawRay(groundCheckPoint.position, Vector3.down, Color.red);
        
        //Jump
        if (Input.GetButtonDown("Jump") && _isGrounded)
            _movement.y = jumpForce;
        
        //Applied Gravity
        _movement.y += Physics.gravity.y * Time.deltaTime * gravityMode;
        _characterController.Move(_movement * Time.deltaTime);

        if(allGuns[_selectedGun].muzzleFlash.activeInHierarchy)
        {
            _muzzleCounter -= Time.deltaTime;
            if(_muzzleCounter <= 0)
                allGuns[_selectedGun].muzzleFlash.SetActive(false);
        }

        if(!_isOverHeated)
        {
            // Debug.Log("Selected Gun::" + _selectedGun);
            if (Input.GetMouseButtonDown(0))
                Shoot();

            if (allGuns[_selectedGun].isAutomatic && Input.GetMouseButton(0))
            {
                _shotCounter -= Time.deltaTime;
                if (_shotCounter <= 0)
                    Shoot();
            }

            _heatCounter -= coolDownRate * Time.deltaTime;
        }
        else
        {
            _heatCounter -= overHeatCoolDownRate * Time.deltaTime;
            if (_heatCounter <= 0)
                _heatCounter = 0;
        }
        
        if (_heatCounter <= 0)
        {
            _uiController.heatedText.SetActive(false);
            _isOverHeated = false;
        }
        _uiController.weaponTemperature.value = _heatCounter;

        float mouseScroll = Input.GetAxisRaw("Mouse ScrollWheel");
        // if(mouseScroll != 0)
        //     Debug.Log(Mathf.RoundToInt(mouseScroll) +" -- " + Mathf.CeilToInt(mouseScroll) +" ==  " + Mathf.FloorToInt(mouseScroll));

        if (mouseScroll > 0)
        {
            _selectedGun++;
            if (_selectedGun >= allGuns.Length)
                _selectedGun = allGuns.Length - 1;
            
            SwitchGun();
        }
        else if (mouseScroll < 0)
        {
            _selectedGun--;
            if (_selectedGun < 0)
                _selectedGun = 0;
            
            SwitchGun();
        }

        for (var i = 0; i < allGuns.Length; i++)
        {
            if (!Input.GetKeyDown((i + 1).ToString())) continue;
            _selectedGun = i;
            SwitchGun();
        }
        
        //Freeing and Locking Cursor
        if (Input.GetKeyDown(KeyCode.Escape))
            Cursor.lockState = CursorLockMode.None;
        else if (Cursor.lockState == CursorLockMode.None)
            if (Input.GetMouseButtonDown(0))
                Cursor.lockState = CursorLockMode.Locked;
    }

    private void LateUpdate()
    {
        var cameraTransform = _camera.transform;
        cameraTransform.position = viewPoint.position;
        cameraTransform.rotation = viewPoint.rotation;
    }

    private void Shoot()
    {
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        ray.direction = viewPoint.forward;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject bulletImpact = Instantiate(bulletImpactPrefab, hit.point + hit.normal * 0.002f,
                Quaternion.LookRotation(hit.normal, Vector3.up));
            
            Destroy(bulletImpact, 5f);
        }

        _shotCounter += allGuns[_selectedGun].timeBetweenShots;

        _heatCounter += allGuns[_selectedGun].heatPerShot;
        if (_heatCounter >= maxHeat)
        {
            _uiController.heatedText.SetActive(true);
            _heatCounter = maxHeat;
            _isOverHeated = true;
        }
        
        allGuns[_selectedGun].muzzleFlash.SetActive(true);
        _muzzleCounter = muzzleDisplayTime;
    }

    private void SwitchGun()
    {
        foreach (Gun gun in allGuns)
            gun.gameObject.SetActive(false);
        
        allGuns[_selectedGun].gameObject.SetActive(true);
        allGuns[_selectedGun].muzzleFlash.SetActive(false);
    }
}

using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(CharacterController))]
public class HybridCameraMovement : MonoBehaviour
{
    [Header("UI")]
    public GameObject uiPanel;
    public XRRayInteractor leftRayInteractor;
    public XRRayInteractor rightRayInteractor;
    private bool isUiOpen = false;

    [Header("Desktop Controls")]
    public float mouseTurnSpeed = 200f;
    public float lookClamp = 80f;
    public float moveSpeed = 5f;

    [Header("XR Controls")]
    public XRNode inputSource = XRNode.LeftHand;
    public XRNode rotationSource = XRNode.RightHand;
    public float xrTurnSpeed = 60f;

    [Header("General")]
    public float gravity = -9.81f;

    private CharacterController character;
    private XROrigin rig;

    private float verticalVelocity;
    private float mouseLookX;

    private Vector2 xrInput;
    private Vector2 xrRotate;
    private bool primaryButton;
    private bool prevPrimaryButton;

    void Start()
    {
        character = GetComponent<CharacterController>();
        rig = GetComponent<XROrigin>();

        if (uiPanel != null) uiPanel.SetActive(false);
        if (leftRayInteractor && rightRayInteractor)
        {
            leftRayInteractor.enabled = false;
            rightRayInteractor.enabled = false;
        }

        LockCursor();
    }

    void Update()
    {
        HandleXRInput();
        HandleDesktopLook();
        HandleUIToggle();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    // ---------------- XR ----------------

    void HandleXRInput()
    {
        InputDevice moveDevice = InputDevices.GetDeviceAtXRNode(inputSource);
        moveDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out xrInput);
        moveDevice.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButton);

        InputDevice rotDevice = InputDevices.GetDeviceAtXRNode(rotationSource);
        rotDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out xrRotate);

        transform.Rotate(0f, xrRotate.x * xrTurnSpeed * Time.deltaTime, 0f);
    }

    // ---------------- DESKTOP ----------------

    void HandleDesktopLook()
    {
        if (isUiOpen) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseTurnSpeed * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseTurnSpeed * Time.deltaTime;

        mouseLookX -= mouseY;
        mouseLookX = Mathf.Clamp(mouseLookX, -lookClamp, lookClamp);

        if (rig.Camera != null)
            rig.Camera.transform.localRotation = Quaternion.Euler(mouseLookX, 0f, 0f);

        transform.Rotate(0f, mouseX, 0f);
    }

    void HandleMovement()
    {
        Vector3 move = Vector3.zero;

        // XR Movement
        Quaternion headYaw = Quaternion.Euler(0f, rig.Camera.transform.eulerAngles.y, 0f);
        move += headYaw * new Vector3(xrInput.x, 0f, xrInput.y);

        // Desktop WASD
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        move += transform.forward * v + transform.right * h;

        // Gravity
        if (character.isGrounded)
            verticalVelocity = 0;
        else
            verticalVelocity += gravity * Time.fixedDeltaTime;

        move.y = verticalVelocity;

        character.Move(move * moveSpeed * Time.fixedDeltaTime);
    }

    // ---------------- UI ----------------

    void HandleUIToggle()
    {
        bool primaryDown = primaryButton && !prevPrimaryButton;

        if (Input.GetKeyDown(KeyCode.Escape) || primaryDown)
        {
            ToggleUI();
        }

        prevPrimaryButton = primaryButton;
    }

    void ToggleUI()
    {
        if (!uiPanel || !leftRayInteractor || !rightRayInteractor) return;

        isUiOpen = !isUiOpen;

        uiPanel.SetActive(isUiOpen);
        leftRayInteractor.enabled = isUiOpen;
        rightRayInteractor.enabled = isUiOpen;

        if (isUiOpen)
            UnlockCursor();
        else
            LockCursor();
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}

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
    public float moveSpeed = 5f;

    [Header("XR Controls")]
    public XRNode inputSource = XRNode.LeftHand;
    public XRNode rotationSource = XRNode.RightHand;
    public float xrTurnSpeed = 60f;

    [Header("General")]
    public float gravity = -9.81f;

    private CharacterController character;
    private XROrigin rig;
    private float verticalVelocity = 0f;

    private Vector2 xrInput;
    private Vector2 xrRotate;
    private bool primaryButton;
    private bool prevPrimaryButton;

    private void Start()
    {
        character = GetComponent<CharacterController>();
        rig = GetComponent<XROrigin>();

        if (uiPanel != null)
        {
            uiPanel.SetActive(false);
        }
        if (leftRayInteractor != null && rightRayInteractor != null)
        {
            leftRayInteractor.enabled = false;
            rightRayInteractor.enabled = false;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {

        // --- XR input ---
        InputDevice device = InputDevices.GetDeviceAtXRNode(inputSource);
        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out xrInput);
        device.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButton);

        InputDevice rotationDevice = InputDevices.GetDeviceAtXRNode(rotationSource);
        rotationDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out xrRotate);
        float yawAmount = xrRotate.x * xrTurnSpeed * Time.deltaTime;
        transform.Rotate(0f, yawAmount, 0f);

        // --- UI toggle ---
        bool primaryDown = primaryButton && !prevPrimaryButton;
        if (Input.GetKeyDown(KeyCode.Escape) || primaryDown)
        {
            toggleUI();
        }
        prevPrimaryButton = primaryButton;
    }

    private void FixedUpdate()
    {
        // --- Movement ---
        Vector3 moveDirection = Vector3.zero;


        // XR movement
        Quaternion headYaw = Quaternion.Euler(0f, rig.Camera.transform.eulerAngles.y, 0f);
        moveDirection += headYaw * new Vector3(xrInput.x, 0f, xrInput.y);

        // Gravity
        if (character.isGrounded)
            verticalVelocity = 0f;
        else
            verticalVelocity += gravity * Time.fixedDeltaTime;

        moveDirection.y = verticalVelocity;

        // Apply movement
        character.Move(moveDirection * moveSpeed * Time.fixedDeltaTime);
    }

    private void toggleUI()
    {

        if (uiPanel == null || leftRayInteractor == null || rightRayInteractor == null) { return; }
        isUiOpen = !isUiOpen;

        uiPanel.SetActive(isUiOpen);
        leftRayInteractor.enabled = isUiOpen;
        rightRayInteractor.enabled = isUiOpen;


        // Cursor lock for desktop testing
        if (isUiOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

}

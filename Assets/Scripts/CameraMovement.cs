using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR;
using Unity.XR.CoreUtils;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine.XR.Interaction.Toolkit.AR.Inputs;

public class CameraMovement : MonoBehaviour
{
    public XRNode inputSource;
    public XRNode rotationSource;
    private Vector2 inputAxis;
    private Vector2 rotateAxis;
    private CharacterController character;
    public float speed = 10;
    private XROrigin rig;
    public float fallingSpeed = -9.81f;
    public float turnSpeed = 60f;

    private void Start()
    {
        character = GetComponent<CharacterController>();
        rig = GetComponent<XROrigin>();
    }

    private void Update()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(inputSource);
        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out inputAxis);

        InputDevice rotationDevice = InputDevices.GetDeviceAtXRNode(rotationSource);
        rotationDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out rotateAxis);
    }

    private void FixedUpdate()
    {
        float yaw = rotateAxis.x * turnSpeed * Time.fixedDeltaTime;
        transform.Rotate(0, yaw, 0);

        Quaternion headYaw = Quaternion.Euler(0, rig.Camera.transform.eulerAngles.y, 0);
        Vector3 direction = headYaw * new Vector3(inputAxis.x, 0, inputAxis.y);
        character.Move(direction * Time.fixedDeltaTime * speed);
        character.Move(Vector3.up * fallingSpeed * Time.fixedDeltaTime);
    }
}

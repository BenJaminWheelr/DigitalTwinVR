using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR;

public class HandModel : MonoBehaviour
{
    public InputDeviceCharacteristics controllerCharacteristics;
    private bool isValid = false;
    private InputDevice targetDevice;
    public GameObject controllerPrefab;
    private GameObject spawnedController;
    public GameObject handModelPrefab;
    private GameObject spawnedHandModel;
    public bool showController = false;
    private Animator handAnimator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        getDevices();
    }

    void getDevices()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevices(devices);


        InputDevices.GetDevicesWithCharacteristics(controllerCharacteristics, devices);

        if (devices.Count > 0)
        {
            isValid = true;
            foreach (InputDevice device in devices)
            {
                Debug.Log(device.name + ". " + device.characteristics);
            }
            targetDevice = devices[0];
            spawnedController = Instantiate(controllerPrefab, transform);
            spawnedHandModel = Instantiate(handModelPrefab, transform);
            handAnimator = spawnedHandModel.GetComponent<Animator>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isValid)
        {
            getDevices();
        }
        else
        {
            if (showController)
            {
                spawnedHandModel.SetActive(false);
                spawnedController.SetActive(true);
            }
            else
            {
                spawnedHandModel.SetActive(true);
                spawnedController.SetActive(false) ;
                UpdateAnimator();
            }
        }
    }

    void UpdateAnimator()
    {
        if(targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
        {
            handAnimator.SetFloat("Trigger", triggerValue);
        }
        else
        {
            handAnimator.SetFloat("Trigger", 0);
        }
        if (targetDevice.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
        {
            handAnimator.SetFloat("Grip", gripValue);
        }
        else
        {
            handAnimator.SetFloat("Grip", 0);
        }
    }
}

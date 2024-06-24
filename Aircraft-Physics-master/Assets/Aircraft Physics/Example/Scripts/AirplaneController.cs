﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AirplaneController : MonoBehaviour
{
    [SerializeField]
    List<AeroSurface> controlSurfaces = null;
    [SerializeField]
    List<WheelCollider> wheels = null;
    [SerializeField]
    float rollControlSensitivity = 0.2f;
    [SerializeField]
    float pitchControlSensitivity = 0.2f;
    [SerializeField]
    float yawControlSensitivity = 0.2f;


    [Range(-1, 1)]
    public float Pitch;
    [Range(-1, 1)]
    public float Yaw;
    [Range(-1, 1)]
    public float Roll;
    [Range(0, 1)]
    public float Flap;
    [SerializeField]
    Text displayText = null;

    float thrustPercent;
    float brakesTorque;
    public float throttleSpeed = 1.0f;

    public Slider thrustSlider;

    AircraftPhysics aircraftPhysics;
    Rigidbody rb;

    //Added variables
    public bool visibleControls = true;
    GameObject controlsDisplay;

    private void Start()
    {
        aircraftPhysics = GetComponent<AircraftPhysics>();
        rb = GetComponent<Rigidbody>();
        controlsDisplay = GameObject.Find("Controls");
    }

    private void Update()
    {
        Pitch = Input.GetAxis("Vertical");
        Roll = Input.GetAxis("Horizontal");
        Yaw = Input.GetAxis("Yaw");

        float throttleInput = Input.GetAxis("ThrottleInc"); // RT
        float reverseThrottleInput = Input.GetAxis("ThrottleDec"); // LT

        //thrustPercent = thrustSlider.value;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            thrustPercent = thrustPercent > 0 ? 0 : 1f;
            /*thrustPercent = thrustPercent + 0.001f;
            if(thrustPercent > 1f)
            {
                thrustPercent = 1f;
            }*/
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Flap = Flap > 0 ? 0 : 0.3f;
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            Flap = Flap > 0 ? 0 : 0.3f;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            brakesTorque = brakesTorque > 0 ? 0 : 100f;
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            brakesTorque = brakesTorque > 0 ? 0 : 100f;
        }

        // Steuere die Throttle basierend auf den Trigger-Eingaben
        float throttleChange = throttleInput - reverseThrottleInput; // Differenz zwischen RT und LT
        thrustPercent += throttleChange  * throttleSpeed; // Anpassung der Throttle 

        // Begrenze thrustPercent auf den Bereich zwischen 0 und 1
        thrustPercent = Mathf.Clamp01(thrustPercent);

        //Debug.Log("ThrottleInc input: " + throttleInput);
        //Debug.Log("ThrottleDec input: " + reverseThrottleInput);

        if (visibleControls)
        {
            if (!controlsDisplay.activeSelf)
            {
                controlsDisplay.SetActive(true);
            }
            displayText.text = "V: " + ((int)rb.velocity.magnitude).ToString("D3") + " m/s\n";
            displayText.text += "A: " + ((int)transform.position.y).ToString("D4") + " m\n";
            displayText.text += "T: " + (int)(thrustPercent * 100) + "%\n";
            displayText.text += brakesTorque > 0 ? "B: ON" : "B: OFF";
        }
        else
        {
            controlsDisplay.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        SetControlSurfecesAngles(Pitch, Roll, Yaw, Flap);
        aircraftPhysics.SetThrustPercent(thrustPercent);
        foreach (var wheel in wheels)
        {
            wheel.brakeTorque = brakesTorque;
            // small torque to wake up wheel collider
            wheel.motorTorque = 0.01f;
        }
    }

    public void SetControlSurfecesAngles(float pitch, float roll, float yaw, float flap)
    {
        foreach (var surface in controlSurfaces)
        {
            if (surface == null || !surface.IsControlSurface) continue;
            switch (surface.InputType)
            {
                case ControlInputType.Pitch:
                    surface.SetFlapAngle(pitch * pitchControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Roll:
                    surface.SetFlapAngle(roll * rollControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Yaw:
                    surface.SetFlapAngle(yaw * yawControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Flap:
                    surface.SetFlapAngle(Flap * surface.InputMultiplyer);
                    break;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            SetControlSurfecesAngles(Pitch, Roll, Yaw, Flap);
    }

    public float getVelocity()
    {
        return rb.velocity.magnitude;
    }

    public float getAltitude()
    {
        return transform.position.y;
    }
}

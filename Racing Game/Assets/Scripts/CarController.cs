using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody _carRB;
    [SerializeField] private Transform[] _rayPoints;
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private Transform _accelerationPoint;
    [SerializeField] private GameObject[] _tires = new GameObject[4];
    [SerializeField] private GameObject[] _frontTireParent = new GameObject[2];
    [SerializeField] private TrailRenderer[] _skidMarks = new TrailRenderer[2];
    [SerializeField] private ParticleSystem[] _skidSmokes = new ParticleSystem[2];
    [SerializeField] private AudioSource _engineSFX, _skidSFX;


    [Header("Suspension")]
    [SerializeField] private float _springStiffness;
    [SerializeField] private float _damperStiffness;
    [SerializeField] private float _restLength;
    [SerializeField] private float _springTravel;
    [SerializeField] private float _wheelRadius;

    private int[] _wheelIsGrounded = new int[4];
    private bool _isGrounded = false;

    [Header("Input")]
    private float _moveInput = 0;
    private float _steerInput = 0;

    [Header("Car Settings")]
    [SerializeField] private float _acceleration = 10f;
    [SerializeField] private float _deceleration = 5f;
    [SerializeField] private float _maxSpeed = 50f;
    [SerializeField] private float _steerStrength = 30f;
    [SerializeField] private AnimationCurve _turningCurve;
    [SerializeField] private float _dragCoefficient = 10f;
    [SerializeField] private float _brakingDeceleration = 100f;
    [SerializeField] private float _brakingDragCoefficient = 0.5f;

    private Vector3 _currentCarLocalVelocity = Vector3.zero;
    private float _carVelocityRatio = 0;

    [Header("Visuals")]
    [SerializeField] private float _tireRotSpeed = 3000f;
    [SerializeField] private float _maxSteerAngle = 30f;
    [SerializeField] private float _minSideSkidVelocity = 1.2f;

    [Header("Audio")]
    [Range(0, 1)]
    [SerializeField] private float _minEnginePitch = 1f;
    [Range(1, 5)]
    [SerializeField] private float _maxEnginePitch = 5f;


    #region Unity Functions

    private void FixedUpdate()
    {
        Suspension();
        GroundCheck();
        CalculateCarVelocity();
        Movement();
        Visuals();
        EngineSound();
    }

    private void Update()
    {
        GetPlayerInput();
    }

    #endregion

    #region Input Handling

    private void GetPlayerInput()
    {
        _moveInput = Input.GetAxis("Vertical");
        _steerInput = Input.GetAxis("Horizontal");
    }

    #endregion

    #region Movement

    private void Movement()
    {
        if (_isGrounded)
        {
            Acceleration();
            Deceleration();
            Turn();
            SidewayDrag();
        }
    }

    private void Acceleration()
    {
        if (_currentCarLocalVelocity.z < _maxSpeed)
        {
            _carRB.AddForceAtPosition(_acceleration * _moveInput * transform.forward, _accelerationPoint.position, ForceMode.Acceleration);
        }
    }

    private void Deceleration()
    {
        _carRB.AddForceAtPosition((Input.GetKey(KeyCode.Space) ? _brakingDeceleration : _deceleration) * Mathf.Abs(_carVelocityRatio) * -transform.forward, _accelerationPoint.position, ForceMode.Acceleration);
    }

    private void Turn()
    {
        _carRB.AddRelativeTorque(_steerStrength * _steerInput * _turningCurve.Evaluate(Mathf.Abs(_carVelocityRatio)) * Mathf.Sign(_carVelocityRatio) * _carRB.transform.up, ForceMode.Acceleration);
    }
    private void SidewayDrag()
    {
        float currentSidewaySpeed = _currentCarLocalVelocity.x;

        float dragMagnitude = -currentSidewaySpeed * (Input.GetKey(KeyCode.Space) ? _brakingDragCoefficient : _dragCoefficient);

        Vector3 dragForce = transform.right * dragMagnitude;

        _carRB.AddForceAtPosition(dragForce, _carRB.worldCenterOfMass, ForceMode.Acceleration);
    }

    #endregion

    #region Visuals

    private void Visuals()
    {
        TireVisuals();
        VFX();
    }

    private void TireVisuals()
    {
        float steeringAngle = _maxSteerAngle * _steerInput;

        for (int i = 0; i < _tires.Length; i++)
        {
            if (i < 2)
            {
                _tires[i].transform.Rotate(Vector3.right, _tireRotSpeed * _carVelocityRatio * Time.deltaTime, Space.Self);

                _frontTireParent[i].transform.localEulerAngles = new Vector3(_frontTireParent[i].transform.localEulerAngles.x, steeringAngle, _frontTireParent[i].transform.localEulerAngles.z);
            }
            else
            {
                _tires[i].transform.Rotate(Vector3.right, _tireRotSpeed * _moveInput * Time.deltaTime, Space.Self);
            }
        }
    }

    private void VFX()
    {
        if (_isGrounded && Mathf.Abs(_currentCarLocalVelocity.x) > _minSideSkidVelocity && _carVelocityRatio > 0)
        {
            ToggleSkidMarks(true);
            ToggleSkidSmokes(true);
            ToggleSkidSound(true);
        }
        else
        {
            ToggleSkidMarks(false);
            ToggleSkidSmokes(false);
            ToggleSkidSound(false);
        }
    }

    private void ToggleSkidMarks(bool toggle)
    {
        foreach (var skidMark in _skidMarks)
        {
            skidMark.emitting = toggle;
        }
    }

    private void ToggleSkidSmokes(bool toggle)
    {
        foreach (var skidSmoke in _skidSmokes)
        {
            if (toggle)
            {
                skidSmoke.Play();
            }
            else
            {
                skidSmoke.Stop();
            }
        }
    }

    private void SetTirePosition(GameObject tire, Vector3 targetPosition)
    {
        tire.transform.position = targetPosition;
    }

    #endregion

    #region Audio

    private void EngineSound()
    {
        _engineSFX.pitch = Mathf.Lerp(_minEnginePitch, _maxEnginePitch, Mathf.Abs(_carVelocityRatio));
    }

    private void ToggleSkidSound(bool toggle)
    {
        _skidSFX.mute = !toggle;
    }

    #endregion

    #region Car Status Check

    private void GroundCheck()
    {
        int tempGroundedWheels = 0;

        for (int i = 0; i < _wheelIsGrounded.Length; i++)
        {
            tempGroundedWheels += _wheelIsGrounded[i];
        }

        if (tempGroundedWheels > 1)
        {
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }
    }

    private void CalculateCarVelocity()
    {
        _currentCarLocalVelocity = transform.InverseTransformDirection(_carRB.linearVelocity);
        _carVelocityRatio = _currentCarLocalVelocity.z / _maxSpeed;
    }

    #endregion

    #region Suspension Functions

    private void Suspension()
    {
        for (int i = 0; i < _rayPoints.Length; i++)
        {
            RaycastHit hit;
            float maxLength = _restLength + _springTravel;

            if (Physics.Raycast(_rayPoints[i].position, -_rayPoints[i].up, out hit, maxLength + _wheelRadius, _groundMask))
            {
                _wheelIsGrounded[i] = 1;

                float currentSpringLength = hit.distance - _wheelRadius;
                float springCompression = (_restLength - currentSpringLength) / _springTravel;

                // Calculate damper force (proporsional to the velocity of suspension compression)
                float springVelocity = Vector3.Dot(_carRB.GetPointVelocity(_rayPoints[i].position), _rayPoints[i].up);
                float dampForce = _damperStiffness * springVelocity;

                float springForce = _springStiffness * springCompression;

                float netForce = springForce - dampForce;

                _carRB.AddForceAtPosition(netForce * _rayPoints[i].up, _rayPoints[i].position);

                // Visuals
                SetTirePosition(_tires[i], hit.point + _rayPoints[i].up * _wheelRadius);

                Debug.DrawLine(_rayPoints[i].position, hit.point, Color.green);
            }
            else
            {
                _wheelIsGrounded[i] = 0;

                // Visuals
                SetTirePosition(_tires[i], _rayPoints[i].position - _rayPoints[i].up * maxLength);

                Debug.DrawLine(_rayPoints[i].position, _rayPoints[i].position + (_wheelRadius + maxLength) * -_rayPoints[i].up, Color.red);
            }
        }
    }

    #endregion
}

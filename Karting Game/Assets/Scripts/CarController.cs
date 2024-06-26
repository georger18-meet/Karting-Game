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


    private Vector3 _currentCarLocalVelocity = Vector3.zero;
    private float _carVelocityRatio = 0;


    #region Unity Functions

    private void FixedUpdate()
    {
        Suspension();
        GroundCheck();
        CalculateCarVelocity();
        Movement();
    }

    private void Update()
    {
        GetPlayerInput();
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
        _carRB.AddForceAtPosition(_acceleration * _moveInput * transform.forward, _accelerationPoint.position, ForceMode.Acceleration);
    }

    private void Deceleration()
    {
        _carRB.AddForceAtPosition(_deceleration * _moveInput * -transform.forward, _accelerationPoint.position, ForceMode.Acceleration);
    }

    private void Turn()
    {
        _carRB.AddRelativeTorque(_steerStrength * _steerInput * _turningCurve.Evaluate(Mathf.Abs(_carVelocityRatio)) * Mathf.Sign(_carVelocityRatio) * _carRB.transform.up, ForceMode.Acceleration);
    }
    private void SidewayDrag()
    {
        float currentSidewaySpeed = _currentCarLocalVelocity.x;

        float dragMagnitude = -currentSidewaySpeed * _dragCoefficient;

        Vector3 dragForce = transform.right * dragMagnitude;

        _carRB.AddForceAtPosition(dragForce,_carRB.worldCenterOfMass, ForceMode.Acceleration);
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
        _currentCarLocalVelocity = transform.InverseTransformDirection(_carRB.velocity);
        _carVelocityRatio = _currentCarLocalVelocity.z / _maxSpeed;
    }

    #endregion

    #region Input Handling

    private void GetPlayerInput()
    {
        _moveInput = Input.GetAxis("Vertical");
        _steerInput = Input.GetAxis("Horizontal");
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

                float springVelocity = Vector3.Dot(_carRB.GetPointVelocity(_rayPoints[i].position), _rayPoints[i].up);
                float dampForce = _damperStiffness * springVelocity;

                float springForce = _springStiffness * springCompression;

                float netForce = springForce - dampForce;

                _carRB.AddForceAtPosition(netForce * _rayPoints[i].up, _rayPoints[i].position);

                Debug.DrawLine(_rayPoints[i].position, hit.point, Color.green);
            }
            else
            {
                _wheelIsGrounded[i] = 0;

                Debug.DrawLine(_rayPoints[i].position, _rayPoints[i].position + (_wheelRadius + maxLength) * -_rayPoints[i].up, Color.red);
            }
        }
    }

    #endregion
}

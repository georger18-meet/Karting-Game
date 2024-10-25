using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Vector2 _moveInput;

    [SerializeField] private float _drivingSpeed;
    [SerializeField] private float _maxFwdSpeed = 4f;
    [SerializeField] private float _maxReverseSpeed = 2f;
    [SerializeField] private float _accelerationSpeed = 0.01f;
    [SerializeField] private float _decelerationSpeed = 0.01f;
    [SerializeField] private int _steeringAngle = 15;

    [SerializeField] private GameObject _steeringWheel, _wheelFR, _wheelFL, _wheelBR, _wheelBL;

    [Header("Physics")]
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private float _gravityModifier = 10f;
    [SerializeField] private float _sphereRadius = 0.1f;
    [SerializeField] private Vector3 _sphereOffset = new Vector3(0, 0.09f, 0);
    [SerializeField] private Vector3 _velocity;
    [SerializeField] private bool _isGrounded;
    [SerializeField] private bool _gravityEnabled = true;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Gravity();
        HandleMovement();
        HandleAnimations();
    }


    // ----- Functions -----

    private void HandleMovement()
    {
        Driving();
        Steering();
    }

    private void Driving()
    {
        // Getting Input
        _moveInput.y = Input.GetAxisRaw("Vertical");

        // Accelerating
        if (_moveInput.y > 0)
        {
            if (_drivingSpeed >= _maxFwdSpeed)
            {
                _drivingSpeed = _maxFwdSpeed;
            }
            else
            {
                _drivingSpeed += _moveInput.y * _accelerationSpeed;
            }
        }
        // Decelerating
        else if (_moveInput.y < 0)
        {
            if (_drivingSpeed <= -_maxReverseSpeed)
            {
                _drivingSpeed = -_maxReverseSpeed;
            }
            else
            {
                _drivingSpeed += _moveInput.y * _decelerationSpeed;
            }
        }
        // Not Accelerating Nor Decelerating
        else
        {
            if (_drivingSpeed > 0.1f)
            {
                _drivingSpeed -= _decelerationSpeed;
            }
            else if (_drivingSpeed < -0.1f)
            {
                _drivingSpeed += _decelerationSpeed;
            }
            else
            {
                _drivingSpeed = 0;
            }
        }

        // Moving Transform
        transform.Translate(0, 0, _drivingSpeed * Time.deltaTime);
    }

    private void Steering()
    {
        // Getting Input
        _moveInput.x = Input.GetAxisRaw("Horizontal");

        if (_moveInput.x != 0)
        {
            if (_drivingSpeed > 0)
            {
                transform.Rotate(0, _steeringAngle * _moveInput.x * Time.deltaTime, 0);
            }
            else if (_drivingSpeed < 0)
            {
                transform.Rotate(0, _steeringAngle * -_moveInput.x * Time.deltaTime, 0);
            }
        }
    }

    private void Gravity()
    {
        _isGrounded = Physics.CheckSphere(transform.position + _sphereOffset, _sphereRadius, _groundMask);

        if (_gravityEnabled)
        {
            if (_isGrounded)
            {
                _velocity.y = 0;
            }
            else
            {
                _velocity.y -= _gravityModifier * Time.deltaTime;
            }


            transform.Translate(_velocity * Time.deltaTime);
        }
    }

    private void HandleAnimations()
    {
        // Driving
        _wheelBR.transform.Rotate(_drivingSpeed / 2, 0, 0);
        _wheelBL.transform.Rotate(_drivingSpeed / 2, 0, 0);

        _wheelFR.transform.Rotate(_drivingSpeed / 2, 0, 0);
        _wheelFL.transform.Rotate(_drivingSpeed / 2, 0, 0);

        // Steering
        _steeringWheel.transform.localRotation = Quaternion.Euler(0, 0, _steeringAngle * -_moveInput.x);
    }


    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (_isGrounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(transform.position + _sphereOffset, _sphereRadius);
    }
}

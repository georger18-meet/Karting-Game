using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartController : MonoBehaviour
{
    [SerializeField] private Vector2 _moveInput;

    [SerializeField] private float _drivingSpeed;
    [SerializeField] private float _maxFwdSpeed = 4f;
    [SerializeField] private float _maxReverseSpeed = 2f;
    [SerializeField] private float _accelerationSpeed = 0.01f;
    [SerializeField] private float _decelerationSpeed = 0.01f;
    [SerializeField] private int _steeringAngle = 15;

    [SerializeField] private GameObject _steeringWheel, _wheelFR, _wheelFL, _wheelBR, _wheelBL;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
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
        transform.Translate(0, 0, _drivingSpeed * Time.fixedDeltaTime);
    }

    private void Steering()
    {
        // Getting Input
        _moveInput.x = Input.GetAxisRaw("Horizontal");

        if (_moveInput.x != 0)
        {
            if (_drivingSpeed > 0)
            {
                transform.Rotate(0, _steeringAngle * _moveInput.x * Time.fixedDeltaTime, 0);
            }
            else if (_drivingSpeed < 0)
            {
                transform.Rotate(0, _steeringAngle * -_moveInput.x * Time.fixedDeltaTime, 0);
            }
        }
    }
}

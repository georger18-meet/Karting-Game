using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody _carRB;
    [SerializeField] private Transform[] _rayPoints;
    [SerializeField] private LayerMask _groundMask;

    [Header("Suspension")]
    [SerializeField] private float _springStiffness;
    [SerializeField] private float _damperStiffness;
    [SerializeField] private float _restLength;
    [SerializeField] private float _springTravel;
    [SerializeField] private float _wheelRadius;


    private void FixedUpdate()
    {
        Suspension();
    }


    private void Suspension()
    {
        foreach (Transform rp in _rayPoints)
        {
            RaycastHit hit;
            float maxLength = _restLength + _springTravel;

            if (Physics.Raycast(rp.position, -rp.up, out hit, maxLength + _wheelRadius, _groundMask))
            {
                float currentSpringLength = hit.distance - _wheelRadius;
                float springCompression = (_restLength - currentSpringLength) / _springTravel;

                float springVelocity = Vector3.Dot(_carRB.GetPointVelocity(rp.position), rp.up);
                float dampForce = _damperStiffness * springVelocity;

                float springForce = _springStiffness * springCompression;

                float netForce = springForce - dampForce;

                _carRB.AddForceAtPosition(netForce * rp.up, rp.position);

                Debug.DrawLine(rp.position, hit.point, Color.green);
            }
            else
            {
                Debug.DrawLine(rp.position, rp.position + (_wheelRadius + maxLength) * -rp.up, Color.red);
            }
        }
    }
}

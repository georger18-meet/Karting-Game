using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _lookAtTarget;
    [SerializeField] private Transform[] _camPositions;
    [SerializeField] private float _smoothTime = 1f;
    [SerializeField] private int _camIndex;


    private void Update()
    {
        SwitchPOV();
    }

    private void FixedUpdate()
    {
        CamBehavior();
    }


    private void CamBehavior()
    {
        Vector3 velocity = Vector3.zero;

        if (_camIndex == _camPositions.Length - 1)
        {
            transform.position = _camPositions[_camIndex].position;
        }
        else
        {
            transform.position = Vector3.SmoothDamp(transform.position, _camPositions[_camIndex].position, ref velocity, _smoothTime * Time.deltaTime);
        }

        transform.LookAt(_lookAtTarget);
    }

    private void SwitchPOV()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            _camIndex++;
            if (_camIndex >= _camPositions.Length)
            {
                _camIndex = 0;
            }
        }
    }
}

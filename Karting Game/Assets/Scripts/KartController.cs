using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartController : MonoBehaviour
{
    [SerializeField] private Vector2 moveInput;

    [SerializeField] private Vector3 moveVect;
    [SerializeField] private float MaxFwdSpeed = 4f;
    [SerializeField] private float MaxReverseSpeed = 2f;
    [SerializeField] private float AccelerationSpeed = 1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.x = Input.GetAxisRaw("Horizontal");

        if (moveInput.y > 0)
        {
            moveVect.z = moveInput.y * MaxFwdSpeed;
        }
        else if (moveInput.y < 0)
        {
            moveVect.z = moveInput.y * MaxReverseSpeed;
        }
        else
        {
            moveVect.z = 0;
        }

        transform.Translate(moveVect * Time.fixedDeltaTime);
    }
}

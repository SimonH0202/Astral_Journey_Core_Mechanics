using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    //SphereCast variables
    public LayerMask layerMask;

    //private variables
    private Transform cameraTransform;
    private Transform target;
    private Vector3 inputDirection;



    // Start is called before the first frame update
    void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        var forward = cameraTransform.forward;
        var right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        inputDirection = forward + right;
        inputDirection = inputDirection.normalized;

        RaycastHit info;

        if (Physics.SphereCast(transform.position, 3f, inputDirection, out info, 15, layerMask))
        {
            target = info.collider.transform;
        }


    }
    void OnDrawGizmos()
    {
        if (target != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(target.position, 1);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Test : MonoBehaviour
{
    CharacterController characterController;
    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        characterController.transform.DOMove(new Vector3(0, 0, 0), 1.0f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

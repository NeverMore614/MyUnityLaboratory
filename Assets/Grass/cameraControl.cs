using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraControl : MonoBehaviour
{
    // Start is called before the first frame update
    Vector3 direction = Vector3.zero;
    Vector2 rotation = Vector2.zero;
    void Start()
    {
        
    }

    private void Update()
    {
        direction = Vector3.zero;
        rotation.y += Input.GetAxis("Mouse X");
        rotation.x -= Input.GetAxis("Mouse Y");
        if (Input.GetKey(KeyCode.W))
        {
            direction += transform.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            direction -= transform.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            direction -= transform.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            direction += transform.right;
        }
        transform.rotation = Quaternion.Euler(rotation.x, rotation.y, 0);
        transform.position += Vector3.Normalize(direction) * 5 * Time.deltaTime;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }
}

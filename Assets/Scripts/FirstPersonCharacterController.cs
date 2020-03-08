using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FirstPersonCharacterController : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private Camera camera;

    private Rigidbody rigidbody;
    // Start is called before the first frame update
    void Start()
    {
        rigidbody = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            rigidbody.AddForce(new Vector3(0, jumpForce, 0));
        }

        var direction = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
        var angles = camera.transform.rotation.eulerAngles;
        angles += direction;
        camera.transform.rotation = Quaternion.Euler(angles);

        var movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (movement.magnitude > double.Epsilon)
        {
            var yaw = Quaternion.Euler(new Vector3(0, angles.y, 0));
            movement = yaw * movement;
            rigidbody.velocity = movement * Time.deltaTime * speed + new Vector3(0, rigidbody.velocity.y, 0);
        }
        else 
        {
            rigidbody.velocity = new Vector3(0, rigidbody.velocity.y, 0);
        }
    }
}

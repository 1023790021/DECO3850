using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstronautController : MonoBehaviour
{
    public float rotateSpeed = 100f; // Rotation speed
    public float moveSpeed = 5f; // Movement speed
    public float jumpForce = 30f; // Jump speed
    public float gravity = 1.62f; // Gravity setting
    public float groundDistance = 0.3f;
    public float pickupRange = 2f;

    private Rigidbody rb; // Rigidbody component
    public Transform handTransform; // Hand transform
    public GameObject Model; // Model object
    public Animator animator; // Animator component
    private bool isGrounded; // Grounded flag


    void Start()
    {
        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();

        // Lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Get the Animator component
        animator = Model.GetComponent<Animator>();

        // Adjust the global gravity setting
        Physics.gravity = new Vector3(0f, -gravity, 0f);
    }

    void Update()
    {
        // Check if the astronaut is on the ground
        isGrounded = Physics.Raycast(transform.position, -Vector3.up, groundDistance);

        // Check if the Escape key was pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Toggle the cursor lock and visibility
            Cursor.lockState = (Cursor.lockState == CursorLockMode.Locked) ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = (Cursor.lockState == CursorLockMode.None);
        }

        // Get keyboard input
        float horizontalInput = Input.GetAxis("Horizontal"); // Horizontal input (left/right arrow keys or A/D keys)
        float verticalInput = Input.GetAxis("Vertical"); // Vertical input (up/down arrow keys or W/S keys)

        // Control the astronaut's movement based on input
        Vector3 movement = new Vector3(0f, 0f, verticalInput) * moveSpeed * Time.deltaTime;
        transform.Translate(movement);

        // Rotate the astronaut based on horizontal input
        transform.Rotate(0f, horizontalInput * rotateSpeed * Time.deltaTime, 0f);
        
        // Rotate the astronaut based on horizontal input
        if (horizontalInput < 0)
        {
            TurnLeft();
        }
        else if (horizontalInput > 0)
        {
            TurnRight();
        }

        // Check if the jump key was pressed
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // Apply an upward force
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        Physics.gravity = new Vector3(0f, -30f, 0f);

        // Check if the astronaut is holding an object
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (handTransform.childCount == 0)
            {
                PickObject();
            }
            else
            {
                DropObject();
            }
        }
    }

    void TurnLeft()
    {
        transform.Rotate(0f, -rotateSpeed * Time.deltaTime, 0f);
    }

    void TurnRight()
    {
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
    }
    
    void Walk()
    {
        rb.AddForce(Vector3.forward * moveSpeed, ForceMode.Impulse);
    }

    void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void PickObject()
    {
        // Find all nearby objects
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, pickupRange);

        // Find the nearest object that can be picked up
        foreach (Collider collider in nearbyObjects)
        {
            if (collider.gameObject.CompareTag("Pickupable"))
            {
                // Pick up the object
                Model = collider.gameObject;

                // Make the astronaut the parent of the object
                Model.transform.SetParent(handTransform);

                // Position the object at the pickup point
                Model.transform.position = handTransform.position;

                // Prevent the object from being affected by physics while it's being held
                Rigidbody objectRb = Model.GetComponent<Rigidbody>();
                if (objectRb != null)
                {
                    objectRb.isKinematic = true;
                }
                break;
            }
        }

        animator.SetTrigger("TrpickUp");
    }

    void DropObject()
    {
        if (Model != null)
        {
            // Remove the astronaut as the parent of the object
            Model.transform.SetParent(null);

            // Allow the object to be affected by physics again
            Rigidbody objectRb = Model.GetComponent<Rigidbody>();
            if (objectRb != null)
            {
                objectRb.isKinematic = false;
            }

            // Forget the object
            Model = null;
        }

        animator.SetTrigger("TrpickUp");
    }

    public void RotateAstronaut(string command)
    {
        Debug.Log("Command received: " + command); // 日志记录接收到的命令
        switch (command)
        {
            case "Turning Left":
                if (isGrounded) TurnLeft();
                break;
            case "Turning Right":
                if (isGrounded) TurnRight();
                break;
            case "Walking":
                if (isGrounded) Walk();
                break;
            case "Jumping":
                if (isGrounded) Jump();
                break;
            case "pickup":
                if (isGrounded) PickObject();
                break;
            case "putdown":
                if (isGrounded) DropObject();
                break;
            default:
                Debug.LogError("Unrecognized command: " + command); // 未识别的命令错误日志
                break;
        }
    }
}
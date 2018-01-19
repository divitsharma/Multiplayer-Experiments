using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour {

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private float cameraRotLimit = 85f;

    private Vector3 velocity = Vector3.zero;
    private Vector3 rotation = Vector3.zero;
    private float cameraRotX = 0f;
    private float currentCameraRotX = 0f;
    private Vector3 thrusterForce = Vector3.zero;

    private Rigidbody rb;



	void Start ()
    {
        rb = GetComponent<Rigidbody>();
	}

    void FixedUpdate ()
    {
        PerformMovement();
        PerformRotation();
    }

	
	public void Move (Vector3 _velocity)
    {
        velocity = _velocity;
    }

    public void Rotate (Vector3 _rotation)
    {
        rotation = _rotation;
    }
    
    public void RotateCamera (float _cameraRotX)
    {
        cameraRotX = _cameraRotX;
    }

    public void ApplyThruster (Vector3 _thrusterForce)
    {
        thrusterForce = _thrusterForce;
    }


    // perform movement based on velocity variable
    void PerformMovement ()
    {
        if (velocity != Vector3.zero)
        {
            // moves player to a new position
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        }

        // thrusters
        if (thrusterForce != Vector3.zero)
        {
            rb.AddForce(thrusterForce * Time.fixedDeltaTime, ForceMode.Acceleration);
        }
    }

    void PerformRotation ()
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
        if (cam != null)
        {
            // set and clamp current rotation
            currentCameraRotX -= cameraRotX;
            currentCameraRotX = Mathf.Clamp(currentCameraRotX, -cameraRotLimit, cameraRotLimit);
            // apply current camera rotation
            cam.transform.localEulerAngles = new Vector3(currentCameraRotX, 0f, 0f);
        }
    }


}

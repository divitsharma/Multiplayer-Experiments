using UnityEngine;


[RequireComponent(typeof(ConfigurableJoint))]
[RequireComponent(typeof(PlayerMotor))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour {

    [SerializeField] // makes field show up in inspector
    private float speed = 5f;

    [SerializeField]
    private float sensitivity = 3f;

    [SerializeField]
    private float thrusterForce = 1000f;
    [SerializeField]
    private float thrusterFuelBurnSpeed = 1f; // amount of time thruster can be used
    [SerializeField]
    private float thrusterFuelRegenSpeed = 0.3f;

    private float thrusterFuelAmount = 1f;

    public float GetThrusterFuelAmount ()
    {
        return thrusterFuelAmount;
    }

    [SerializeField]
    private LayerMask environmentMask;

    [Header("Spring settings")]
    [SerializeField]
    private float jointSpring = 20f;
    [SerializeField]
    private float jointMaxForce = 40f;

    // Component caching
    private PlayerMotor motor;
    private ConfigurableJoint joint;
    private Animator animator;

    void Start()
    {
        motor = GetComponent<PlayerMotor>();
        joint = GetComponent<ConfigurableJoint>();
        animator = GetComponent<Animator>();

        SetJointSettings(jointSpring);
    }


    void Update()
    {
        // Set target point for spring
        RaycastHit _hit;
        if (Physics.Raycast(transform.position, Vector3.down, out _hit, 100f, environmentMask))
        {
            joint.targetPosition = new Vector3(0f, -_hit.point.y, 0f);
        }
        else
        {
            joint.targetPosition = new Vector3(0f, 0f, 0f);
        }

        // calculate movement velocity as a 3d vector
        float _xMove = Input.GetAxis("Horizontal");
        float _zMove = Input.GetAxis("Vertical");

        Vector3 _moveHorizontal = transform.right * _xMove;
        Vector3 _moveVertical = transform.forward * _zMove;

        // get final move direction
        Vector3 _velocity = (_moveHorizontal + _moveVertical) * speed;

        // Animate movement
        animator.SetFloat("ForwardVelocity", _zMove);

        // Apply movement
        motor.Move(_velocity);


        // Calculate rotation as a 3d Vector for turning
        float _yRot = Input.GetAxisRaw("Mouse X");

        Vector3 _rot = new Vector3(0f, _yRot, 0f) * sensitivity;

        // Apply rotation
        motor.Rotate(_rot);


        // Calculate camera rotation as a 3d Vector
        float _xRot = Input.GetAxisRaw("Mouse Y");

        float _cameraRotX = _xRot * sensitivity; // degree to rotate

        // Apply rotation
        motor.RotateCamera(_cameraRotX);


        // Get thruster force
        Vector3 _thrusterForce = Vector3.zero;
        if (Input.GetButton("Jump") && thrusterFuelAmount > 0f)
        {
            thrusterFuelAmount -= thrusterFuelBurnSpeed * Time.deltaTime;

            if (thrusterFuelAmount >= 0.05f)
            {
                _thrusterForce = Vector3.up * thrusterForce;
                SetJointSettings(0f);
            }
        }
        else
        {
            thrusterFuelAmount += thrusterFuelRegenSpeed * Time.deltaTime;

            SetJointSettings(jointSpring);
        }

        thrusterFuelAmount = Mathf.Clamp(thrusterFuelAmount, 0, 1);

        // Apply thruster force
        motor.ApplyThruster(_thrusterForce);
    }


    public void SetJointSettings (float _jointSpring)
    {
        joint.yDrive = new JointDrive { positionSpring = _jointSpring, maximumForce = jointMaxForce };
    }
    public void ResetJointSettings ()
    {
        joint.yDrive = new JointDrive { positionSpring = jointSpring, maximumForce = jointMaxForce };
    }

}

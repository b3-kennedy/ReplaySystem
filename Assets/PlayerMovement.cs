using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
  public enum PlayerState {NORMAL, SPRINT};
    public PlayerState state;

    

    
    public float normalSpeed;
    public float sprintSpeed;
    public float acceleration;
    public float groundDrag;

    public Transform groundCheck;

    public Transform orientation;

    public float jumpForce;
    public float airMultiplier;

    float horizontal;
    float vertical;

    

    bool normalOnLand = false;

    bool wasAirborne;

    Vector3 moveDir;
    Rigidbody rb;

    public float landingSlowdownFactor = 0.5f; // Percentage of speed after landing
    public float landingSlowdownDuration = 1.0f; // Time in seconds to recover full speed
    private float currentSlowdownTime = 0f;
    private bool isRecoveringSpeed = false;

    float targetSpeedOnLand;

    bool isSprinting = false;
    public float sprintMultiplier;
    float smoothVel;


    [Header("Debugging")]
    public float targetSpeed;
    public float speed;
    public float magnitude;

    ReplayManager replayInstance;

    string objectId;



    // Start is called before the first frame update
    void Start()
    {
        speed = normalSpeed;
        targetSpeed = normalSpeed;
        state = PlayerState.NORMAL;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        replayInstance = ReplayManager.Instance;
        objectId = GetComponent<ObjectId>().GetId();
    }

    //public override void OnNetworkSpawn()
    //{
    //    if (IsOwner)
    //    {
    //        rb.isKinematic = false;
    //    }
    //    else
    //    {
    //        rb.isKinematic = true;
    //    }
    //}

    // Update is called once per frame
    void Update()
    {
        

        magnitude = rb.linearVelocity.magnitude;

        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        // Calculate movement direction
        Vector3 inputDirection = new Vector3(horizontal, 0, vertical).normalized;

        // Check if the player is moving forward relative to their orientation
        bool isMovingForward = Input.GetKey(KeyCode.W);

        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            Jump();
        }



        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (isMovingForward && IsGrounded())
            {
                isSprinting = true;
                state = PlayerState.SPRINT;
                targetSpeed = sprintSpeed;
            }
            else if(!isMovingForward && IsGrounded())
            {
                isSprinting = false;
                state = PlayerState.NORMAL;
                targetSpeed = normalSpeed;
                speed = normalSpeed;
            }

        }
        else if (Input.GetKeyUp(KeyCode.LeftShift) && IsGrounded())
        {
            isSprinting = false;
            state = PlayerState.NORMAL;
            speed = normalSpeed;
            targetSpeed = normalSpeed;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift) && !IsGrounded())
        {
            normalOnLand = true;
        }

        if(IsGrounded() && normalOnLand)
        {
            isSprinting = false;
            state = PlayerState.NORMAL;
            speed = normalSpeed;
            targetSpeed = normalSpeed;
            normalOnLand = false;
        }



        bool grounded = IsGrounded();



        if(wasAirborne && grounded)
        {
            OnLand();
            wasAirborne = false;
        }

        if (!grounded)
        {
            wasAirborne = true;
        }

        if (isSprinting)
        {
            speed = Mathf.MoveTowards(speed, sprintSpeed, Time.deltaTime * sprintMultiplier);
        }

        // Gradually recover speed after landing
        if (isRecoveringSpeed)
        {
            currentSlowdownTime += Time.deltaTime;
            speed = Mathf.Lerp(targetSpeedOnLand * landingSlowdownFactor, targetSpeedOnLand, currentSlowdownTime / landingSlowdownDuration);

            if (currentSlowdownTime >= landingSlowdownDuration)
            {
                isRecoveringSpeed = false;
                speed = targetSpeedOnLand; // Ensure full speed is restored
            }
        }

        SpeedControl();

        if (IsGrounded())
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0;
        }
    }

    void OnLand()
    {
        landingSlowdownFactor = (1 - (targetSpeed / 10));
        targetSpeedOnLand = targetSpeed;
        //rb.velocity = Vector3.zero;
        StartLandingSlowdown();
    }

    void StartLandingSlowdown()
    {
        isRecoveringSpeed = true;
        currentSlowdownTime = 0f;
        speed = targetSpeedOnLand * landingSlowdownFactor; // Initial slowdown
    }

    public bool IsGrounded()
    {
        bool rayHit = Physics.Raycast(groundCheck.position, -transform.up, 0.51f);

        return (isOnSlope() || rayHit);
    }

    public bool isOnSlope()
    {
        if(Physics.Raycast(groundCheck.position, -transform.up, out RaycastHit hit ,1f))
        {
            if(hit.normal.y != 1)
            {
                return true;
            }
        }
        return false;
        //Debug.Log()
        //return );
    }

    private void FixedUpdate()
    {
        MovePlayer();
        MovementAction action = new(replayInstance.GetReplayTime(), rb.position, objectId);
        replayInstance.actions.Add(action);
    }

    void MovePlayer()
    {

        moveDir = orientation.forward * vertical + orientation.right * horizontal;

        if (IsGrounded())
        {
            rb.AddForce(moveDir.normalized * speed * 10f, ForceMode.Force);
            
        }
        else
        {
            rb.AddForce(moveDir.normalized * speed * 10f * airMultiplier, ForceMode.Force);
        }
        
    }

    void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if(flatVel.magnitude > speed)
        {
            Vector3 limitedVel = flatVel.normalized * speed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
}

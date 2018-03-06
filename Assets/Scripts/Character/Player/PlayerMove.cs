using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float crouchSpeed = 1f;
    public float turnSpeed = 5f;
    public float jumpSpeed = 10f;
    public float sideSpeed = 10f;
    public AudioSource audioSource;
    public AudioClip runClip;
    public AudioClip jumpClip;
    public AudioClip landClip;
    public FloatVariable playerHealth;

    [HideInInspector]
    public bool isInGrass;
    [HideInInspector]
    public bool isCrouch;
    [HideInInspector]
    public Animator anim;

    private Transform cam;
    private Rigidbody rb;

    private float groundCheckDistance = 0.2f;
    private float[] speeds;
    private int currSpeedIndex = 0;
    private float currSpeed;
    private bool isGround;
    private LayerMask terrainLayer;

    private void Start()
    {
        cam = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
        if (anim == null) anim = GetComponentInChildren<Animator>();

        speeds = new float[3];
        speeds[0] = runSpeed;
        speeds[1] = walkSpeed;
        speeds[2] = crouchSpeed;
        currSpeed = speeds[currSpeedIndex];

        terrainLayer = 1 << LayerMask.NameToLayer("Terrain");
    }

    private void Update()
    {
        if (playerHealth.Value <= 0)
        {
            return;
        }

        if (transform.position.y < -100)
        {
            transform.position = new Vector3(35, 10, 33);
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.C))
#else
        if(TouchButton.GetButtonDown("Speed"))
#endif
        {
            currSpeedIndex++;
            currSpeedIndex %= 3;
            currSpeed = speeds[currSpeedIndex];
            if (currSpeedIndex == 2)
            {
                isCrouch = true;
            }
            else
            {
                isCrouch = false;
            }
            anim.SetBool(Hashes.CrouchBool, isCrouch);
        }

        CheckIsGround();

        if (isGround)
        {
#if UNITY_EDITOR
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
#else
            float h = Joystick.GetAxis("Horizontal");
            float v = Joystick.GetAxis("Vertical");
#endif
            var animState = anim.GetCurrentAnimatorStateInfo(0).shortNameHash;
            if (animState == Hashes.LocomotionState || animState == Hashes.CrouchState)
            {
                Locomotion(h, v);
            }
            else if (animState == Hashes.FightMoveState)
            {
                FightMove(h, v);
            }
        }
        else
        {
            rb.AddForce(Physics.gravity);         // extra gravity
            if (rb.velocity.y < 0)
            {
                groundCheckDistance = 0.2f;
            }
        }
    }

    private void Locomotion(float h, float v)
    {
        Vector3 move;

        move = v * cam.forward + h * cam.right;
        move = new Vector3(move.x, 0, move.z);
        var forward = Vector3.Dot(transform.forward, move.normalized) * transform.forward * currSpeed;
        Translate(forward);
        Rotate(move);

#if UNITY_EDITOR
        if (Input.GetButtonDown("Jump"))
#else
        if (TouchButton.GetButtonDown("Jump"))
#endif
        {
            Jump();
        }
    }

    private void FightMove(float h, float v)
    {
        var move = (v * transform.forward + h * transform.right).normalized * runSpeed;
        Translate(move);
        var rotate = new Vector3(cam.forward.x, 0, cam.forward.z);
        Rotate(rotate);
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, jumpSpeed, rb.velocity.z);
        groundCheckDistance = 0.01f;
        isGround = false;

        anim.SetTrigger(Hashes.JumpTrigger);
        audioSource.clip = jumpClip;
        audioSource.Play();
    }

    private void Translate(Vector3 vector)
    {
        if (CheckGroundNormal())
        {
            rb.velocity = vector;
        }
        else
        {
            rb.velocity = Vector3.zero;
        }
        anim.SetFloat(Hashes.SpeedFloat, vector.magnitude);
    }

    private void Rotate(Vector3 move)
    {
        if (move.magnitude == 0)
        {
            return;
        }
        var angle = Vector3.Angle(transform.forward, move);
        var normal = Vector3.Cross(transform.forward, move);
        angle = Mathf.Sign(Vector3.Dot(transform.up, normal)) * angle;
        var rotation = transform.rotation * Quaternion.Euler(0, angle, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, turnSpeed * Time.deltaTime);
    }

    private void CheckIsGround()
    {
        if (Physics.Raycast(transform.position + new Vector3(0, 0.1f, 0), Vector3.down, groundCheckDistance, terrainLayer))
        {
            isGround = true;
        }
        else if (Physics.Raycast(transform.position + new Vector3(0.1f, 0.1f, 0), Vector3.down, groundCheckDistance, terrainLayer) ||
            Physics.Raycast(transform.position + new Vector3(-0.1f, 0.1f, 0), Vector3.down, groundCheckDistance, terrainLayer) ||
            Physics.Raycast(transform.position + new Vector3(0, 0.1f, 0.1f), Vector3.down, groundCheckDistance, terrainLayer) ||
            Physics.Raycast(transform.position + new Vector3(0, 0.1f, -0.1f), Vector3.down, groundCheckDistance, terrainLayer))
        {
            isGround = true;
        }
        else
        {
            isGround = false;
        }
    }

    private bool CheckGroundNormal()
    {
        var result = true;
        RaycastHit hit;
        if (Physics.Raycast(transform.position + new Vector3(0, 0.3f, 0), transform.forward, out hit, 0.6f))
        {
            if (Vector3.Dot(hit.normal, Vector3.up) < 0.7)
            {
                result = false;
            }
        }
        return result;
    }

    public void PlayRunAudio()
    {
        if (isCrouch)
        {
            return;
        }
        if (audioSource.clip != runClip)
        {
            audioSource.clip = runClip;
        }
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}

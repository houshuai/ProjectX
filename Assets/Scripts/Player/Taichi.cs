using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Taichi : MonoBehaviour
{
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float crouchSpeed = 1f;
    public float turnSpeed = 5f;
    public float jumpSpeed = 8f;
    public float health = 100f;
    public AudioClip runClip;
    public AudioClip jumpClip;
    public AudioClip landClip;
    public AudioClip hitClip;

    [HideInInspector]
    public bool isInGrass;
    [HideInInspector]
    public bool isDead;

    private Transform cam;
    private Animator anim;
    private Rigidbody rb;
    private AudioSource audioSource;

    private Vector3 forward;
    private float groundCheckDistance = 0.11f;
    private bool isCrouch = false;
    private float[] speeds;
    private int currSpeedIndex = 0;
    private float currSpeed;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main.transform;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        speeds = new float[3];
        speeds[0] = runSpeed;
        speeds[1] = walkSpeed;
        speeds[2] = crouchSpeed;
        currSpeed = speeds[currSpeedIndex];
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.C))
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

        if (CheckIsGround())
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            if (h != 0 || v != 0)
            {
                Vector3 move;
                move = v * cam.forward + h * cam.right;
                move = new Vector3(move.x, 0, move.z);
                forward = Vector3.Dot(transform.forward, move.normalized) * transform.forward * currSpeed * Time.deltaTime;
                Translate(forward);
                Rotation(move);

                if (Input.GetButtonDown("Jump"))
                {
                    Jump();
                }
            }
            else
            {
                forward = Vector3.zero;
                anim.SetFloat(Hashes.SpeedFloat, 0f);

                if (audioSource.clip == runClip && audioSource.isPlaying)
                {
                    audioSource.Stop();
                }

            }

        }
        else
        {
            rb.MovePosition(transform.position + forward);
            if (rb.velocity.y < 0)
            {
                groundCheckDistance = 0.11f;
            }
        }



    }

    private void Jump()
    {
        var upOffset = forward + transform.up * jumpSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + upOffset);
        groundCheckDistance = 0.01f;
        anim.SetTrigger(Hashes.JumpTrigger);

        audioSource.clip = jumpClip;
        audioSource.loop = false;
        audioSource.Play();
    }

    private void Translate(Vector3 vector)
    {
        rb.MovePosition(transform.position + vector);
        anim.SetFloat(Hashes.SpeedFloat, currSpeed);

        if (audioSource.clip != runClip)
        {
            audioSource.clip = runClip;
            audioSource.loop = true;
        }
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    private void Rotation(Vector3 move)
    {
        var rot = Quaternion.LookRotation(move);
        var target = Quaternion.Lerp(transform.rotation, rot, turnSpeed);
        transform.rotation = target;

    }

    private bool CheckIsGround()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundCheckDistance);
    }

}

using UnityEngine;

public class CharacterMove : MonoBehaviour
{
    public float[] speeds;
    public float turnSpeed = 2;

    protected Animator anim;
    protected Rigidbody rb;
    private Transform cam;
    private float currSpeed;
    private int currSpeedIndex = 0;
    private bool isRide;

    protected void Initial()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        cam = Camera.main.transform;
        currSpeedIndex = 0;
        currSpeed = speeds[currSpeedIndex];
    }

    protected void ChangeCurrSpeed()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.C))
#else
        if(TouchButton.GetButtonDown("Speed"))
#endif
        {
            currSpeedIndex++;
            currSpeedIndex %= speeds.Length;
            currSpeed = speeds[currSpeedIndex];
        }
    }

    protected void Move()
    {
#if UNITY_EDITOR
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
#else
        float h = Joystick.GetAxis("Horizontal");
        float v = Joystick.GetAxis("Vertical");
#endif

        var move = v * cam.forward + h * cam.right;
        move = new Vector3(move.x, 0, move.z);

        Rotate(move);

        Translate(move);
    }

    private void Translate(Vector3 move)
    {
        rb.velocity = Vector3.Dot(transform.forward, move.normalized) * transform.forward * currSpeed;
        anim.SetFloat(Hashes.SpeedFloat, rb.velocity.magnitude / speeds[speeds.Length - 1]);
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
}

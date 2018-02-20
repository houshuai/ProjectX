using UnityEngine;

public class WhaleMove : CharacterMove
{
    public float diveSpeed = 10;

    private void Start()
    {
        Initial();
    }

    private void Update()
    {
        ChangeCurrSpeed();
        Move();

#if UNITY_EDITOR
        if (Input.GetButtonDown("Jump"))
#else
        if (TouchButton.GetButtonDown("Jump"))
#endif
        {
            Dive();
        }
    }

    private void Dive()
    {
        rb.velocity = new Vector3(rb.velocity.x * diveSpeed, diveSpeed, rb.velocity.z * diveSpeed);
        anim.SetTrigger(Hashes.DiveTrigger);
    }
}

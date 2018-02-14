using UnityEngine;
using UnityEngine.EventSystems;

public class CameraLook : MonoBehaviour
{
    public Transform rig;
    public float distance = 5.0f;
    public float xSpeed = 3.0f;
    public float ySpeed = 2.0f;
    public float zSpeed = 2.0f;

    private float x;
    private float y;
    private float z;

    private float yMin = -30.0f;
    private float yMax = 80.0f;
    private float zMin = 2f;
    private float zMax = 5f;
    private PlayerMove player;


    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
        z = -distance;

        player = FindObjectOfType<PlayerMove>();

        if (EventSystem.current == null)
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }
    }

    void LateUpdate()
    {
        if (rig == null)
        {
            return;
        }

        if (player.isLock)
        {
            var direction = player.currEnemy.position - rig.position;
            direction.y = -0.2f;
            direction = direction.normalized;
            transform.position = rig.position + direction * z;
            transform.LookAt(player.currEnemy);
        }
        else
        {
            x += TouchPad.GetAxis("X") * xSpeed; //Input.GetAxis("Mouse X") * xSpeed;
            y -= TouchPad.GetAxis("Y") * ySpeed; //Input.GetAxis("Mouse Y") * ySpeed;
            z += Input.GetAxis("Mouse ScrollWheel") * zSpeed;
            y = Mathf.Clamp(y, yMin, yMax);
            z = Mathf.Clamp(z, -zMax, -zMin);
            RotatePosition();
            UpdatePosition();
        }

    }

    void RotatePosition()
    {
        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 position = rotation * new Vector3(0.0f, 0.0f, z) + rig.position;

        transform.position = position;
        transform.rotation = rotation;
    }

    void UpdatePosition()
    {
        RaycastHit hit;
        if (Physics.Raycast(rig.position, transform.position - rig.position, out hit, zMax))
        {
            if (!hit.collider.gameObject.CompareTag(Tags.MainCamera))
            {
                transform.position = hit.point;
            }
        }
    }
}

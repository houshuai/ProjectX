using UnityEngine;

public class ChaseAgent : MonoBehaviour
{
    public float speed;
    public float turnSpeed;
    [HideInInspector]
    public float actualSpeed;
    public float Remaining { get { return Vector3.Distance(transform.position, target); } }

    private Rigidbody rb;
    private Node currNode;
    private Vector3 target;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        currNode = ChaseMesh.GetNode(transform.position);
        if (currNode == null)
        {
            Debug.LogWarning("chase agent not get node");
        }
        target = transform.position;
    }

    public void Chase(Vector3 target)
    {
        this.target = target;
        if (currNode.Contains(target))
        {
            MoveAndRotate(target);
        }
        else
        {
            Node next = currNode;
            float distance = float.MaxValue;
            foreach (var neighbor in currNode.neighbors)
            {
                var temp = Vector3.Distance(neighbor.position, target);
                if (distance > temp)
                {
                    distance = temp;
                    next = neighbor;
                }
            }

            MoveAndRotate(next.position);

            if (next.Contains(transform.position))
            {
                currNode = next;
            }
        }
    }

    private void MoveAndRotate(Vector3 moveTo)
    {
        var vector = new Vector2(moveTo.x - currNode.position.x, moveTo.z - currNode.position.z);
        var angle = Vector2.Angle(new Vector2(transform.forward.x, transform.forward.z), vector);

        var rotation = Quaternion.LookRotation(new Vector3(vector.x, 0, vector.y));
        rb.MoveRotation(Quaternion.Lerp(transform.rotation, rotation, turnSpeed * Time.deltaTime));

        var desiredSpeed = speed * Mathf.Cos(angle * Mathf.Deg2Rad);
        desiredSpeed = Mathf.Max(0, desiredSpeed);
        actualSpeed = Mathf.Lerp(actualSpeed, desiredSpeed, 0.1f);
        rb.velocity = transform.forward * actualSpeed;
    }
}

using UnityEngine;

public class ChaseAgent : MonoBehaviour
{
    public float speed;
    public float turnSpeed;
    [HideInInspector]
    public float actualSpeed;
    public float Remaining
    {
        get
        {
            return Vector3.Distance(transform.position, target);
        }
    }
    
    private Node currNode;
    private Vector3 target;

    private void Start()
    {
        currNode = ChaseMesh.GetNode(transform.position);
        if (currNode == null)
        {
            Debug.LogWarning("chase agent not get node");
        }
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
                var temp = Vector3.Distance(neighbor.center, target);
                if (distance > temp)
                {
                    distance = temp;
                    next = neighbor;
                }
            }
            MoveAndRotate(next.center);

            if (next.Contains(transform.position))
            {
                currNode = next;
            }
        }
    }

    private void MoveAndRotate(Vector3 moveTo)
    {
        var vector = moveTo - transform.position;
        var vectorGround = new Vector3(vector.x, 0, vector.z).normalized;

        var rotation = Quaternion.LookRotation(vectorGround);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, turnSpeed * Time.deltaTime);

        var moveDir = Quaternion.FromToRotation(vectorGround, vector) * transform.forward;
        var desiredSpeed = speed * Vector3.Dot(vectorGround, transform.forward);
        desiredSpeed *= Vector3.Dot(transform.forward, moveDir);
        desiredSpeed = Mathf.Max(0, desiredSpeed);
        actualSpeed = Mathf.Lerp(actualSpeed, desiredSpeed, 0.1f);
        transform.position += moveDir * actualSpeed * Time.deltaTime;
    }
}

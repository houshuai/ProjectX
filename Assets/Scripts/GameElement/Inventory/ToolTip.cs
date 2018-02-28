using UnityEngine;

public class ToolTip : MonoBehaviour
{
    private void LateUpdate()
    {
        transform.position = Input.mousePosition;
    }
}

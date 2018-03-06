using UnityEngine;

public class ChangeScene : MonoBehaviour
{
    public string nextSceneName;
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Tags.Player) && Scene.Instance != null)
        {
            Scene.Instance.SwitchScene(nextSceneName);
        }
    }
}

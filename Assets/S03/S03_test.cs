using UnityEngine;

[ExecuteAlways]
public class S03_test : MonoBehaviour
{
    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private Vector3 targetRotationEuler;
    [SerializeField] private Vector3 targetScale = Vector3.one;

    void Update()
    {
        transform.position = targetPosition;
        transform.rotation = Quaternion.Euler(targetRotationEuler);
        transform.localScale = targetScale;
    }
}

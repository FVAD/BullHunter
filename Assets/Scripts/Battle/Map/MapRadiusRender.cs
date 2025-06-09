using UnityEngine;
using UnityEditor;

public class MapRadiusRender : MonoBehaviour
{
    // Start is called before the first frame update
    public float LineLength => Vector3.Distance(startPoint, endPoint);
    public Vector3 startPoint = Vector3.zero;
    public Vector3 endPoint = Vector3.forward;
    public Color lineColor = Color.red;

    void OnDrawGizmos()
    {
        Gizmos.color = lineColor;
        Gizmos.DrawLine(startPoint, endPoint);
    }
}

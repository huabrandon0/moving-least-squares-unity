using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PointMover : SerializedMonoBehaviour
{
    [SerializeField]
    Camera Camera { get; set; }

    [SerializeField]
    LayerMask MovementPlaneLayerMask { get; set; }

    [SerializeField]
    LayerMask PointLayerMask { get; set; }

    [ShowInInspector]
    Point SelectedPoint { get; set; }

    Vector3 SelectedPointOffset { get; set; }

    void Awake()
    {
        if (!Camera)
            Camera = Camera.main;
    }

    void Update()
    {
        bool inputKeyDown = Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.F);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (inputKeyDown && SelectedPoint != null)
        {
            SelectedPoint = null;
        }
        else if (inputKeyDown && SelectedPoint == null && Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, PointLayerMask, QueryTriggerInteraction.Collide))
        {
            Point point = hit.collider.GetComponent<Point>();
            if (point)
            {
                SelectedPoint = point;
                if (Physics.Raycast(ray, out hit, float.MaxValue, MovementPlaneLayerMask))
                    SelectedPointOffset = SelectedPoint.transform.position - hit.point;
                else
                    SelectedPointOffset = Vector3.zero;
            }
        }
        else if (SelectedPoint != null && Physics.Raycast(ray, out hit, float.MaxValue, MovementPlaneLayerMask))
        {
            PointManager.Instance.DeregisterPoint(SelectedPoint);
            SelectedPoint.transform.position = hit.point + SelectedPointOffset;
            PointManager.Instance.RegisterPoint(SelectedPoint);
        }
    }
}

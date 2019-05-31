using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;

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

    [SerializeField]
    TextMeshProUGUI PointCoordinateText { get; set; }

    [SerializeField]
    Vector3 PointCoordinateTextOffset { get; set; } = new Vector3(0f, 20f, 0f);
    
    public IPointMoverState CurrentState { get; private set; }
    IPointMoverState IdleState { get; set; }
    IPointMoverState HoldingState { get; set; }
    IPointMoverState HoveringState { get; set; }

    void Awake()
    {
        if (!Camera)
            Camera = Camera.main;

        IdleState = new Idle(this);
        HoldingState = new Holding(this);
        HoveringState = new Hovering(this);
        CurrentState = IdleState;

        HidePointCoordinates();
    }

    void Update()
    {
        CurrentState.Update();
    }

    void DisplayPointCoordinates(Point point)
    {
        if (!PointCoordinateText.enabled)
            PointCoordinateText.enabled = true;

        var screenPoint = Camera.WorldToScreenPoint(point.transform.position);
        PointCoordinateText.rectTransform.position = screenPoint + (Screen.height / 600f) * PointCoordinateTextOffset;
        PointCoordinateText.SetText(string.Format("({0:0.00}, {1:0.00})", point.Coordinates[0], point.Coordinates[1]));
    }

    void HidePointCoordinates()
    {
        PointCoordinateText.enabled = false;
    }

    public interface IPointMoverState
    {
        void Update();
    }

    class Idle : IPointMoverState
    {
        PointMover PointMover { get; set; }
        
        public Idle(PointMover point)
        {
            PointMover = point;
        }

        public void Update()
        {
            bool inputKeyDown = Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.F);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            bool raycastHitSomePoint = Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, PointMover.PointLayerMask);
            Point point = null;
            if (raycastHitSomePoint)
                point = hit.collider.GetComponent<Point>();
            
            PointMover.HidePointCoordinates();

            if (raycastHitSomePoint && point != null)
            {
                if (inputKeyDown)
                {
                    // Hold the point
                    PointMover.SelectedPoint = point;
                    PointMover.SelectedPoint.OnPointerDown();
                    PointMover.CurrentState = PointMover.HoldingState;
                }
                else
                {
                    // Hover the point
                    PointMover.SelectedPoint = point;
                    PointMover.SelectedPoint.OnPointerEnter();
                    PointMover.CurrentState = PointMover.HoveringState;
                }
            }
        }
    }

    class Hovering : IPointMoverState
    {
        PointMover PointMover { get; set; }
        
        public Hovering(PointMover pointMover)
        {
            PointMover = pointMover;
        }

        public void Update()
        {
            bool inputKeyDown = Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.F);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            bool raycastHitSomePoint = Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, PointMover.PointLayerMask);
            Point point = null;
            if (raycastHitSomePoint)
                point = hit.collider.GetComponent<Point>();
            
            PointMover.DisplayPointCoordinates(PointMover.SelectedPoint);

            if (raycastHitSomePoint && point != null)
            {
                if (inputKeyDown)
                {
                    // Hold the point
                    if (point == PointMover.SelectedPoint)
                    {
                        PointMover.SelectedPoint.OnPointerDown();

                        if (Physics.Raycast(ray, out hit, float.MaxValue, PointMover.MovementPlaneLayerMask))
                            PointMover.SelectedPointOffset = PointMover.SelectedPoint.transform.position - hit.point;
                        else
                            PointMover.SelectedPointOffset = Vector3.zero;

                        PointMover.CurrentState = PointMover.HoldingState;
                    }
                    else
                    {
                        if (PointMover.SelectedPoint)
                            PointMover.SelectedPoint.OnPointerExit();

                        PointMover.SelectedPoint = point;
                        PointMover.SelectedPoint.OnPointerDown();
                        
                        if (Physics.Raycast(ray, out hit, float.MaxValue, PointMover.MovementPlaneLayerMask))
                            PointMover.SelectedPointOffset = PointMover.SelectedPoint.transform.position - hit.point;
                        else
                            PointMover.SelectedPointOffset = Vector3.zero;

                        PointMover.CurrentState = PointMover.HoldingState;
                    }
                }
                else
                {
                    if (point != PointMover.SelectedPoint)
                    {
                        // Switch hover to new point
                        if (PointMover.SelectedPoint)
                            PointMover.SelectedPoint.OnPointerExit();

                        PointMover.SelectedPoint = point;
                        PointMover.SelectedPoint.OnPointerEnter();
                        PointMover.CurrentState = PointMover.HoveringState;
                    }
                }
            }
            else
            {
                PointMover.SelectedPoint.OnPointerExit();
                PointMover.SelectedPoint = null;
                PointMover.CurrentState = PointMover.IdleState;
            }
        }
    }

    class Holding : IPointMoverState
    {
        PointMover PointMover { get; set; }
        
        public Holding(PointMover pointMover)
        {
            PointMover = pointMover;
        }

        public void Update()
        {
            bool inputKeyDown = Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.F);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            bool raycastHitSomePoint = Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, PointMover.PointLayerMask);
            Point point = null;
            if (raycastHitSomePoint)
                point = hit.collider.GetComponent<Point>();
            
            PointMover.DisplayPointCoordinates(PointMover.SelectedPoint);

            if (!inputKeyDown)
            {
                if (Physics.Raycast(ray, out hit, float.MaxValue, PointMover.MovementPlaneLayerMask))
                {
                    PointManager.Instance.DeregisterPoint(PointMover.SelectedPoint);
                    PointMover.SelectedPoint.transform.position = hit.point + PointMover.SelectedPointOffset;
                    PointManager.Instance.RegisterPoint(PointMover.SelectedPoint);
                }
            }
            else
            {
                if (raycastHitSomePoint && point != null)
                {
                    if (point != PointMover.SelectedPoint)
                    {
                        // Switch hover to new point
                        if (PointMover.SelectedPoint)
                            PointMover.SelectedPoint.OnPointerExit();

                        PointMover.SelectedPoint = point;
                        PointMover.SelectedPoint.OnPointerEnter();
                        PointMover.CurrentState = PointMover.HoveringState;
                    }
                    else
                    {
                        // Same point, so hover
                        PointMover.SelectedPoint.OnPointerUp();
                        PointMover.CurrentState = PointMover.HoveringState;
                    }
                }
                else
                {
                    PointMover.SelectedPoint.OnPointerExit();
                    PointMover.SelectedPoint = null;
                    PointMover.CurrentState = PointMover.IdleState;
                }
            }
        }
    }
}

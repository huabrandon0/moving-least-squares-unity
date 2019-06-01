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
        Ray mouseScreenPointRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool isMouseOverPoint = Physics.Raycast(mouseScreenPointRay, out RaycastHit pointLayerHit, float.MaxValue, PointLayerMask);
        bool isMouseOverMovementPlane = Physics.Raycast(mouseScreenPointRay, out RaycastHit movementPlaneLayerHit, float.MaxValue, MovementPlaneLayerMask);

        UpdateInfo frameInfo = new UpdateInfo(
            Input.GetKeyDown(KeyCode.Mouse0),
            Input.GetKeyUp(KeyCode.Mouse0),
            isMouseOverPoint,
            isMouseOverPoint ? pointLayerHit.collider.GetComponent<Point>() : null,
            isMouseOverMovementPlane,
            isMouseOverMovementPlane ? movementPlaneLayerHit.point : Vector3.zero
        );

        CurrentState.Update(frameInfo);
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
        void Update(UpdateInfo updateInfo);
    }

    public class UpdateInfo
    {
        public bool HoldKeyDown { get; private set; }
        public bool HoldKeyUp { get; private set; }
        public bool IsMouseOverPoint { get; private set; }
        public Point Point { get; private set; }
        public bool IsMouseOverMovementPlane { get; private set; }
        public Vector3 MovementPlanePoint { get; private set; }

        public UpdateInfo(bool holdKeyDown, bool holdKeyUp, bool isMouseOverPoint, Point point, bool isMouseOverMovementPlane, Vector3 movementPlanePoint)
        {
            HoldKeyDown = holdKeyDown;
            HoldKeyUp = holdKeyUp;
            IsMouseOverPoint = isMouseOverPoint;
            Point = point;
            IsMouseOverMovementPlane = isMouseOverMovementPlane;
            MovementPlanePoint = movementPlanePoint;
        }
    }

    class Idle : IPointMoverState
    {
        PointMover PointMover { get; set; }
        
        public Idle(PointMover point)
        {
            PointMover = point;
        }

        public void Update(UpdateInfo updateInfo)
        {
            PointMover.HidePointCoordinates();

            if (updateInfo.IsMouseOverPoint && updateInfo.Point != null)
            {
                if (updateInfo.HoldKeyDown)
                {
                    // Hold the point
                    PointMover.SelectedPoint = updateInfo.Point;
                    PointMover.SelectedPoint.OnPointerDown();
                    PointMover.CurrentState = PointMover.HoldingState;
                }
                else
                {
                    // Hover the point
                    PointMover.SelectedPoint = updateInfo.Point;
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

        public void Update(UpdateInfo updateInfo)
        {
            PointMover.DisplayPointCoordinates(PointMover.SelectedPoint);

            if (updateInfo.IsMouseOverPoint && updateInfo.Point != null)
            {
                if (updateInfo.HoldKeyDown)
                {
                    // Hold the point
                    if (updateInfo.Point == PointMover.SelectedPoint)
                    {
                        PointMover.SelectedPoint.OnPointerDown();

                        if (updateInfo.IsMouseOverMovementPlane)
                            PointMover.SelectedPointOffset = PointMover.SelectedPoint.transform.position - updateInfo.MovementPlanePoint;
                        else
                            PointMover.SelectedPointOffset = Vector3.zero;

                        PointMover.CurrentState = PointMover.HoldingState;
                    }
                    else
                    {
                        if (PointMover.SelectedPoint)
                            PointMover.SelectedPoint.OnPointerExit();

                        PointMover.SelectedPoint = updateInfo.Point;
                        PointMover.SelectedPoint.OnPointerDown();
                        
                        if (updateInfo.IsMouseOverMovementPlane)
                            PointMover.SelectedPointOffset = PointMover.SelectedPoint.transform.position - updateInfo.MovementPlanePoint;
                        else
                            PointMover.SelectedPointOffset = Vector3.zero;

                        PointMover.CurrentState = PointMover.HoldingState;
                    }
                }
                else
                {
                    if (updateInfo.Point != PointMover.SelectedPoint)
                    {
                        // Switch hover to new point
                        if (PointMover.SelectedPoint)
                            PointMover.SelectedPoint.OnPointerExit();

                        PointMover.SelectedPoint = updateInfo.Point;
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

        public void Update(UpdateInfo updateInfo)
        {
            PointMover.DisplayPointCoordinates(PointMover.SelectedPoint);

            if (!updateInfo.HoldKeyUp)
            {
                if (updateInfo.IsMouseOverMovementPlane)
                {
                    PointManager.Instance.DeregisterPoint(PointMover.SelectedPoint);
                    PointMover.SelectedPoint.transform.position = updateInfo.MovementPlanePoint + PointMover.SelectedPointOffset;
                    PointManager.Instance.RegisterPoint(PointMover.SelectedPoint);
                }
            }
            else
            {
                if (updateInfo.IsMouseOverPoint && updateInfo.Point != null)
                {
                    if (updateInfo.Point != PointMover.SelectedPoint)
                    {
                        // Switch hover to new point
                        if (PointMover.SelectedPoint)
                            PointMover.SelectedPoint.OnPointerExit();

                        PointMover.SelectedPoint = updateInfo.Point;
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

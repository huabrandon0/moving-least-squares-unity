using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Sirenix.OdinInspector;
using System;
using UnityEngine.EventSystems;

public class Point : SerializedMonoBehaviour, IComparer<Point>, IComparable<Point>
{
    [SerializeField]
    Animator Animator { get; set; }

    public IPointState CurrentState { get; private set; }
    IPointState IdleState { get; set; }
    IPointState PressedState { get; set; }
    IPointState HoveredState { get; set; }

    void Awake()
    {
        IdleState = new Idle(this);
        PressedState = new Pressed(this);
        HoveredState = new Hovered(this);
        CurrentState = IdleState;
    }

    public void OnPointerEnter()
    {
        CurrentState.OnPointerEnter();
    }

    public void OnPointerExit()
    {
        CurrentState.OnPointerExit();
    }

    public void OnPointerDown()
    {
        CurrentState.OnPointerDown();
    }

    public void OnPointerUp()
    {
        CurrentState.OnPointerUp();
    }
    
    void OnEnable()
    {
        PointManager.Instance.RegisterPoint(this);
    }

    void OnDisable()
    {
        PointManager.Instance.DeregisterPoint(this);
    }

    public Vector<double> Coordinates
    {
        get { return DenseVector.OfArray(new double[] { transform.position.x, transform.position.y }); }
    }

    public int Compare(Point point1, Point point2)
    {
        return Math.Sign(point1.Coordinates[0] - point2.Coordinates[0]);
    }

    public int CompareTo(Point point)
    {
        return Math.Sign(Coordinates[0] - point.Coordinates[0]);
    }

    enum AnimatorState
    {
        Idle = 0,
        Hovered = 1,
        Pressed = 2
    }

    public interface IPointState
    {
        void OnPointerEnter();
        void OnPointerExit();
        void OnPointerDown();
        void OnPointerUp();
    }

    class Idle : IPointState
    {
        Point Point { get; set; }

        public Idle(Point point)
        {
            Point = point;
        }

        public void OnPointerDown()
        {
            Point.Animator.SetInteger("State", (int)AnimatorState.Pressed);
            Point.CurrentState = Point.PressedState;
        }

        public void OnPointerEnter()
        {
            Point.Animator.SetInteger("State", (int)AnimatorState.Hovered);
            Point.CurrentState = Point.HoveredState;
        }

        public void OnPointerExit()
        {

        }

        public void OnPointerUp()
        {

        }
    }

    class Hovered : IPointState
    {
        Point Point { get; set; }

        public Hovered(Point point)
        {
            Point = point;
        }

        public void OnPointerDown()
        {
            Point.Animator.SetInteger("State", (int)AnimatorState.Pressed);
            Point.CurrentState = Point.PressedState;
        }

        public void OnPointerEnter()
        {

        }

        public void OnPointerExit()
        {
            Point.Animator.SetInteger("State", (int)AnimatorState.Idle);
            Point.CurrentState = Point.IdleState;
        }

        public void OnPointerUp()
        {

        }
    }

    class Pressed : IPointState
    {
        Point Point { get; set; }

        public Pressed(Point point)
        {
            Point = point;
        }

        public void OnPointerDown()
        {

        }

        public void OnPointerEnter()
        {

        }

        public void OnPointerExit()
        {
            Point.Animator.SetInteger("State", (int)AnimatorState.Idle);
            Point.CurrentState = Point.IdleState;
        }

        public void OnPointerUp()
        {
            Point.Animator.SetInteger("State", (int)AnimatorState.Hovered);
            Point.CurrentState = Point.HoveredState;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Sirenix.OdinInspector;
using System;

public class Point : SerializedMonoBehaviour, IComparer<Point>, IComparable<Point>
{
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
}

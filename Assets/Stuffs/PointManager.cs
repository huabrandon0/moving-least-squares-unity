using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PointManager
{
    public static void Init()
    {
        if (_instance == null)
            _instance = new PointManager();
    }

    static PointManager _instance;
    public static PointManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new PointManager();
            return _instance;
        }
    }

    public SortedSet<Point> Points { get; private set; } = new SortedSet<Point>();

    public void RegisterPoint(Point point)
    {
        Points.Add(point);
    }

    public void DeregisterPoint(Point point)
    {
        Points.Remove(point);
    }
}

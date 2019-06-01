using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Sirenix.OdinInspector;
using System;
using System.Linq;

public class TestBehaviour : SerializedMonoBehaviour
{
    [ShowInInspector]
    MLS MLS;

    [SerializeField]
    LineRenderer LineRenderer { get; set; }

    [SerializeField]
    List<double> EvaluationPoints { get; set; }

    bool Pause { get; set; } = false;

    float TimeOffset { get; set; } = 0f;

    [SerializeField]
    public double ScalingFactorMin { get; set; } = 1f;

    [SerializeField]
    public double ScalingFactorMax { get; set; } = 30f;

    void Awake()
    {
        Matrix BasisMatrix(double x)
        {
            return DenseMatrix.OfArray(new double[,] {
                { 1 },
                { x },
                //{ Math.Pow(x, 2) },
                //{ Math.Pow(x, 3) },
                //{ Math.Pow(x, 4) },
                //{ Math.Pow(x, 5) },
            });
        }

        double WeightFunction(double z)
        {
            double phi;

            double zAbs = Math.Abs(z); // Take the absolute value of z

            if (zAbs <= 0.5f)
                phi = 1f - 6f * Math.Pow(zAbs, 2f) + 6f * Math.Pow(zAbs, 3f);
            else if (zAbs <= 1f)
                phi = 2f - 6 * zAbs + 6 * Math.Pow(zAbs, 2f) - 2 * Math.Pow(zAbs, 3f);
            else
                phi = 0f;

            return phi;
        }

        //List<Vector<double>> dataPoints = new List<Vector<double>>()
        //{
        //    DenseVector.OfArray(new double[] { 1d, 1.4d }),
        //    DenseVector.OfArray(new double[] { 2d, 2.3d }),
        //    DenseVector.OfArray(new double[] { 3d, 1.7d }),
        //    DenseVector.OfArray(new double[] { 4d, 1.9d }),
        //    DenseVector.OfArray(new double[] { 5d, 2.7d }),
        //    DenseVector.OfArray(new double[] { 6d, 1.6d }),
        //    DenseVector.OfArray(new double[] { 7d, 3.3d }),
        //    DenseVector.OfArray(new double[] { 8d, 2.2d }),
        //    DenseVector.OfArray(new double[] { 9d, 2.5d }),
        //    DenseVector.OfArray(new double[] { 10d, 1.9d }),
        //};

        List<Vector<double>> dataPoints = PointManager.Instance.Points.Select((p) => p.Coordinates).ToList();

        MLS = new MLS(BasisMatrix, dataPoints, WeightFunction);

        StartCoroutine(OscillateScalingFactor());
    }

    void LateUpdate()
    {
        List<Vector<double>> dataPoints = PointManager.Instance.Points.Select((p) => p.Coordinates).ToList();
        MLS.DataPoints = dataPoints;
        double range = dataPoints[dataPoints.Count - 1][0] - dataPoints[0][0];
        double step = 0.05d;
        EvaluationPoints = Enumerable.Range(0, (int)Math.Ceiling(range / step) + 1).Select(i => dataPoints[0][0] + i * step).ToList();
        SetLineRendererPoints();
    }

    IEnumerator OscillateScalingFactor()
    {
        float startTime = Time.time;

        while (true)
        {
            float currentTime = Time.time + TimeOffset;

            float time = currentTime - startTime;

            float interpVal = (float)(0.5d + -Math.Cos(time) * 0.5d);

            SetScalingFactor(Mathf.Lerp((float)ScalingFactorMin, (float)ScalingFactorMax, interpVal));

            yield return null;
        }
    }

    [Button]
    public void StartOscillating()
    {
        if (Pause)
            PauseOscillation();

        StopAllCoroutines();
        StartCoroutine(OscillateScalingFactor());
    }

    [Button]
    public void PauseOscillation()
    {
        Pause = !Pause;
        if (Pause)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;
    }

    [Button]
    public void StepBack()
    {
        TimeOffset += 0.05f;
    }

    [Button]
    public void StepForward()
    {
        TimeOffset -= 0.05f;
    }

    [Button]
    public void StopOscillating()
    {
        StopAllCoroutines();
    }
    
    [Button]
    public void SetScalingFactor(double s)
    {
        MLS.ScalingFactor = s;
    }

    [Button]
    public void EvaluateMLS(double x)
    {
        Debug.Log(MLS.Evaluate(x));
    }

    public void SetLineRendererPoints()
    {
        List<Vector3> positions = new List<Vector3>();

        for (int i = 0; i < EvaluationPoints.Count; i++)
            positions.Add(new Vector3((float)EvaluationPoints[i], (float)MLS.Evaluate(EvaluationPoints[i]), 0f));
        
        LineRenderer.positionCount = positions.Count;
        LineRenderer.SetPositions(positions.ToArray());
    }
}

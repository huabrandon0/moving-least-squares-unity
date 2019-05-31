using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Sirenix.OdinInspector;
using System;

public class MLS
{
    public delegate Matrix MatrixEvaluator(double x);
    MatrixEvaluator BasisMatrix { get; set; }
    
    public delegate double REvaluator(double x);
    REvaluator WeightFunction { get; set; }

    [ShowInInspector]
    public List<Vector<double>> DataPoints { get; set; }
    
    const double SCALING_FACTOR_MIN = 1d + double.Epsilon;
    const double SCALING_FACTOR_MAX = double.MaxValue;

    [ShowInInspector]
    double _scalingFactor;
    public double ScalingFactor
    {
        get { return _scalingFactor; }
        //set { _scalingFactor = Mathf.Clamp((float)value, (float)SCALING_FACTOR_MIN, (float)SCALING_FACTOR_MAX); }
        set { _scalingFactor = Math.Min(Math.Max(value, SCALING_FACTOR_MIN), SCALING_FACTOR_MAX); }
    }

    public MLS(MatrixEvaluator basisMatrix, List<Vector<double>> dataPoints, REvaluator weightFunction, double scalingFactor = SCALING_FACTOR_MIN)
    {
        BasisMatrix = basisMatrix;
        DataPoints = dataPoints.ToList();
        WeightFunction = weightFunction;
        ScalingFactor = scalingFactor;
    }

    public double Evaluate(double x)
    {
        List<double> weights = Enumerable.Repeat(0d, DataPoints.Count).ToList();

        int basisLength = BasisMatrix(1).RowCount;
        Matrix<double> m = Matrix<double>.Build.Dense(basisLength, basisLength);
        
        for (int i = 0; i < DataPoints.Count; i++)
        {
            double phi = WeightFunction((x - DataPoints[i][0]) / ScalingFactor); // phi(x - x_i / s)
            weights[i] = phi;
            
            Matrix<double> p_xi = BasisMatrix(DataPoints[i][0]);
            m += phi * p_xi * p_xi.Transpose(); // M += phi(x - x_i / s) * p(x_i) * p(x_i)^t
        }
        
        Matrix<double> mInverse = m.Inverse(); // M^-1
        Matrix<double> p_x_t = BasisMatrix(x).Transpose(); // p(x)^t

        double p_n_x = 0f; // p_n(x)
        
        for (int i = 0; i < DataPoints.Count; i++)
        {
            Matrix<double> p_xi = BasisMatrix(DataPoints[i][0]);
            double m_i = (weights[i] * p_x_t * mInverse * p_xi)[0,0]; // M_i(x) = phi(x - x_i / s) * p(x)^t * M^-1 * p(x_i)
            p_n_x += DataPoints[i][1] * m_i; // p_n(x) += y_i * M_i(x)
        }
        
        return p_n_x;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoissonTester : MonoBehaviour
{
    public float radius = 1;
    public Vector2 regionSize = Vector2.zero;
    public int rejectionSamples = 30;
    public float displayRadius = 1;

    public int minPointsCount = 25;

    List<Vector2> points;

    private void OnValidate()
    {
        points = new List<Vector2>();
        int iterations = 0;
        while (points.Count < minPointsCount)
        {
            points = PoissonDiscSampler.GeneratePoints(radius, regionSize, rejectionSamples);
            iterations++;
            if (iterations > 100) break;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(regionSize / 2, regionSize);
        if (points != null)
        {
            for (int i = 0; i < points.Count; i++)
            {
                if (i<minPointsCount)
                {
                    Gizmos.color = Color.green;
                }
                else
                {
                    Gizmos.color = Color.grey;
                }
                Gizmos.DrawWireSphere(points[i], displayRadius);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public class Sensing : MonoBehaviour 
{
    [Tooltip("How many degrees can the angles in a triangle deviate from 180 / 0 and yet still be considered a line?")]
    public float lineTolerance;

    public float triangleAngleMinimum;
    public float triangleAngleMaximum;
    
    public enum Type
    {
        None,
        Line,
        Triangle,
    }

    public class TouchFrame
    {
        public List<Vector2> touches;
    }

    private Vector2 Average(IEnumerable<Vector2> vectors)
    {
        Vector2 sum = Vector2.zero;

        foreach (var vector in vectors)
            sum += vector;

        return sum / vectors.Count();
    }

    public void Update()
    {
        // default to position tracking as centroid
        this.position = Average(Input.touches.Select(touch => touch.position));

        // collect the current touch data together
        var frame = new TouchFrame
        {
            touches = Input.touches.Select(touch => touch.position).ToList(),
        };

        // for now we use frames immediately if they have 3 points, otherwise
        // ignore them entirely
        if (frame.touches.Count == 3)
        {
            ProcessFrameImmediately(frame);
        }

        // in the future there should be a distinct initial phase in which we
        // trying to determine which shape we are seeing. once determined, we
        // cease trying to identify shape and instead try to find the most
        // likely orientation of the known shape based on points we observe and
        // the previously known state
    }
    
    public Vector2 position;
    public float angle;
    public Type type;
    public float variable;
    public Vector3 feature;

    public Vector3 ExtractSidesFeature(TouchFrame frame)
    {
        Vector2 a = frame.touches[0];
        Vector2 b = frame.touches[1];
        Vector2 c = frame.touches[2];

        float ab = (b - a).sqrMagnitude * .001f;
        float bc = (c - b).sqrMagnitude * .001f;
        float ca = (a - c).sqrMagnitude * .001f;

        // TODO: need to sort by winding!!

        // rotate the sides so the shortest is first (for debugging mostly,
        // the recognition will also rotate the side lengths anyway)
        Vector3 sides;

        if (ab < bc && ab < ca)
        {
            sides.x = ab;
            sides.y = bc;
            sides.z = ca;
        }
        else if (bc < ab && bc < ca)
        {
            sides.x = bc;
            sides.y = ca;
            sides.z = ab;
        }
        else
        {
            sides.x = ca;
            sides.y = ab;
            sides.z = bc;
        }

        return sides;
    }

    public void ProcessFrameImmediately(TouchFrame frame)
    {
        feature = ExtractSidesFeature(frame);
    }

    /// <summary>
    /// Return the angle (in radians) of the corner formed by the lines ab and
    /// ac
    /// </summary>
    private float AngleOfCorner(Vector2 a, Vector2 b, Vector2 c)
    {
        float ab = Vector2.SqrMagnitude(b - a);
        float ac = Vector2.SqrMagnitude(c - a);
        float bc = Vector2.SqrMagnitude(c - b);

        float angle = Mathf.Acos((ab + ac - bc) 
                    / (2 * Mathf.Sqrt(ab) * Mathf.Sqrt(ac)));

        return angle;
    }
}

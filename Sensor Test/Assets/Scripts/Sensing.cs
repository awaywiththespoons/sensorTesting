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
    public class TouchFrame
    {
        public List<Vector2> touches;
    }

    public static float Compare(Vector3 a, Vector3 b)
    {
        return Vector3.Dot(b - a, b - a);
    }

    public static Vector3 CycleVectorToMatchOther(Vector3 vector, Vector3 other)
    {
        Vector3 a = vector;
        Vector3 b = CycleVectorComponents(a);
        Vector3 c = CycleVectorComponents(b);

        float ad = Compare(a, other);
        float bd = Compare(b, other);
        float cd = Compare(c, other);

        if (ad <= bd && ad <= cd)
        {
            return a;
        }
        else if (bd <= ad && bd <= cd)
        {
            return b;
        }
        else
        {
            return c;
        }
    }

    public static Vector3 SortVectorComponents(Vector3 vector)
    {
        if (vector.y > vector.z)
        {
            float swap = vector.y;

            vector.y = vector.z;
            vector.z = swap;       
        }
    
        if (vector.x > vector.y)
        {
            float swap = vector.x;

            vector.x = vector.y;
            vector.y = swap;       
        }

        if (vector.y > vector.z)
        {
            float swap = vector.y;

            vector.y = vector.z;
            vector.z = swap;       
        }

        return vector;
    }

    public static Vector3 CycleVectorComponents(Vector3 vector)
    {
        return new Vector3(vector.y, vector.z, vector.x);
    }

    private static float Rank(Vector3 vector)
    {
        return vector.x;

        return vector.x * vector.x * vector.x
             + vector.y * vector.y
             + vector.z;
    }

    public static Vector3 MinimiseVectorRank(Vector3 vector)
    {
        Vector3 a = vector;
        Vector3 b = CycleVectorComponents(a);
        Vector3 c = CycleVectorComponents(b);

        float ad = Rank(a);
        float bd = Rank(b);
        float cd = Rank(c);

        if (ad <= bd && ad <= cd)
        {
            return a;
        }
        else if (bd <= ad && bd <= cd)
        {
            return b;
        }
        else
        {
            return c;
        }
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
            valid = true;
        }
        else
        {
            valid = false;
        }

        // in the future there should be a distinct initial phase in which we
        // trying to determine which shape we are seeing. once determined, we
        // cease trying to identify shape and instead try to find the most
        // likely orientation of the known shape based on points we observe and
        // the previously known state
    }
    
    public Vector2 position;
    public float angle;
    public Vector3 feature;
    public bool valid;

    private static float PolarAngle(Vector2 point)
    {
        return Mathf.Atan2(point.y, point.x);
    }

    public static Vector3 ExtractSidesFeature(TouchFrame frame)
    {
        Vector2 a = frame.touches[0];
        Vector2 b = frame.touches[1];
        Vector2 c = frame.touches[2];

        Vector2 centroid = (a + b + c) / 3f;

        var points = frame.touches.OrderBy(point => PolarAngle(point - centroid)).ToList();
        a = points[0];
        b = points[1];
        c = points[2];

        float ab = (b - a).magnitude / Screen.dpi * 2.54f;
        float bc = (c - b).magnitude / Screen.dpi * 2.54f;
        float ca = (a - c).magnitude / Screen.dpi * 2.54f;

        return new Vector3(ab, bc, ca);

        /*
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
        */
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

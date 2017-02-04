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
    public struct Side
    {
        public Vector2 pointA, pointB;
        public float length;
        public float direction; // triangle=clockwise, line=out

        public Side(Vector2 pointA, Vector2 pointB)
        {
            this.pointA = pointA;
            this.pointB = pointB;

            length = (pointB - pointA).magnitude;
            direction = 0;
        }
    }

    public class TouchFrame
    {
        public List<Vector2> touches;

        public Vector2 centroid;
        public float lineness;

        public List<Side> sides;
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

    private Vector2 Average(IEnumerable<Vector2> vectors)
    {
        Vector2 sum = Vector2.zero;

        foreach (var vector in vectors)
            sum += vector;

        return sum / vectors.Count();
    }

    public TouchFrame frame;

    public void Update()
    {
        // default to position tracking as centroid
        this.position = Average(Input.touches.Select(touch => touch.position / Screen.dpi * 2.54f));

        // collect the current touch data together
        frame = new TouchFrame
        {
            touches = Input.touches.Select(touch => touch.position / Screen.dpi * 2.54f).ToList(),
            centroid = position,
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

    public static float PolarAngle(Vector2 point)
    {
        return Mathf.Atan2(point.y, point.x) * Mathf.Rad2Deg;
    }

    public static Vector3 ExtractSidesFeature(TouchFrame frame)
    {
        var points = frame.touches.OrderBy(point => PolarAngle(point - frame.centroid)).ToList();
        Vector2 a = points[0];
        Vector2 b = points[1];
        Vector2 c = points[2];

        float ab = (b - a).magnitude;
        float bc = (c - b).magnitude;
        float ca = (a - c).magnitude;

        return new Vector3(ab, bc, ca);
    }

    public void ProcessFrameImmediately(TouchFrame frame)
    {
        Vector2 a = frame.touches[0];
        Vector2 b = frame.touches[1];
        Vector2 c = frame.touches[2];

        frame.centroid = (a + b + c) / 3f;

        feature = ExtractSidesFeature(frame);

        var points = frame.touches.OrderBy(point => PolarAngle(point - frame.centroid)).ToList();
        a = points[0];
        b = points[1];
        c = points[2];

        var ab = new Side(a, b);
        var bc = new Side(b, c);
        var ca = new Side(c, a);

        frame.lineness = Triangle.Lineness(new Vector3(ab.length, bc.length, ca.length));
        frame.sides = new List<Side> { ab, bc, ca };

        float angle;

        if (frame.lineness >= 0.95f)
        {
            var mid = frame.sides.OrderBy(side => side.length).ElementAt(1);

            Vector2 da = frame.centroid - mid.pointA;
            Vector2 db = frame.centroid - mid.pointB;

            if (da.sqrMagnitude > db.sqrMagnitude)
            {
                angle = PolarAngle(da);
            }
            else
            {
                angle = PolarAngle(db);
            }
        }
        else
        {
            var best = frame.sides.OrderByDescending(side => side.length).ElementAt(0);

            float ra = PolarAngle(frame.centroid - best.pointA);
            float rb = PolarAngle(frame.centroid - best.pointB);

            Vector2 direction;

            if (Mathf.DeltaAngle(ra, rb) > 0)
            {
                direction = best.pointB - best.pointA;
            }
            else
            {
                direction = best.pointA - best.pointB;
            }

            angle = PolarAngle(direction);
        }

        if (Mathf.DeltaAngle(this.angle, angle) < 120)
        {
            this.angle = angle;
        }
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

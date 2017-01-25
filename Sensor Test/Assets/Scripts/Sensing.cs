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
    public Transform testObject;

    [Tooltip("How many degrees can the angles in a triangle deviate from 180 / 0 and yet still be considered a line?")]
    public float lineTolerance;

    public float triangleAngleMinimum;
    public float triangleAngleMaximum;

    public class TouchFrame
    {
        public List<Vector2> touches;
    }

    public void Update()
    {
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

    private float minD = 9999, maxD = 0;
    public Vector2 position;
    public float angle;

    public void ProcessFrameImmediately(TouchFrame frame)
    {
        // assume we only get three points
        Vector2 a = frame.touches[0];
        Vector2 b = frame.touches[1];
        Vector2 c = frame.touches[2];

        Vector2 ab = b - a;
        Vector2 bc = c - b;
        Vector2 ca = a - c;

        // angle on each point of the triangle formed
        float cab = AngleOfCorner(c, a, b) * Mathf.Rad2Deg;
        float bca = AngleOfCorner(b, c, a) * Mathf.Rad2Deg;
        float abc = AngleOfCorner(a, b, c) * Mathf.Rad2Deg;
        
        var points = new List<Vector2> { a, b, c };
        var angles = new List<float> { cab, bca, abc };

        bool line = angles.All(angle => Mathf.Abs(angle - 180) <= lineTolerance
                                     || Mathf.Abs(angle -   0) <= lineTolerance);
        
        bool triangle = angles.All(angle => angle <= triangleAngleMaximum
                                         && angle >= triangleAngleMinimum);

        if (line)
        {
            // determine orientation of line

            // 1. find point closest to the center of all points ("middle")
            Vector2 center = (a + b + c) / 3f;
            Vector2 middle = points.OrderBy(point => (center - point).sqrMagnitude).First();

            // 2. "front" is the closest point to "middle" and "back" is the
            // furthest
            var furthest = points.OrderByDescending(point => (middle - point).sqrMagnitude).ToList();
            Vector2 back = furthest[0];
            Vector2 front = furthest[1];

            // 3. the object points like an arrow to "front" from "back";
            Vector2 arrow = front - back;

            // 4. determine the angle of that arrow
            float angle = Mathf.Atan2(arrow.y, arrow.x) * Mathf.Rad2Deg;

            // 5. determine the length of the back line
            float length = (back - middle).magnitude;

            testObject.transform.eulerAngles = Vector3.forward * angle;

            minD = Mathf.Min(length, minD);
            maxD = Mathf.Max(length, maxD);

            Debug.LogFormat("Line ({0:0.0} degrees, {2:0}-{1:0}-{3:0} species)", angle, length, minD, maxD);

            this.position = center;
            this.angle = angle;
        }

        if (triangle)
        {
            // 1. sort points in order of sum of squared distance to other 
            // points, the "front" point is the first 
            var corners = points.OrderBy(point => points.Sum(other => (other - point).sqrMagnitude)).ToList();

            // 2. find angle on "front" corner and use this to determine the
            // shape type
            float species = AngleOfCorner(corners[1], corners[0], corners[2]) * Mathf.Rad2Deg;

            // 3. the object points like an arrow from the center of all points
            // to the "front" point
            Vector2 center = (points[0] + points[1] + points[2]) / 3;
            Vector2 arrow = corners[0] - center;

            // 4. determine the angle of that arrow
            float angle = Mathf.Atan2(arrow.y, arrow.x) * Mathf.Rad2Deg;

            Debug.LogFormat("Triangle ({0:0.0} degrees, {1:0} species)", angle, species);

            this.position = center;
            this.angle = angle;
        }

        if (!line && !triangle)
        {
            Debug.Log("???");
        }
    }

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

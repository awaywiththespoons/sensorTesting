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

    public float lineAngleMinimum;
    public float lineAngleMaximum;

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
    }

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

        Debug.LogFormat("{0:0.0}, {1:0.0}, {2:0.0}", cab, abc, bca);

        var points = new List<Vector2> { a, b, c };
        var angles = new List<float> { cab, bca, abc };

        bool line = angles.All(angle => angle >= lineAngleMaximum
                                     || angle <= lineAngleMinimum);

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
            var furthest = points.OrderBy(point => (middle - point).sqrMagnitude).ToList();
            Vector2 back = furthest[0];
            Vector2 front = furthest[1];

            // 3. the object points like an arrow to "front" from "back";
            Vector2 arrow = front - back;

            // 4. determine the angle of that arrow
            float angle = Mathf.Atan2(arrow.y, arrow.x) * Mathf.Rad2Deg;

            Debug.LogFormat("Line ({0} degrees)", angle);

            testObject.transform.eulerAngles = Vector3.forward * angle;
        }

        if (triangle)
        {
            Debug.Log("Triangle");
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

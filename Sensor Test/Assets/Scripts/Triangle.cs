using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public static class Triangle
{
    public static float Lineness(Vector3 vector)
    {
        float a = Mathf.Abs(1 - vector.x / (vector.y + vector.z));
        float b = Mathf.Abs(1 - vector.y / (vector.x + vector.z));
        float c = Mathf.Abs(1 - vector.z / (vector.x + vector.y));

        return 1 - Mathf.Min(a, b, c) * 2;
    }

    public static bool IsLine(Vector3 vector)
    {
        return Lineness(vector) >= 0.95f;
    }

    public static float Compare(Vector3 a, Vector3 b)
    {
        return Vector3.Dot(b - a, b - a);
    }

    public static float CompareBestCycle(Vector3 a, Vector3 b)
    {
        return Compare(CycleToMatch(a, b), b);
    }

    public static Vector3 Cycle(Vector3 vector)
    {
        return new Vector3(vector.y, vector.z, vector.x);
    }

    public static Vector3 CycleToMatch(Vector3 vector, Vector3 other)
    {
        Vector3 a = vector;
        Vector3 b = Cycle(a);
        Vector3 c = Cycle(b);

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

    public static Vector3 CycleToSide(Vector3 vector, float side)
    {
        Vector3 a = vector;
        Vector3 b = Cycle(a);
        Vector3 c = Cycle(b);

        float ad = Mathf.Abs(side - a.x);
        float bd = Mathf.Abs(side - b.x);
        float cd = Mathf.Abs(side - c.x);

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

    public static Vector3 CycleMinimiseX(Vector3 vector)
    {
        Vector3 a = vector;
        Vector3 b = Cycle(a);
        Vector3 c = Cycle(b);

        if (a.x <= b.x && a.x <= c.x)
        {
            return a;
        }
        else if (b.x <= a.x && b.x <= c.x)
        {
            return b;
        }
        else
        {
            return c;
        }
    }
}

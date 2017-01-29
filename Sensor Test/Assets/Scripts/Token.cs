using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public class Token : MonoBehaviour 
{
    public Sensing.Type patternType;
    public float patternVariableTarget;
    public List<Hotspot> hotspots = new List<Hotspot>();
    public Hotspot defaultHotspot;

    public void Refresh(Vector2 position, float angle)
    {
        gameObject.SetActive(true);

        var hotspot = FindHotspot(position, angle);

        DisableHotspots();
        hotspot.Refresh(position, angle);
    }

    private Hotspot FindHotspot(Vector2 position, float angle)
    {
        for (int i = 0; i < hotspots.Count; ++i)
        {
            var hotspot = hotspots[i];

            if (hotspot.IsValid(position, angle))
            {
                return hotspot;
            }
        }

        return defaultHotspot;
    }

    private void DisableHotspots()
    {
        for (int i = 0; i < hotspots.Count; ++i)
        {
            hotspots[i].Disable();
        }

		defaultHotspot.Disable();
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }
}

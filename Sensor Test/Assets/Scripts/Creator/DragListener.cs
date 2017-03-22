using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

using UnityEngine.EventSystems;

public class DragListener : MonoBehaviour, 
                            IPointerDownHandler,
                            IPointerUpHandler,
                            IBeginDragHandler, 
                            IDragHandler, 
                            IEndDragHandler
{
    [SerializeField]
    private CanvasGroup group;

    public event Action OnBegin = delegate { };
    public event Action<Vector2> OnDisplacementChanged = delegate { };
    public event Action OnEnd = delegate { };

    public bool dragging;

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        dragging = true;
        group.ignoreParentGroups = true;
        group.alpha = 0.5f;
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        dragging = false;
        group.ignoreParentGroups = false;
        group.alpha = 1f;
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        dragging = true;
        group.ignoreParentGroups = true;
        group.alpha = 0.5f;

        OnBegin();
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        if (dragging)
        {
            OnDisplacementChanged(eventData.position - eventData.pressPosition);
        }
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        OnEnd();

        dragging = false;
        group.ignoreParentGroups = false;
        group.alpha = 1f;
    }
}

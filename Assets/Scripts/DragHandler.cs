using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DragHandler : PointerManipulator
{
    public bool selected;
    private Vector2 startPosition;
    private Stack<(Vector2 endPosition, VisualElement element)> oldPositions;
    private Vector3 pointerStartPosition;
    private bool enabled;

    public DragHandler(VisualElement target, Stack<(Vector2, VisualElement)> oldPositions = null)
    {
        this.target = target;
        this.oldPositions = oldPositions ?? new Stack<(Vector2, VisualElement)>();
        selected = true;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
        target.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
        target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
        target.RegisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
        target.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
        target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
        target.UnregisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
    }

    private void PointerDownHandler(PointerDownEvent evt)
    {
        if (evt.button == 0 && selected == true)
        {
            startPosition = target.transform.position;
            target.CapturePointer(evt.pointerId);
            pointerStartPosition = evt.position;
            enabled = true;
        }
    }

    private void PointerMoveHandler(PointerMoveEvent evt)
    {
        if (enabled && target.HasPointerCapture(evt.pointerId))
        {
            Vector3 pointerDelta = evt.position - pointerStartPosition;
            var endPos = startPosition + (Vector2)pointerDelta;
            target.transform.position = endPos;

        }
    }

    private void PointerUpHandler(PointerUpEvent evt)
    {
        if (enabled && target.HasPointerCapture(evt.pointerId))
        {
            oldPositions.Push((startPosition, target));
            target.ReleasePointer(evt.pointerId);
        }
    }

    private void PointerCaptureOutHandler(PointerCaptureOutEvent evt)
    {
        enabled = false;
    }
}
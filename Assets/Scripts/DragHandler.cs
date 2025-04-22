using UnityEngine;
using UnityEngine.UIElements;

public class DragHandler : PointerManipulator
{
    public bool selected;
    private Vector2 startPosition;
    private Vector3 pointerStartPosition;
    private bool enabled;

    public DragHandler(VisualElement target)
    {
        this.target = target;
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
        if(evt.button == 0 && selected == true)
        {
            startPosition = target.transform.position;
            pointerStartPosition = evt.position;
            target.CapturePointer(evt.pointerId);
            enabled = true;
        }     
    }

    private void PointerMoveHandler(PointerMoveEvent evt)
    {
        if (enabled && target.HasPointerCapture(evt.pointerId))
        {
            Vector3 pointerDelta = evt.position - pointerStartPosition;
            target.transform.position = startPosition + (Vector2)pointerDelta;
        }
    }

    private void PointerUpHandler(PointerUpEvent evt)
    {
        if (enabled && target.HasPointerCapture(evt.pointerId))
        {
            target.ReleasePointer(evt.pointerId);
        }
    }

    private void PointerCaptureOutHandler(PointerCaptureOutEvent evt)
    {
        enabled = false;
    }
}
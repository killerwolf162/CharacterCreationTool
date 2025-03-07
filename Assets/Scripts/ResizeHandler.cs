using UnityEngine;
using UnityEngine.UIElements;

public class ResizeHandler : PointerManipulator
{
    private Vector2 startMousePosition;
    private Vector2 startPanelSize;
    private bool enabled;

    public ResizeHandler(VisualElement target)
    {
        this.target = target;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<MouseDownEvent>(OnStartResize);
        target.RegisterCallback<MouseMoveEvent>(OnResize);
        target.RegisterCallback<MouseUpEvent>(OnEndResize);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<MouseDownEvent>(OnStartResize);
        target.UnregisterCallback<MouseMoveEvent>(OnResize);
        target.UnregisterCallback<MouseUpEvent>(OnEndResize);
    }

    private void OnStartResize(MouseDownEvent evt)
    {
        if(evt.button == 1)
        {
            startMousePosition = evt.mousePosition;
            startPanelSize = new Vector2(target.resolvedStyle.width, target.resolvedStyle.height);

            target.CaptureMouse();
            enabled = true;

            evt.StopPropagation();
        }     
    }

    private void OnResize(MouseMoveEvent evt)
    {
        if (!enabled)
            return;

        Vector2 delta = evt.mousePosition - startMousePosition;

        target.style.width = Mathf.Max(50, startPanelSize.x + delta.x);
        target.style.height = Mathf.Max(50, startPanelSize.y + delta.y);

        evt.StopPropagation();
    }

    private void OnEndResize(MouseUpEvent evt)
    {
        if (!enabled)
            return;

        enabled = false;
        target.ReleaseMouse();

        evt.StopPropagation();
    }
}

using UnityEngine;
using UnityEngine.UIElements;

public class ResizeHandler : PointerManipulator
{
    private Vector2 startMousePosition;
    private Vector2 startPanelSize;
    private Vector2 startPanelPosition;
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
        if (evt.button == 1)
        {
            startMousePosition = evt.mousePosition;
            startPanelSize = new Vector2(target.resolvedStyle.width, target.resolvedStyle.height);
            startPanelPosition = new Vector2(target.resolvedStyle.left, target.resolvedStyle.top);

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

        if (evt.altKey) // scale from center when alt is pressed
        {
            float newWidth = Mathf.Max(50, startPanelSize.x + (delta.x * 2));
            float newHeight = Mathf.Max(50, startPanelSize.y + (delta.y * 2));

            float newLeft = startPanelPosition.x - delta.x;
            float newTop = startPanelPosition.y - delta.y;

            target.style.width = newWidth;
            target.style.height = newHeight;
            target.style.left = newLeft;
            target.style.top = newTop;
        }
        else // scale from top left
        {
            target.style.width = Mathf.Max(50, startPanelSize.x + delta.x);
            target.style.height = Mathf.Max(50, startPanelSize.y + delta.y);
        }
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

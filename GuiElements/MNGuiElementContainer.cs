using System;
using System.Collections.Generic;
using Cairo;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using MNGui.Extensions;
using MNGui.GuiElements.Layout;

namespace MNGui.GuiElements;

/// <summary>
/// Container GuiElement that supports layouting based on child elements' ElementBounds hierarchy.
/// This doesn't change the hierarchy of added elements; Logic outside is responsible for constructing it.
/// </summary>
public class MNGuiElementContainer : GuiElement {
    // -- Rendering --
    LoadedTexture contentTexture;
    bool renderFocusHighlight;

    // -- Eelement groups --
    public List<GuiElement> Elements { get; protected set; } = new();

    public List<GuiElement> InteractiveElements { get; protected set; } = new();
    public Dictionary<string, GuiElement> NamedElements { get; protected set; } = new();

    // -- Focus/Tab -- 
    #region Focus/Tab
    protected int currentFocusableElementCount;

    public bool Tabbable = false;

    public override bool Focusable { get { return Tabbable; } }

    public GuiElement? CurrentTabIndexElement {
        get {
            foreach (GuiElement element in InteractiveElements) {
                if (element.Focusable && element.HasFocus) {
                    return element;
                }
            }

            return null;
        }
    }

    public GuiElement? FirstTabbableElement {
        get {
            foreach (GuiElement element in InteractiveElements) {
                if (element.Focusable) {
                    return element;
                }
            }

            return null;
        }
    }

    public int MaxTabIndex {
        get {
            int tabIndex = -1;
            foreach (GuiElement element in InteractiveElements) {
                if (element.Focusable) {
                    tabIndex = Math.Max(tabIndex, element.TabIndex);
                }
            }

            return tabIndex;
        }
    }
    #endregion

    // -- InsideClipBounds --
    protected Stack<ElementBounds> InsideClipBoundsStack = new();

    public override ElementBounds InsideClipBounds {
        get => base.InsideClipBounds;
        set {
            foreach (var element in Elements) {
                if (element.InsideClipBounds == base.InsideClipBounds) {
                    element.InsideClipBounds = value;
                }
            }

            base.InsideClipBounds = value;
        }
    }

    // -- Events --
    public ActionConsumable<bool>? EventRelayoutRequired { get; set; }

    protected Action? EventNotifyRelayoutRequiredToParent { get; set; }

    public MNGuiElementContainer(ICoreClientAPI capi, ElementBounds bounds) : base(capi, bounds) {
        contentTexture = new LoadedTexture(capi);
        bounds.IsDrawingSurface = true;
    }

    /// <summary>
    /// Adds an element as a child of this container. Calling this for each child element and <see cref="SetChildBound(ElementBounds)"/> once needs to work properly.
    /// </summary>
    /// <param name="elem"></param>
    /// <param name="name"></param>
    public void Add(GuiElement elem, string? name = null) {
        Elements.Add(elem);

        if (IsInteractiveElement(elem)) {
            InteractiveElements.Add(elem);

            // Tabbable only interactive elements
            if (elem.Focusable) {
                elem.TabIndex = currentFocusableElementCount++;
                Tabbable = true;
            }
            else {
                elem.TabIndex = -1;
            }
        }

        if (name != null) {
            NamedElements[name] = elem;
        }


        if (elem is MNGuiElementClipStart clipStart) {
            InsideClipBoundsStack.Push(clipStart.Bounds);
        }
        else if (elem is MNGuiElementClipEnd clipEnd) {
            InsideClipBoundsStack.Pop();
        }
        else {
            if (InsideClipBoundsStack.Count == 0) {
                elem.InsideClipBounds = InsideClipBounds;
            }
            else {
                elem.InsideClipBounds = InsideClipBoundsStack.Peek();
            }
        }

        if (elem is MNGuiElementContainer container) {
            container.OnAddedToContainer(this);
        }
    }

    /// <summary>
    /// Set the passed bounds as a child of this container's bounds.
    /// You need to pass the top bounds of child element bounds' hierarchy.
    /// Calling <see cref="Add(GuiElement, string?)" /> for each element and this once needs to work properly.
    /// </summary>
    /// <param name="bounds"></param>
    public void SetChildBound(ElementBounds bounds) {
        Bounds.RemoveAllChildBounds();
        Bounds.WithChildForce(bounds);
    }

    /// <summary>
    /// Reset content states. Elements are removed but not disposed, for future re-adding.
    /// </summary>
    public virtual void ClearContent() {
        foreach (var elem in Elements) {
            if (elem is MNGuiElementContainer container) {
                container.OnRemovedFromContainer(this);
            }
        }
        Elements.Clear();
        InteractiveElements.Clear();
        NamedElements.Clear();
        Bounds.RemoveAllChildBounds();
        InsideClipBoundsStack.Clear();
        currentFocusableElementCount = 0;
        Tabbable = false;

        // Events are not content, so handlers are not cleared here
    }

    /// <summary>
    /// ClearContent + element.Dispose for each element in Elements. Mainly for before constructing and adding new layouts and GuiElements
    /// </summary>
    public virtual void DiscardContent() {
        foreach (var element in Elements) {
            element.Dispose();
        }
        ClearContent();
    }

    /// <summary>
    /// API to parent: should be called when this container is added to the parent
    /// </summary>
    /// <param name="parentContainer"></param>
    protected void OnAddedToContainer(MNGuiElementContainer parentContainer) {
        EventNotifyRelayoutRequiredToParent = parentContainer.OnChildContainerNotifyRelayoutRequired;

    }

    /// <summary>
    /// API to parent: should be called when this container is removed from the parent
    /// </summary>
    /// <param name="parentContainer"></param>
    protected void OnRemovedFromContainer(MNGuiElementContainer parentContainer) {
        if (EventNotifyRelayoutRequiredToParent == parentContainer.OnChildContainerNotifyRelayoutRequired) {
            EventNotifyRelayoutRequiredToParent = null;
        }
    }

    /// <summary>
    /// Handler when received notification of RelayoutRequired from a child container
    /// </summary>
    protected void OnChildContainerNotifyRelayoutRequired() {
        NotifyExternalThenPropagate(false);
    }

    /// <summary>
    /// Notify this containe's layout is changed and need to be re-layout (Measure and Arrange), to external handler and (unless it's consumed) parent
    /// </summary>
    /// <remarks>
    /// MNGuiElementContainer has no timing to notify by itself, so this need to be called from outside when elements are ready
    /// </remarks>
    public void NotifyRelayoutRequired() {
        NotifyExternalThenPropagate(true);
    }

    protected void NotifyExternalThenPropagate(bool fromThis) {
        var consumed = EventRelayoutRequired?.Invoke(fromThis);

        // If the event is consumed, don't propagate to parent
        if (consumed == true) return;

        // Propagate to parent
        EventNotifyRelayoutRequiredToParent?.Invoke();
    }

    public static bool IsInteractiveElement(GuiElement element) {
        var isInteractive = element switch {
            MNGuiElementStaticBase => false,
            GuiElementStaticText => false,
            _ => true
        };

        return isInteractive;
    }

    public override void BeforeCalcBounds() {
        base.BeforeCalcBounds();
        CalcContentSize();
    }

    public void CalcContentSize() {
        foreach (GuiElement cell in Elements) {
            cell.BeforeCalcBounds();
        }

        Bounds.CalcWorldBounds();
    }

    public override void ComposeElements(Context ctx, ImageSurface surface) {
        Bounds.CalcWorldBounds();
        ComposeContent();
    }

    protected void ComposeContent() {
        ImageSurface surface = new ImageSurface(Format.Argb32, (int)Bounds.OuterWidth, (int)Bounds.OuterHeight);
        Context ctx = genContext(surface);

        CalcContentSize();

        foreach (GuiElement elem in Elements) {
            elem.ComposeElements(ctx, surface);
        }

        generateTexture(surface, ref contentTexture);

        ctx.Dispose();
        surface.Dispose();
    }


    public override void OnMouseUp(ICoreClientAPI api, MouseEvent args) {
        // TODO: is this guard correct?
        //if (!InsideClipBounds.PointInside(args.X, args.Y)) return;

        foreach (GuiElement element in InteractiveElements) {
            element.OnMouseUp(api, args);
        }

        if (!args.Handled) base.OnMouseUp(api, args);
    }

    public override void OnMouseDown(ICoreClientAPI api, MouseEvent args) {
        // TODO: is this guard correct?
        if (InsideClipBounds?.PointInside(args.X, args.Y) == false) return;

        renderFocusHighlight = false;
        bool triggered = false;

        foreach (GuiElement element in InteractiveElements) {
            var handledThis = false;
            if (!triggered) {
                element.OnMouseDown(api, args);
                handledThis = args.Handled;
            }

            if (!triggered && handledThis) {
                if (element.Focusable && !element.HasFocus) {
                    element.OnFocusGained();
                }
            }
            else {
                if (element.Focusable && element.HasFocus) {
                    element.OnFocusLost();
                }
            }

            triggered = triggered || handledThis;
        }

        if (!args.Handled) base.OnMouseDown(api, args);
    }


    public override void OnMouseMove(ICoreClientAPI api, MouseEvent args) {
        foreach (GuiElement element in InteractiveElements) {
            element.OnMouseMove(api, args);
            if (args.Handled) break;
        }

        if (!args.Handled) base.OnMouseMove(api, args);
    }


    bool tabPressed = false;
    bool shiftTabPressed = false;
    public override void OnKeyDown(ICoreClientAPI api, KeyEvent args) {
        tabPressed = args.KeyCode == (int)GlKeys.Tab;
        shiftTabPressed = tabPressed && args.ShiftPressed;

        if (!HasFocus) return;

        base.OnKeyDown(api, args);

        foreach (GuiElement element in InteractiveElements) {
            element.OnKeyDown(api, args);
            if (args.Handled) break;
        }

        if (!args.Handled && args.KeyCode == (int)GlKeys.Tab && Tabbable) {
            renderFocusHighlight = true;
            var elem = CurrentTabIndexElement;
            if (elem != null && MaxTabIndex > 0) {
                int dir = args.ShiftPressed ? -1 : 1;
                int nextTabIndex = elem.TabIndex + dir;
                if (nextTabIndex < 0 || nextTabIndex > MaxTabIndex || args.CtrlPressed) return;
                FocusElement(nextTabIndex);
                args.Handled = true;
            }
            else if (MaxTabIndex > 0) {
                FocusElement(args.ShiftPressed ? GameMath.Mod(-1, MaxTabIndex + 1) : 0);
                args.Handled = true;
            }
        }

        if (!args.Handled && (args.KeyCode == (int)GlKeys.Enter || args.KeyCode == (int)GlKeys.KeypadEnter) && CurrentTabIndexElement is GuiElementEditableTextBase) {
            UnfocusChildElements();
        }
    }

    public override void OnKeyUp(ICoreClientAPI api, KeyEvent args) {
        tabPressed = false;
        shiftTabPressed = false;

        if (!HasFocus) return;

        base.OnKeyUp(api, args);

        foreach (GuiElement element in InteractiveElements) {
            element.OnKeyUp(api, args);
            if (args.Handled) break;
        }
    }

    public override void OnKeyPress(ICoreClientAPI api, KeyEvent args) {
        if (!HasFocus) return;

        base.OnKeyPress(api, args);

        foreach (GuiElement element in InteractiveElements) {
            element.OnKeyPress(api, args);
            if (args.Handled) break;
        }
    }

    public override void OnMouseWheel(ICoreClientAPI api, MouseWheelEventArgs args) {
        if (!Bounds.ParentBounds.PointInside(api.Input.MouseX, api.Input.MouseY)) return;
        // TODO: is this guard correct?
        if (InsideClipBounds?.PointInside(api.Input.MouseX, api.Input.MouseY) == false) return;

        // Prefer an element that is currently hovered 
        foreach (var element in InteractiveElements) {
            if (element.IsPositionInside(api.Input.MouseX, api.Input.MouseY)) {
                element.OnMouseWheel(api, args);
            }

            if (args.IsHandled) return;
        }

        foreach (GuiElement element in InteractiveElements) {
            element.OnMouseWheel(api, args);
            if (args.IsHandled) break;
        }
    }

    public override void RenderInteractiveElements(float deltaTime) {
        api.Render.Render2DTexturePremultipliedAlpha(contentTexture.TextureId, Bounds);

        MouseOverCursor = null;
        foreach (GuiElement element in InteractiveElements) {
            element.RenderInteractiveElements(deltaTime);

            if (element.IsPositionInside(api.Input.MouseX, api.Input.MouseY)) {
                MouseOverCursor = element.MouseOverCursor;
            }
        }

        ElementBounds tempClipBounds;
        foreach (GuiElement element in InteractiveElements) {
            if (element.HasFocus && renderFocusHighlight) {
                if (InsideClipBounds != null) {
                    tempClipBounds = element.InsideClipBounds;
                    element.InsideClipBounds = null;
                    element.RenderFocusOverlay(deltaTime);
                    element.InsideClipBounds = tempClipBounds;
                }
                else {
                    element.RenderFocusOverlay(deltaTime);
                }
            }
        }
    }

    public override void Dispose() {
        base.Dispose();
        contentTexture.Dispose();

        foreach (var val in Elements) {
            val.Dispose();
        }
        ClearContent();

        EventRelayoutRequired = null;
        EventNotifyRelayoutRequiredToParent = null;
    }

    public override void RenderBoundsDebug() {
        base.RenderBoundsDebug();
        foreach (var elem in Elements) {
            // Skip layout elements if Outline mode is 2
            if (GuiComposer.Outlines == 2) {
                if (elem is GuiElementDebugHorizontalLayout || elem is GuiElementDebugVerticalLayout) {
                    continue;
                }
            }
            elem.RenderBoundsDebug();
        }
    }

    public override int OutlineColor() {
        var intVal = ColorUtil.ToRgba(255, 128, 255, 128);
        return intVal;
    }


    public bool FocusElement(int tabIndex) {
        GuiElement? newFocusedElement = null;

        foreach (GuiElement element in InteractiveElements) {
            if (element.Focusable && element.TabIndex == tabIndex) {
                newFocusedElement = element;
                break;
            }
        }

        if (newFocusedElement != null) {
            UnfocusChildElementsExcept(newFocusedElement);
            newFocusedElement.OnFocusGained();
            return true;
        }

        return false;
    }

    public void UnfocusChildElements() {
        UnfocusChildElementsExcept(null);
    }

    /// <summary>
    /// Unfocuses all focusable child elements, optionally keeping one in focus.
    /// Pass <see langword="null"/> to unfocus all elements.
    /// </summary>
    /// <param name="excluded">The element to keep focused, or <see langword="null"/> to unfocus all.</param>
    public void UnfocusChildElementsExcept(GuiElement? excluded) {
        foreach (GuiElement element in Elements) {
            if (element == excluded) continue;

            if (element.Focusable && element.HasFocus) {
                element.OnFocusLost();
            }
        }
    }

    public override void OnFocusGained() {
        base.OnFocusGained();

        if (CurrentTabIndexElement != null) return;

        renderFocusHighlight = tabPressed;
        if (shiftTabPressed) FocusElement(MaxTabIndex);
        else FocusElement(FirstTabbableElement.TabIndex);
    }

    public override void OnFocusLost() {
        base.OnFocusLost();

        renderFocusHighlight = false;
        UnfocusChildElements();
    }
}


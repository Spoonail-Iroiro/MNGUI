using System;
using System.Collections.Generic;
using MNGui.Exceptions;
using MNGui.Extensions;
using MNGui.GuiElements;
using MNGui.Layouts;
using Vintagestory.API.Client;
using Vintagestory.API.Util;

namespace MNGui.DialogBuilders;
public class ContainerDialogController {
    protected ICoreClientAPI capi;

    public LayoutBase ChildLayout { get; protected set; }

    public GuiComposer Composer { get; protected set; }

    // ReCompose is deferred by a context
    protected bool isRecomposeDeferredByContext = false;

    // True while this controller is executing ReCompose
    protected bool isRecomposing = false;

    // A ReCompose request occurred during ReCompose
    protected bool isRecomposeRequested = false;

    public ContainerDialogController(ICoreClientAPI capi, GuiComposer composer, LayoutBase childLayout, bool isRoot = true) {
        this.capi = capi;
        ChildLayout = childLayout;
        Composer = composer;

        if (isRoot) {
            var container = GetMainContainerElement();
            container!.EventRelayoutRequired = fromThis => { OnBoundsUpdated(); return true; };
        }
    }

    public MNGuiElementContainer? GetMainContainerElement() {
        return Composer.GetElement<MNGuiElementContainer>(ContainerDialogBuilder.MainContainerName);
    }

    public T GetElement<T>(string name) where T : class {
        var elem = GetElementSafe<T>(name);
        if (elem == null) throw new GuiElementLookupException(name, $"Couldn't find element '{name}' with type {typeof(T).Name}");

        return elem;
    }

    public T? GetElementSafe<T>(string name) where T : class {
        var stack = new Stack<MNGuiElementContainer>();
        var mainContainer = GetMainContainerElement();
        if (mainContainer != null) stack.Push(mainContainer);
        while (stack.Count > 0) {
            var container = stack.Pop();
            var containerElement = container.NamedElements!.Get(name) as T;
            if (containerElement != null) return containerElement;

            foreach (var elem in container.Elements) {
                if (elem is MNGuiElementContainer childContainer) {
                    stack.Push(childContainer);
                }
            }
        }

        var composerElement = Composer.GetElement<T>(name);
        if (composerElement != null) return composerElement;

        return null;
    }

    int recomposeCountMax = 100;

    public void OnBoundsUpdated() {
        var container = GetMainContainerElement();
        if (container == null) return;

        if (ChildLayout is LayoutWithElementBounds lweb) {
            ChildLayout.Init();

            ChildLayout.Measure();

            container.Bounds.CalcWorldBounds();

            lweb.ArrangeWithMinSize();

            if (!isRecomposeDeferredByContext) {
                SerializedRecompose();
            }
        }
    }

    protected void SerializedRecompose() {
        if (isRecomposing) {
            isRecomposeRequested = true;
            return;
        }

        isRecomposing = true;
        try {
            var i = 0;

            do {
                if (i++ >= recomposeCountMax) {
                    throw new InvalidOperationException($"Infinite ReCompose detected!");
                }
                isRecomposeRequested = false;
                Composer.ReCompose();
            } while (isRecomposeRequested);
        }
        finally {
            isRecomposing = false;
        }
    }

    public LayoutInitializeContext StartLayoutInitializeContext() {
        return new LayoutInitializeContext(this);
    }

    protected void ResolvePendingLayoutImmediatelyAll() {
        var stack = new Stack<MNGuiElementContainer>();
        var mainContainer = GetMainContainerElement();
        if (mainContainer != null) stack.Push(mainContainer);
        while (stack.Count > 0) {
            var container = stack.Pop();
            if (container is MNGuiElementInnerLayoutContainer innerContainer) {
                innerContainer.ResolvePendingNewLayout();
            }

            foreach (var elem in container.Elements) {
                if (elem is MNGuiElementContainer childContainer) {
                    stack.Push(childContainer);
                }
            }
        }
    }

    public class LayoutInitializeContext : IDisposable {
        ContainerDialogController dialogController;
        public LayoutInitializeContext(ContainerDialogController dialogController) {
            this.dialogController = dialogController;
            dialogController.isRecomposeDeferredByContext = true;
        }

        public void Dispose() {
            this.dialogController.ResolvePendingLayoutImmediatelyAll();
            dialogController.isRecomposeDeferredByContext = false;
            dialogController.SerializedRecompose();
        }
    }
}


using MNGui.Extensions;
using MNGui.GuiElements;
using MNGui.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Util;

namespace MNGui.DialogBuilders;
public class StandardDialogController {
    protected ICoreClientAPI capi;

    public LayoutBase ChildLayout { get; protected set; }

    public GuiComposer Composer { get; protected set; }

    public StandardDialogController(ICoreClientAPI capi, GuiComposer composer, LayoutBase childLayout) {
        this.capi = capi;
        ChildLayout = childLayout;
        Composer = composer;
    }

    public MNGuiElementContainer? GetMainContainerElement() {
        return Composer.GetElement<MNGuiElementContainer>(StandardDialogBuilder.MainContainerName);
    }

    public MNGuiElementVerticalScrollbar? GetScrollbarElement() {
        return Composer.GetElement<MNGuiElementVerticalScrollbar>(StandardDialogBuilder.ScrollbarName);
    }

    public T? GetElement<T>(string name) where T : class {
        var container = GetMainContainerElement();
        if (container != null) {
            var containerElement = container.NamedElements!.Get(name) as T;
            if (containerElement != null) return containerElement;
        }

        var composerElement = Composer.GetElement<T>(name);
        if (composerElement != null) return composerElement;

        return null;
    }

    public void OnBoundsUpdated() {
        var container = GetMainContainerElement();
        if (container == null) return;

        if (ChildLayout is LayoutWithElementBounds lweb) {
            //container.Clear();

            ChildLayout.Measure();

            //foreach (var elementInfo in ChildLayout.GetAllGuiElements()) {
            //    container.Add(elementInfo.Element, elementInfo.Name);
            //}

            //container.SetChildBound(lweb.Bounds!);

            container.Bounds.CalcWorldBounds();

            lweb.ArrangeWithMinSize();

            Composer.ReCompose();

            GetScrollbarElement()?.OnBoundsUpdated();
        }
    }

}

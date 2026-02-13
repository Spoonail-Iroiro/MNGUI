using System;
using System.Diagnostics.CodeAnalysis;
using Vintagestory.API.Client;
using MNGui.Layouts;
using MNGui.GuiElements;
using MNGui.Extensions;

namespace MNGui.DialogBuilders;
public class ContainerDialogBuilder {
    public static readonly string MainContainerName = "container-main";
    public static readonly string ScrollbarName = "scrollbar-main";

    public bool IsStandardCloseEnabled { get; private set; } = true;

    // "root" layout must have ElementBounds for getting child of the MNGuiElementContainer
    public LayoutWithElementBounds? ChildLayout { get; private set; }

    public static MNGuiElementContainer? GetMainContainerElement(GuiComposer composer) {
        return composer.GetElement<MNGuiElementContainer>(MainContainerName);
    }

    public ContainerDialogBuilder() {
    }

    public ContainerDialogBuilder WithStandardClose(bool enabled) {
        IsStandardCloseEnabled = enabled;
        return this;
    }

    [MemberNotNull(nameof(ChildLayout))]
    public void SetChildLayout(LayoutWithElementBounds layout) {
        ChildLayout = layout;
    }

    public GuiComposer Layout(ICoreClientAPI capi, GuiDialogBlockEntity gui) {
        return Layout(capi, gui, gui.GetType().Name + gui.BlockEntityPosition);
    }

    public GuiComposer Layout(ICoreClientAPI capi, GuiDialogGeneric gui, string dialogId) {
        var dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
        dialogBounds.Name = "bounds-dialog";

        var bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
        bgBounds.Name = "bounds-bg";
        bgBounds.BothSizing = ElementSizing.FitToChildren;

        var containerBounds = bgBounds.ForkContainingChild();
        containerBounds.WithFixedPadding(2.0);
        containerBounds.fixedY = GuiStyle.TitleBarHeight;
        containerBounds.BothSizing = ElementSizing.FitToChildren;
        containerBounds.Name = "bounds-container";

        var composer = capi.Gui.CreateCompo(dialogId, dialogBounds);
        composer
            .AddShadedDialogBG(bgBounds)
            .AddDialogTitleBar(gui.DialogTitle, () => { if (IsStandardCloseEnabled) gui.TryClose(); })
            .BeginChildElements(bgBounds) // Begin bgBounds child
                .AddInteractiveElement(new MNGuiElementContainer(capi, containerBounds), MainContainerName)
            .EndChildElements();

        var container = composer.GetElement<MNGuiElementContainer>(MainContainerName)!;

        if (ChildLayout == null) throw new InvalidOperationException($"{typeof(StandardDialogBuilder).Name} can't generate dialog without ChildLayout!");

        ChildLayout.Init();

        ChildLayout.Measure();

        foreach (var elementInfo in ChildLayout.GetAllGuiElements()) {
            container.Add(elementInfo.Element, elementInfo.Name);
        }

        container.SetChildBound(ChildLayout.Bounds);

        container.Bounds.CalcWorldBounds();

        ChildLayout.ArrangeWithMinSize();

        composer.Compose();

        return composer;
    }

}

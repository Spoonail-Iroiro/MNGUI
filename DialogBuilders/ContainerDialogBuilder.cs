using MNGui.Extensions;
using MNGui.GuiElements;
using MNGui.Layouts;
using Vintagestory.API.Client;

namespace MNGui.DialogBuilders;
public class ContainerDialogBuilder {
    public static readonly string MainContainerName = "container-containerdialogbuilder-main";
    public static readonly string ScrollbarName = "scrollbar-containerdialogbuilder-main";

    public bool IsStandardCloseEnabled { get; private set; } = true;
    public string? CustomDialogTitle { get; private set; }
    public string? CustomDialogId { get; private set; }
    public ElementBounds? CustomDialogBounds { get; private set; }

    public static MNGuiElementContainer? GetMainContainerElement(GuiComposer composer) {
        return composer.GetElement<MNGuiElementContainer>(MainContainerName);
    }

    public ContainerDialogBuilder() {
    }

    public ContainerDialogBuilder WithoutStandardClose() {
        IsStandardCloseEnabled = false;
        return this;
    }

    public ContainerDialogBuilder WithCustomDialogTitle(string dialogTitle) {
        CustomDialogTitle = dialogTitle;
        return this;
    }

    public ContainerDialogBuilder WithCustomDialogId(string dialogId) {
        CustomDialogId = dialogId;
        return this;
    }

    public ContainerDialogBuilder WithCustomDialogBounds(ElementBounds bounds) {
        CustomDialogBounds = bounds;
        return this;
    }

    public GuiComposer Build(ICoreClientAPI capi, LayoutWithElementBounds layout, GuiDialogBlockEntity gui) {
        return Build(capi, layout, gui, CustomDialogId ?? gui.GetType().Name + gui.BlockEntityPosition);
    }

    public GuiComposer Build(ICoreClientAPI capi, LayoutWithElementBounds layout, GuiDialogGeneric gui) {
        return Build(capi, layout, gui, CustomDialogId ?? gui.GetType().Name);
    }

    GuiComposer Build(ICoreClientAPI capi, LayoutWithElementBounds layout, GuiDialogGeneric gui, string dialogId) {
        var dialogBounds = CustomDialogBounds ?? ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
        dialogBounds.Name = "bounds-dialog";

        // Padding to avoid container content clipped
        var containerPadding = 2.0;

        var bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding - containerPadding);
        bgBounds.Name = "bounds-bg";
        bgBounds.BothSizing = ElementSizing.FitToChildren;

        var containerBounds = bgBounds.ForkContainingChild();
        containerBounds.WithFixedPadding(containerPadding);
        containerBounds.fixedY = GuiStyle.TitleBarHeight;
        containerBounds.BothSizing = ElementSizing.FitToChildren;
        containerBounds.Name = "bounds-container";

        var composer = capi.Gui.CreateCompo(dialogId, dialogBounds);
        composer
            .AddShadedDialogBG(bgBounds)
            .AddDialogTitleBar(CustomDialogTitle ?? gui.DialogTitle, () => { if (IsStandardCloseEnabled) gui.TryClose(); })
            .BeginChildElements(bgBounds) // Begin bgBounds child
                .AddInteractiveElement(new MNGuiElementContainer(capi, containerBounds), MainContainerName)
            .EndChildElements();

        var container = composer.GetElement<MNGuiElementContainer>(MainContainerName)!;

        layout.Init();

        layout.Measure();

        foreach (var elementInfo in layout.GetAllGuiElements()) {
            container.Add(elementInfo.Element, elementInfo.Name);
        }

        container.SetChildBound(layout.Bounds);

        container.Bounds.CalcWorldBounds();

        layout.ArrangeWithMinSize();

        composer.Compose();

        return composer;
    }
}

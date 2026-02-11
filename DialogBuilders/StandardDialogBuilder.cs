using MNGui.Layouts;
using MNGui.GuiElements;
using System;
using Vintagestory.API.Client;
using MNGui.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace MNGui.DialogBuilders {
    public class StandardDialogBuilder {
        double fixedHeight;

        // "root" layout must have ElementBounds for getting child of the MNGuiElementContainer
        public LayoutWithElementBounds? ChildLayout { get; private set; }

        public static readonly string MainContainerName = "container-main";
        public static readonly string ScrollbarName = "scrollbar-main";

        public static MNGuiElementContainer? GetMainContainerElement(GuiComposer composer) {
            return composer.GetElement<MNGuiElementContainer>(MainContainerName);
        }

        public static MNGuiElementVerticalScrollbar? GetScrollbarElement(GuiComposer composer) {
            return composer.GetElement<MNGuiElementVerticalScrollbar>(ScrollbarName);
        }

        public StandardDialogBuilder(double fixedHeight = 400) {
            this.fixedHeight = fixedHeight;
        }

        [MemberNotNull(nameof(ChildLayout))]
        public void SetChildLayout(LayoutWithElementBounds layout) {
            ChildLayout = layout;
        }

        public GuiComposer Layout(ICoreClientAPI capi, GuiDialogBlockEntity gui) {
            return Layout(capi, gui, nameof(gui) + gui.BlockEntityPosition);
        }

        public GuiComposer Layout(ICoreClientAPI capi, GuiDialogGeneric gui, string dialogId) {
            var dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            dialogBounds.Name = "bounds-dialog";

            var bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.Name = "bounds-bg";
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            var insetBounds = ElementBounds.Fixed(0, GuiStyle.TitleBarHeight, 10, fixedHeight + GuiStyle.HalfPadding * 2);
            insetBounds.Name = "bounds-inset";
            insetBounds.horizontalSizing = ElementSizing.FitToChildren;
            //bgBounds.WithChild(insetBounds);

            var scrollBarBounds = insetBounds.CopyOffsetedSibling()
                .WithFixedWidth(20)
                .WithSizing(ElementSizing.Fixed);
            scrollBarBounds.Name = "bounds-scroll-bar";
            scrollBarBounds.RightOf(insetBounds, 3);

            // Bounds to add paddings between the inset and the clip
            // Adding paddings to the inset doesnt work: inset drawing breaks
            // TODO: Try adding paddings to the clip - but doesn't sound right
            var clipParentBounds = insetBounds.ForkContainingChild();
            clipParentBounds.Name = "bounds-clipparent";
            clipParentBounds.WithFixedPadding(GuiStyle.HalfPadding);
            clipParentBounds.horizontalSizing = ElementSizing.FitToChildren;

            var clipBounds = clipParentBounds.ForkContainingChild();
            clipBounds.Name = "bounds-clip";
            clipBounds.horizontalSizing = ElementSizing.FitToChildren;

            var containerBounds = clipBounds.ForkContainingChild();
            containerBounds.BothSizing = ElementSizing.FitToChildren;
            containerBounds.Name = "bounds-container";

            var composer = capi.Gui.CreateCompo(dialogId, dialogBounds);
            composer
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(gui.DialogTitle, () => gui.TryClose())
                .BeginChildElements(bgBounds) // Begin bgBounds child
                    .AddInset(insetBounds, 3)
                    .BeginChildElements() // Begin insetBounds child
                        .BeginChildElements(clipParentBounds) // Begin clipParentBounds (now child of insetBounds) child
                            .BeginClip(clipBounds) // Begin clipBounds child (auto)
                                .AddInteractiveElement(new MNGuiElementContainer(capi, containerBounds), MainContainerName)
                            .EndClip()
                        .EndChildElements()
                    .EndChildElements()
                    //.AddVerticalScrollbar(val => OnNewScrollbarvalue(composer, val), scrollBarBounds, "scrollbar-main")
                    .AddInteractiveElement(new MNGuiElementVerticalScrollbar(capi, scrollBarBounds), ScrollbarName)
                .EndChildElements();

            // Scroll bar setting

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

            var mainScrollBar = composer.GetElement<MNGuiElementVerticalScrollbar>(ScrollbarName)!;
            mainScrollBar.SetViewAndContentBounds(clipBounds, containerBounds);
            mainScrollBar.OnBoundsUpdated();

            return composer;
        }
    }
}

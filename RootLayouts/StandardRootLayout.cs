using MNGUI.Layouts;
using MNGUI.GUIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory;
using MNGUITest;
using MNGUI.Extensions;
using MNGUI.GUIElements.Layout;

namespace MNGUI.RootLayouts {
    public class StandardRootLayout {
        double fixedHeight;
        double scrollBarContentFixedY;

        // "root" layout must have ElementBounds for getting child of the MNGuiElementContainer
        public LayoutWithElementBounds ChildLayout { get; private set; }

        public Dictionary<string, GuiElement> NamedElements { get; private set; }

        public StandardRootLayout(double fixedHeight = 400) {
            this.fixedHeight = fixedHeight;
        }

        public void SetChildLayout(LayoutWithElementBounds layout) {
            ChildLayout = layout;
            //RegisterNamedElements();
        }

        //protected void RegisterNamedElements() {
        //    NamedElements = new Dictionary<string, GuiElement>();
        //    var layoutStack = new Stack<LayoutBase>();
        //    layoutStack.Push(ChildLayout);

        //    while (layoutStack.Count > 0) {
        //        var currentLayout = layoutStack.Pop();
        //        if (currentLayout is HorizontalLayout hl) {
        //            foreach (var l in hl.ChildLayouts) {
        //                layoutStack.Push(l);
        //            }
        //        }
        //        else if (currentLayout is VerticalLayout vl) {
        //            foreach (var l in vl.ChildLayouts) {
        //                layoutStack.Push(l);
        //            }
        //        }
        //        else if (currentLayout is SingleLayout sl) {
        //            if (sl.Name != null) {
        //                NamedElements[sl.Name] = sl.GuiElement;
        //            }
        //        }
        //        else {
        //            throw new NotImplementedException();
        //        }
        //    }
        //}

        //public T GetNamedElement<T>(string name) where T : class {
        //    var rtn = GetNamedElementOrNull<T>(name);

        //    if (rtn == null) throw new InvalidOperationException($"No such a GuiElement: {name}");

        //    return rtn;
        //}

        public T GetNamedElementOrNull<T>(string name) where T : class {
            if (NamedElements == null) throw new InvalidOperationException($"ContainerComposer has not children!");
            var elem = NamedElements.GetValueOrDefault(name);
            if (elem == null) return null;

            var result = elem as T;
            if (result == null) throw new InvalidOperationException($"GuiElement {name} is found, but not a type {nameof(T)}. Actual: {elem.GetType().Name}");

            return elem as T;
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
            //insetBounds.WithChild(clipBounds);

            var containerBounds = clipBounds.ForkContainingChild();
            containerBounds.BothSizing = ElementSizing.FitToChildren;
            containerBounds.Name = "container";

            var composer = capi.Gui.CreateCompo(dialogId, dialogBounds);
            composer
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(gui.DialogTitle, () => gui.TryClose())
                .BeginChildElements(bgBounds) // Begin bgBounds child
                    .AddInset(insetBounds, 3)
                    .BeginChildElements() // Begin insetBounds child
                        .BeginChildElements(clipParentBounds) // Begin clipParentBounds (now child of insetBounds) child
                            .BeginClip(clipBounds) // Begin clipBounds child (auto)
                                .AddInteractiveElement(new MNGuiElementContainer(capi, containerBounds), "container-main")
                            .EndClip()
                        .EndChildElements()
                    .EndChildElements()
                    .AddVerticalScrollbar(val => OnNewScrollbarvalue(composer, val), scrollBarBounds, "scrollbar-main")
                .EndChildElements();

            // Scroll bar setting

            var container = composer.GetElement<MNGuiElementContainer>("container-main")!;

            ChildLayout.Measure();

            foreach (var element in ChildLayout.GetAllGuiElements()) {
                container.Add(element);
            }

            container.SetChildBound(ChildLayout.Bounds);

            container.Bounds.CalcWorldBounds();

            ChildLayout.Arrange();


            composer.Compose();


            //container.Bounds.CalcWorldBounds();

            var mainScrollBar = composer.GetScrollbar("scrollbar-main");
            mainScrollBar.SetHeights(scrollBarBounds.OuterHeightInt, (float)(containerBounds.OuterHeight + GuiStyle.HalfPadding * 2));
            scrollBarContentFixedY = container.Bounds.fixedY;

            return composer;
        }

        //public void UpdateContainerBounds() {
        //    var container = Composer.GetElement("main-container");
        //    container.Bounds.CalcWorldBounds();
        //    var mainScrollBar = Composer.GetScrollbar("scrollbar-main");
        //    mainScrollBar.SetHeights((float)mainScrollBar.Bounds.OuterHeight, (float)(container.Bounds.OuterHeight + GuiStyle.HalfPadding * 2));
        //}

        void OnNewScrollbarvalue(GuiComposer composer, float value) {
            var container = composer.GetElement("container-main");
            if (container == null) return;
            container.Bounds.fixedY = scrollBarContentFixedY - value;
            container.Bounds.CalcWorldBounds();
        }
    }
}

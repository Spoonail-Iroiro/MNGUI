using MNGUI.Layouts;
using MNGUI.GUIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;

namespace MNGUI.RootLayouts {
    internal class StandardRootLayout {
        double fixedHeight;
        double scrollBarContentFixedY;

        public LayoutBase ChildLayout { get; private set; }

        public Dictionary<string, GuiElement> NamedElements { get; private set; }

        public ElementBounds ContainerBounds { get; private set; }

        public GuiComposer Composer { get; private set; }

        public Action OnTitleBarClose;


        public StandardRootLayout(double fixedHeight = 400) {
            this.fixedHeight = fixedHeight;
        }

        public void SetChildLayout(LayoutBase layout) {
            ChildLayout = layout;

            RegisterNamedElements();

        }

        protected void RegisterNamedElements() {
            NamedElements = new Dictionary<string, GuiElement>();
            var layoutStack = new Stack<LayoutBase>();
            layoutStack.Push(ChildLayout);

            while (layoutStack.Count > 0) {
                var currentLayout = layoutStack.Pop();
                if (currentLayout is HorizontalLayout hl) {
                    foreach (var l in hl.ChildLayouts) {
                        layoutStack.Push(l);
                    }
                }
                else if (currentLayout is VerticalLayout vl) {
                    foreach (var l in vl.ChildLayouts) {
                        layoutStack.Push(l);
                    }
                }
                else if (currentLayout is SingleLayout sl) {
                    if (sl.Name != null) {
                        NamedElements[sl.Name] = sl.GuiElement;
                    }
                }
                else {
                    throw new NotImplementedException();
                }
            }
        }

        public T GetNamedElement<T>(string name) where T : class {
            var rtn = GetNamedElementOrNull<T>(name);

            if (rtn == null) throw new InvalidOperationException($"No such a GuiElement: {name}");

            return rtn;
        }

        public T GetNamedElementOrNull<T>(string name) where T : class {
            if (NamedElements == null) throw new InvalidOperationException($"ContainerComposer has not children!");
            var elem = NamedElements.GetValueOrDefault(name);
            if (elem == null) return null;

            var result = elem as T;
            if (result == null) throw new InvalidOperationException($"GuiElement {name} is found, but not a type {nameof(T)}. Actual: {elem.GetType().Name}");

            return elem as T;
        }

        public GuiComposer Layout(ICoreClientAPI capi, GuiDialogBlockEntity gui) {
            var dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

            var bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            var insetBounds = ElementBounds.Fixed(0, GuiStyle.TitleBarHeight, 10, fixedHeight + GuiStyle.HalfPadding * 2);
            insetBounds.horizontalSizing = ElementSizing.FitToChildren;
            bgBounds.WithChild(insetBounds);

            var scrollBarBounds = insetBounds.CopyOffsetedSibling()
                .WithFixedWidth(20)
                .WithSizing(ElementSizing.Fixed);
            scrollBarBounds.RightOf(insetBounds, 3);

            var clipBounds = insetBounds.ForkContainingChild(GuiStyle.HalfPadding, GuiStyle.HalfPadding, GuiStyle.HalfPadding, GuiStyle.HalfPadding); ;
            clipBounds.horizontalSizing = ElementSizing.FitToChildren;
            insetBounds.WithChild(clipBounds);

            var containerBounds = clipBounds.ForkContainingChild();
            containerBounds.BothSizing = ElementSizing.FitToChildren;
            containerBounds.Name = "container";
            ContainerBounds = containerBounds;

            Composer = capi.Gui.CreateCompo(nameof(gui) + gui.BlockEntityPosition, dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(gui.DialogTitle, OnTitleBarCloseInternal)
                .BeginChildElements(bgBounds)
                    .AddInset(insetBounds, 3)
                    .BeginClip(clipBounds)
                        .AddInteractiveElement(new MNGuiElementContainer(capi, containerBounds), "scroll-content")
                    .EndClip()
                    .AddVerticalScrollbar(OnNewScrollbarvalue, scrollBarBounds, "scroll-bar")
                .EndChildElements();

            // Scroll bar setting

            var container = Composer.GetElement("scroll-content") as MNGuiElementContainer;

            ChildLayout.Layout(container);

            container.Bounds.CalcWorldBounds();
            ChildLayout.BeforeComposerCompose();

            Composer.Compose();

            //container.Bounds.CalcWorldBounds();

            var mainScrollBar = Composer.GetScrollbar("scroll-bar");
            mainScrollBar.SetHeights(scrollBarBounds.OuterHeightInt, (float)(containerBounds.OuterHeight + GuiStyle.HalfPadding * 2));
            scrollBarContentFixedY = container.Bounds.fixedY;

            return Composer;
        }

        public void UpdateContainerBounds() {
            var container = Composer.GetElement("scroll-content") as MNGuiElementContainer;
            container.Bounds.CalcWorldBounds();
            var mainScrollBar = Composer.GetScrollbar("scroll-bar");
            mainScrollBar.SetHeights((float)mainScrollBar.Bounds.OuterHeight, (float)(container.Bounds.OuterHeight + GuiStyle.HalfPadding * 2));
        }

        public void ConnectToTitleBarClose(Action titleBarCloseHandler) {
            OnTitleBarClose = titleBarCloseHandler;
        }

        private void OnTitleBarCloseInternal() {
            OnTitleBarClose?.Invoke();
        }

        void OnNewScrollbarvalue(float value) {
            var container = Composer.GetElement("scroll-content") as MNGuiElementContainer;
            container.Bounds.fixedY = scrollBarContentFixedY - value;
            container.Bounds.CalcWorldBounds();
        }
    }
}

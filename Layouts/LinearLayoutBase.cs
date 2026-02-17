using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using MNGui.Extensions;
using MNGui.GuiElements;
using MNGui.GuiElements.Layout;
using MNGui.Util;
using MNGui.Layouts.Interfaces;

namespace MNGui.Layouts;

public abstract class LinearLayoutBase : LayoutWithElementBounds, IChildLayoutMixin {
    public HorizontalAlignment HorizontalAlignment { get; private set; }
    public VerticalAlignment VerticalAlignment { get; private set; }

    protected ICoreClientAPI capi;

    public int Gap { get; protected set; }

    // Paddings Currently Not Supported
    //public int HorizontalPadding { get; protected set; }
    //public int VerticalPadding { get; protected set; }

    public List<LayoutBase> ChildLayouts { get; protected set; } = new();

    public GuiElement? Element { get; protected set; }

    // Currently not used: for holding ElementBounds without GuiElement for future
    protected ElementBounds? bounds;

    public override ElementBounds? Bounds => (Element?.Bounds ?? bounds);

    public LinearLayoutBase(ICoreClientAPI capi, int gap, HorizontalAlignment hAlign, VerticalAlignment vAlign) {
        this.capi = capi;
        Gap = gap;

        HorizontalSizePolicy = SizePolicy.UnspecifiedLayout;
        VerticalSizePolicy = SizePolicy.UnspecifiedLayout;
        HorizontalStretchWeight = 1.0;
        VerticalStretchWeight = 1.0;

        HorizontalAlignment = hAlign;
        VerticalAlignment = vAlign;
    }

    public virtual void SetAlignment(HorizontalAlignment? horizontalAlignment, VerticalAlignment? verticalAlignment) {
        if (horizontalAlignment != null) {
            HorizontalAlignment = horizontalAlignment.Value;
        }

        if (verticalAlignment != null) {
            VerticalAlignment = verticalAlignment.Value;
        }
    }

    // TODO: shared default bounds? 
    protected ElementBounds CreateDefaultBounds() { return ElementBounds.FixedSize(100, 100).WithSizing(ElementSizing.FitToChildren); }

    /// <summary>
    /// Reset bound states while keeping them the same object. Copy default values from default bounds and remove children without wrong tree issue
    /// </summary>
    protected void ResetBounds() {
        if (Bounds != null) {
            Bounds.CopyFixedPropertiesFrom(CreateDefaultBounds());
            Bounds.RemoveAllChildBounds();
        }
    }

    public override IEnumerable<GuiElementInfo> GetAllGuiElements() {
        if (Element != null) {
            // Doesn't add with name - layout has no actual GuiElement, it's only for debug draw
            yield return new GuiElementInfo(Element, null);
        }

        foreach (LayoutBase layout in ChildLayouts) {
            foreach (var elem in layout.GetAllGuiElements()) {
                yield return elem;
            }
        }
    }

    /// <summary>
    /// Returns is it's stretching size policy that pripritized than UnspecifiedLayout + SpaceGreeding
    /// </summary>
    /// <param name="sizePolicy"></param>
    /// <returns></returns>
    public static bool IsStretchingSizePolicy(SizePolicy sizePolicy) {
        return sizePolicy == SizePolicy.Stretch || sizePolicy == SizePolicy.EnforceRatio;
    }

    public static List<double> CalcDistributedLength(
            double availableLength,
            List<double> minLengthes,
            List<SizePolicy> sizePolicies,
            List<double> stretchWeights
        ) {
        var remainingLength = availableLength - minLengthes.Sum();
        if (remainingLength < LayoutUtil.EPSILON_LENGTH) remainingLength = 0.0;
        // TODO: zero division cover
        var stretchWeightsDenominator = stretchWeights.Sum();

        var distLengthes = new List<double>();

        foreach (var (minLength, sizePolicy, stretchWeight) in Enumerable.Zip(minLengthes, sizePolicies, stretchWeights)) {
            switch (sizePolicy) {
                case SizePolicy.MinSize:
                    distLengthes.Add(minLength);
                    break;
                case SizePolicy.Stretch:
                    // TODO: proper algorythm - now just distributing remaining length
                    distLengthes.Add(minLength + remainingLength * stretchWeight / stretchWeightsDenominator);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        return distLengthes;
    }

    public static List<double> CalcAlignedPositions(double offset, List<double> distributedLengthes, double gap) {
        var currentPos = offset;
        var rtn = new List<double>();

        foreach (var length in distributedLengthes) {
            rtn.Add(currentPos);

            currentPos += length + gap;
        }

        return rtn;
    }
}

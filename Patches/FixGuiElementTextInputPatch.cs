using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Vintagestory.API.Client;

namespace MNGUI.Patches;

public static class FixedGLScissorMethods {
    public static void MyGlScissor(IRenderAPI render, int x, int y, int width, int height) {

    }

    public static void MyGlScissorFlag(IRenderAPI render, bool isStart, GuiElement guiElement) {
        if (isStart) {
            if (guiElement is GuiElementTextInput guiTI) {
                GlScissorPushForTextInput(render, guiTI);
            }
            else {
                GlScissorPushForCommonElement(render, guiElement);
            }
        }
        else {
            render.PopScissor();
        }
    }

    public static void GlScissorPushForCommonElement(IRenderAPI render, GuiElement guiElement) {
        render.PushScissor(guiElement.Bounds, true);
    }

    public static void GlScissorPushForTextInput(IRenderAPI render, GuiElementTextInput guiElement) {
        // This differs from vanilla when the GuiElementTextInput has non-zero bottomSpacing or rightSpacing 
        // But actually vanilla implementation never sets these values, so this case does not occur in practice
        render.PushScissor(guiElement.Bounds, true);
    }
}

public static class GLScissorPatchUtil {
    /// <summary>
    /// Replace brutal GlScissor method calls with stackable IRenderAPI.PushScissor and IRenderAPI.PopScissor
    /// </summary>
    /// <param name="codeMatcher"></param>
    /// <returns></returns>
    public static bool FixBrutalGlScissor(CodeMatcher codeMatcher) {
        codeMatcher.MatchStartForward(CodeMatch.Calls(() => default(IRenderAPI).GlScissor(0, 0, 0, 0)));

        // Couldn't find GlScissor: already patched
        if (codeMatcher.IsInvalid) {
            return false;
        }

        codeMatcher
            .RemoveInstruction()
            .Insert(CodeInstruction.Call(() => FixedGLScissorMethods.MyGlScissor(default, 0, 0, 0, 0)))
            .MatchEndForward(
                new CodeMatch(OpCodes.Ldc_I4_1),
                CodeMatch.Calls(() => default(IRenderAPI).GlScissorFlag(default))
            )
            .ThrowIfInvalid("GlScssorFlag(true) not found")
            .RemoveInstruction()
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                CodeInstruction.Call(() => FixedGLScissorMethods.MyGlScissorFlag(default, default, default))
            )
            .MatchEndForward(
                new CodeMatch(OpCodes.Ldc_I4_0),
                CodeMatch.Calls(() => default(IRenderAPI).GlScissorFlag(default))
            )
            .ThrowIfInvalid("GlScssorFlag(false) not found")
            .RemoveInstruction()
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                CodeInstruction.Call(() => FixedGLScissorMethods.MyGlScissorFlag(default, default, default))
            );

        return true;
    }
}


[HarmonyPatch(typeof(GuiElementTextInput), nameof(GuiElementTextInput.RenderInteractiveElements))]
public class FixGuiElementTextInputPatch {
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
        var codeMatcher = new CodeMatcher(instructions);

        GLScissorPatchUtil.FixBrutalGlScissor(codeMatcher);

        return codeMatcher.Instructions();
    }

}

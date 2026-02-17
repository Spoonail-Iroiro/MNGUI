using System.Collections.Generic;
using System.Linq;

namespace MNGui.Layouts;

public enum HorizontalAlignment {
    Left,
    Center,
    Right
}

public enum VerticalAlignment {
    Top,
    Middle,
    Bottom
}

//public enum BoxAlignment {
//    TopLeft,
//    TopCenter,
//    TopRight,
//    MiddleLeft,
//    MiddleCenter,
//    MiddleRight,
//    BottomLeft,
//    BottomCenter,
//    BottomRight
//}

//public class AlignmentUtil {
//    public static (HorizontalAlignment, VerticalAlignment) ToEachSideAlignments(BoxAlignment alignment) {
//        var hAlignment = (HorizontalAlignment)((int)alignment % 3);
//        var vAlignment = (VerticalAlignment)((int)alignment / 3);

//        return (hAlignment, vAlignment);
//    }
//}

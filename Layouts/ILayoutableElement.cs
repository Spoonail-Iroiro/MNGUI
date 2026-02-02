using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MNGui.Layouts;
public interface ILayoutableElement {
    public void Init();

    /// <summary>
    /// Called before parent layout's Measure, should prepare for set its Bounds representing MinSize
    /// </summary>
    /// <returns></returns>
    public void BeforeMeasure();

    /// <summary>
    /// Called after parent layout's Arrange, meaning the element has proper fixedX/fixedY/Width/Height for inner layouting
    /// </summary>
    public void AfterArrange();
}

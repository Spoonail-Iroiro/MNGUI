using System;
using System.Collections.Generic;
using System.Linq;

namespace MNGui.Exceptions;

public class GuiElementLookupException : Exception {
    public string ElementKey { get; protected set; }

    public GuiElementLookupException(
            string elementKey,
            string message,
            Exception? innerException = null
        ) : base(message, innerException) {
        ElementKey = elementKey;
    }
}

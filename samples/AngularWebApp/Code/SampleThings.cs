using System;

namespace AngularWebApp.Code
{
    [TypeScriptGenerator.Include]
    [Flags]
    public enum SampleThings
    {
        [TypeScriptGenerator.EnumLabel("Zero")]
        None = 0,
        FirstValue = 1,
        SecondValue = 1 << 1,
        ThirdValue = 1 << 2,
    }
}

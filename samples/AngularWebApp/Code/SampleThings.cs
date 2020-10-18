using System;

namespace AngularWebApp.Code
{
    [TypeScriptGenerator.Attributes.Include]
    [Flags]
    public enum SampleThings
    {
        [TypeScriptGenerator.Attributes.EnumLabel("Zero")]
        None = 0,
        FirstValue = 1,
        SecondValue = 1 << 1,
        ThirdValue = 1 << 2,
    }
}

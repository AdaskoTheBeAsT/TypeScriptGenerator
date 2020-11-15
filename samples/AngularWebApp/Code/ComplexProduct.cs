using System.Collections.Generic;

namespace AngularWebApp.Code
{
    [TypeScriptGenerator.Attributes.Include]
    public class ComplexProduct
        : Product
    {
        public List<Product> Products { get; } = new List<Product>();
    }
}

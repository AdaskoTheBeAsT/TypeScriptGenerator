namespace AngularWebApp.Code
{
    [TypeScriptGenerator.Attributes.Include]
    public class Product
    {
        public Product()
        {
            Material = new Material();
        }

        public int Id { get; set; }

        public string? Name { get; set; }

        public Material Material { get; set; }
    }
}

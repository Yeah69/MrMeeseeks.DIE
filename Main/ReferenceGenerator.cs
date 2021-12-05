namespace MrMeeseeks.DIE;

internal interface IReferenceGenerator
{
    string Generate(ITypeSymbol type);
    string Generate(string prefix, ITypeSymbol type);
    string Generate(string prefix, ITypeSymbol type, string suffix);
    string Generate(string hardcodedName);
}

internal class ReferenceGenerator : IReferenceGenerator
{
    private int _i = -1;
    private readonly int _j;

    internal ReferenceGenerator(int j) => _j = j;

    public string Generate(ITypeSymbol type) => 
        GenerateInner(string.Empty, $"{char.ToLower(type.Name[0])}{type.Name.Substring(1)}", string.Empty);

    public string Generate(string prefix, ITypeSymbol type) =>
        GenerateInner(prefix, type.Name, string.Empty);

    public string Generate(string prefix, ITypeSymbol type, string suffix) =>
        GenerateInner(prefix, type.Name, suffix);

    public string Generate(string hardcodedName) => 
        GenerateInner(string.Empty, hardcodedName, string.Empty);
    
    private string GenerateInner(string prefix, string inner, string suffix) => 
        $"{prefix}{inner}{suffix}_{_j}_{++_i}";
}
    
internal interface IReferenceGeneratorFactory
{
    IReferenceGenerator Create();
}

internal class ReferenceGeneratorFactory : IReferenceGeneratorFactory
{
    private readonly Func<int, IReferenceGenerator> _referenceGeneratorFactory;
    private int _j = -1;
        
    public ReferenceGeneratorFactory(Func<int, IReferenceGenerator> referenceGeneratorFactory) => _referenceGeneratorFactory = referenceGeneratorFactory;

    public IReferenceGenerator Create() => _referenceGeneratorFactory(++_j);
}
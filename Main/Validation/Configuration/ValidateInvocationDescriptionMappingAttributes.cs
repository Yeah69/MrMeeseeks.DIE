using MrMeeseeks.DIE.Logging;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Validation.Configuration;

internal interface IValidateInvocationDescriptionMappingAttributes
{
    void Validate(AttributeData invocationDescriptionMappingAttribute,
        ImmutableArray<AttributeData> typeDescriptionMappingAttributes,
        ImmutableArray<AttributeData> methodDescriptionMappingAttributes);
}

internal sealed class ValidateInvocationDescriptionMappingAttributes : IValidateInvocationDescriptionMappingAttributes
{
    private readonly ILocalDiagLogger _localDiagLogger;
    private readonly HashSet<string> _acceptedMemberNames = ["TargetType", "TargetMethod"];

    internal ValidateInvocationDescriptionMappingAttributes(
        ILocalDiagLogger localDiagLogger)
    {
        _localDiagLogger = localDiagLogger;
    }
    
    public void Validate(AttributeData invocationDescriptionMappingAttribute, 
        ImmutableArray<AttributeData> typeDescriptionMappingAttributes,
        ImmutableArray<AttributeData> methodDescriptionMappingAttributes)
    {
        if (invocationDescriptionMappingAttribute.ConstructorArguments.Length != 1
            || invocationDescriptionMappingAttribute.ConstructorArguments[0].Kind != TypedConstantKind.Type
            || invocationDescriptionMappingAttribute.ConstructorArguments[0].Value is not INamedTypeSymbol invocationType)
        {
            _localDiagLogger.Error(
                ErrorLogData.ValidationGeneral("Malformed description mapping attribute."),
                invocationDescriptionMappingAttribute.GetLocation());
            return;
        }
        
        if (invocationType.TypeKind != TypeKind.Interface)
        {
            _localDiagLogger.Error(
                ErrorLogData.ValidationDescriptionType(invocationType, "Has to be an interface."),
                invocationDescriptionMappingAttribute.GetLocation());
            return;
        }
        
        var members = invocationType.GetMembers();
        var invalidMembers = members.Where(m => !_acceptedMemberNames.Contains(m.Name)).ToImmutableArray();
        if (invalidMembers.Any())
        {
            _localDiagLogger.Error(
                ErrorLogData.ValidationDescriptionType(invocationType, $"Not accepted members: {string.Join(", ", invalidMembers.Select(m => m.Name))}"),
                invocationDescriptionMappingAttribute.GetLocation());
        }

        var typeDescriptionTypes = typeDescriptionMappingAttributes
            .Where(ad => ad.ConstructorArguments.Length == 1
                         && ad.ConstructorArguments[0].Kind == TypedConstantKind.Type
                         && ad.ConstructorArguments[0].Value is INamedTypeSymbol)
            .Select(ad => ad.ConstructorArguments[0].Value)
            .OfType<INamedTypeSymbol>()
            .ToImmutableHashSet(CustomSymbolEqualityComparer.Default);
        
        if (members.FirstOrDefault(m => m.Name == "TargetType") is not IPropertySymbol { GetMethod: not null, SetMethod: null } returnTypeProperty
            || !typeDescriptionTypes.Contains(returnTypeProperty.Type))
        {
            _localDiagLogger.Error(
                ErrorLogData.ValidationDescriptionType(invocationType, "Member \"TargetType\" has to be of a type description type."),
                invocationDescriptionMappingAttribute.GetLocation());
        }
        
        var methodDescriptionTypes = methodDescriptionMappingAttributes
            .Where(ad => ad.ConstructorArguments.Length == 1
                         && ad.ConstructorArguments[0].Kind == TypedConstantKind.Type
                         && ad.ConstructorArguments[0].Value is INamedTypeSymbol)
            .Select(ad => ad.ConstructorArguments[0].Value)
            .OfType<INamedTypeSymbol>()
            .ToImmutableHashSet(CustomSymbolEqualityComparer.Default);

        if (members.FirstOrDefault(m => m.Name == "TargetMethod") is not IPropertySymbol { GetMethod: not null, SetMethod: null } methodDescriptionProperty
            || !methodDescriptionTypes.Contains(methodDescriptionProperty.Type))
        {
            _localDiagLogger.Error(
                ErrorLogData.ValidationDescriptionType(invocationType, "Member \"TargetMethod\" has to be of a method description type."),
                invocationDescriptionMappingAttribute.GetLocation());
        }
    }
}
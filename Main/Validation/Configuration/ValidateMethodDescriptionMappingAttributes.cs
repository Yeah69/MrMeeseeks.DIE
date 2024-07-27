using MrMeeseeks.DIE.Logging;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Validation.Configuration;

internal interface IValidateMethodDescriptionMappingAttributes
{
    void Validate(AttributeData methodDescriptionMappingAttribute,
        ImmutableArray<AttributeData> typeDescriptionMappingAttributes);
}

internal sealed class ValidateMethodDescriptionMappingAttributes : IValidateMethodDescriptionMappingAttributes
{
    private readonly ILocalDiagLogger _localDiagLogger;
    private readonly HashSet<string> _acceptedMemberNames = ["Name", "ReturnType"];

    internal ValidateMethodDescriptionMappingAttributes(
        ILocalDiagLogger localDiagLogger)
    {
        _localDiagLogger = localDiagLogger;
    }
    
    public void Validate(AttributeData methodDescriptionMappingAttribute, ImmutableArray<AttributeData> typeDescriptionMappingAttributes)
    {
        if (methodDescriptionMappingAttribute.ConstructorArguments.Length != 1
            || methodDescriptionMappingAttribute.ConstructorArguments[0].Kind != TypedConstantKind.Type
            || methodDescriptionMappingAttribute.ConstructorArguments[0].Value is not INamedTypeSymbol descriptionType)
        {
            _localDiagLogger.Error(
                ErrorLogData.ValidationGeneral("Malformed description mapping attribute."),
                methodDescriptionMappingAttribute.GetLocation());
            return;
        }
        
        if (descriptionType.TypeKind != TypeKind.Interface)
        {
            _localDiagLogger.Error(
                ErrorLogData.ValidationDescriptionType(descriptionType, "Has to be an interface."),
                methodDescriptionMappingAttribute.GetLocation());
            return;
        }
        
        var members = descriptionType.GetMembers();
        var invalidMembers = members.Where(m => !_acceptedMemberNames.Contains(m.Name)).ToImmutableArray();
        if (invalidMembers.Any())
        {
            _localDiagLogger.Error(
                ErrorLogData.ValidationDescriptionType(descriptionType, $"Not accepted members: {string.Join(", ", invalidMembers.Select(m => m.Name))}"),
                methodDescriptionMappingAttribute.GetLocation());
        }
        
        if (members.FirstOrDefault(m => m.Name == "Name") is not IPropertySymbol { GetMethod: not null, SetMethod: null, Type.SpecialType: SpecialType.System_String })
        {
            _localDiagLogger.Error(
                ErrorLogData.ValidationDescriptionType(descriptionType, "Member \"Name\" has to be of type string."),
                methodDescriptionMappingAttribute.GetLocation());
        }

        var typeDescriptionTypes = typeDescriptionMappingAttributes
            .Where(ad => ad.ConstructorArguments.Length == 1
                         && ad.ConstructorArguments[0].Kind == TypedConstantKind.Type
                         && ad.ConstructorArguments[0].Value is INamedTypeSymbol)
            .Select(ad => ad.ConstructorArguments[0].Value)
            .OfType<INamedTypeSymbol>()
            .ToImmutableHashSet(CustomSymbolEqualityComparer.Default);
        
        if (members.FirstOrDefault(m => m.Name == "ReturnType") is not IPropertySymbol { GetMethod: not null, SetMethod: null } returnTypeProperty
            || !typeDescriptionTypes.Contains(returnTypeProperty.Type))
        {
            _localDiagLogger.Error(
                ErrorLogData.ValidationDescriptionType(descriptionType, "Member \"ReturnType\" has to be of a type description type."),
                methodDescriptionMappingAttribute.GetLocation());
        }
    }
}
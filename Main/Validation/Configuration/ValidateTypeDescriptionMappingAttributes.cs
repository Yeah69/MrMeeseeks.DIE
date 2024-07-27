using MrMeeseeks.DIE.Logging;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Validation.Configuration;

internal interface IValidateTypeDescriptionMappingAttributes
{
    void Validate(AttributeData typeDescriptionMappingAttribute);
}

internal sealed class ValidateTypeDescriptionMappingAttributes : IValidateTypeDescriptionMappingAttributes
{
    private readonly ILocalDiagLogger _localDiagLogger;
    private readonly HashSet<string> _acceptedMemberNames = ["FullName", "Name"];

    internal ValidateTypeDescriptionMappingAttributes(
        ILocalDiagLogger localDiagLogger)
    {
        _localDiagLogger = localDiagLogger;
    }
    
    public void Validate(AttributeData typeDescriptionMappingAttribute)
    {
        if (typeDescriptionMappingAttribute.ConstructorArguments.Length != 1
            || typeDescriptionMappingAttribute.ConstructorArguments[0].Kind != TypedConstantKind.Type
            || typeDescriptionMappingAttribute.ConstructorArguments[0].Value is not INamedTypeSymbol descriptionType)
        {
            _localDiagLogger.Error(
                ErrorLogData.ValidationGeneral("Malformed description mapping attribute."),
                typeDescriptionMappingAttribute.GetLocation());
            return;
        }
        
        if (descriptionType.TypeKind != TypeKind.Interface)
        {
            _localDiagLogger.Error(
                ErrorLogData.ValidationDescriptionType(descriptionType, "Has to be an interface."),
                typeDescriptionMappingAttribute.GetLocation());
            return;
        }
        
        var members = descriptionType.GetMembers();
        var invalidMembers = members.Where(m => !_acceptedMemberNames.Contains(m.Name)).ToImmutableArray();
        if (invalidMembers.Any())
        {
            _localDiagLogger.Error(
                ErrorLogData.ValidationDescriptionType(descriptionType, $"Not accepted members: {string.Join(", ", invalidMembers.Select(m => m.Name))}"),
                typeDescriptionMappingAttribute.GetLocation());
        }
        
        if (members.FirstOrDefault(m => m.Name == "FullName") is not IPropertySymbol { GetMethod: not null, SetMethod: null, Type.SpecialType: SpecialType.System_String })
        {
            _localDiagLogger.Error(
                ErrorLogData.ValidationDescriptionType(descriptionType, "Member \"FullName\" has to be of type string."),
                typeDescriptionMappingAttribute.GetLocation());
        }
        
        if (members.FirstOrDefault(m => m.Name == "Name") is not IPropertySymbol { GetMethod: not null, SetMethod: null, Type.SpecialType: SpecialType.System_String })
        {
            _localDiagLogger.Error(
                ErrorLogData.ValidationDescriptionType(descriptionType, "Member \"Name\" has to be of type string."),
                typeDescriptionMappingAttribute.GetLocation());
        }
    }
}
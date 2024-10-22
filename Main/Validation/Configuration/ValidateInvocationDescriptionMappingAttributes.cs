using MrMeeseeks.DIE.Logging;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Validation.Configuration;

internal interface IValidateInvocationDescriptionMappingAttributes
{
    void Validate(AttributeData invocationDescriptionMappingAttribute);
}

internal sealed class ValidateInvocationDescriptionMappingAttributes : IValidateInvocationDescriptionMappingAttributes
{
    private readonly ILocalDiagLogger _localDiagLogger;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly HashSet<string> _acceptedMemberNames = ["TargetType", "TargetMethod"];

    internal ValidateInvocationDescriptionMappingAttributes(
        ILocalDiagLogger localDiagLogger,
        WellKnownTypes wellKnownTypes)
    {
        _localDiagLogger = localDiagLogger;
        _wellKnownTypes = wellKnownTypes;
    }
    
    public void Validate(AttributeData invocationDescriptionMappingAttribute)
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
        
        if (members.FirstOrDefault(m => m.Name == "TargetType") is not IPropertySymbol { GetMethod: not null, SetMethod: null } returnTypeProperty
            || !CustomSymbolEqualityComparer.Default.Equals(returnTypeProperty.Type, _wellKnownTypes.Type))
        {
            _localDiagLogger.Error(
                ErrorLogData.ValidationDescriptionType(invocationType, $"Member \"TargetType\" has to be of the type \"{_wellKnownTypes.Type.FullName()}\"."),
                invocationDescriptionMappingAttribute.GetLocation());
        }

        if (members.FirstOrDefault(m => m.Name == "TargetMethod") is not IPropertySymbol { GetMethod: not null, SetMethod: null } methodDescriptionProperty
            || !CustomSymbolEqualityComparer.Default.Equals(methodDescriptionProperty.Type, _wellKnownTypes.MethodInfo))
        {
            _localDiagLogger.Error(
                ErrorLogData.ValidationDescriptionType(invocationType, $"Member \"TargetMethod\" has to be of the type \"{_wellKnownTypes.MethodInfo.FullName()}\"."),
                invocationDescriptionMappingAttribute.GetLocation());
        }
    }
}
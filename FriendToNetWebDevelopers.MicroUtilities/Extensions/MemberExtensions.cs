using System.Reflection;
using FriendToNetWebDevelopers.MicroUtilities.Enum;

namespace FriendToNetWebDevelopers.MicroUtilities.Extensions;

public static class MemberExtensions
{
    /// <summary>
    /// Gets a formatted variable name from a member's name.
    /// </summary>
    /// <param name="member">The member to format.</param>
    /// <param name="targetType">The target variable naming convention.</param>
    /// <returns>A string representing the member name in the requested format.</returns>
    public static string ToVariableName(this MemberInfo member, RequestedVariableNameTypeEnum targetType = RequestedVariableNameTypeEnum.CamelCase)
        => Utilities.Variable.GetVariableName(member, targetType);

    /// <summary>
    /// Gets a PascalCase formatted class name from a type.
    /// </summary>
    /// <param name="type">The type to format.</param>
    /// <returns>A PascalCase formatted string of the type name.</returns>
    public static string ToClassName(this Type type)
        => Utilities.Variable.GetClassName(type);
}
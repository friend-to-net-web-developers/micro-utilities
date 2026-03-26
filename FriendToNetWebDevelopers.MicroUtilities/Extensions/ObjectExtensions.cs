using FriendToNetWebDevelopers.MicroUtilities.Enum;

namespace FriendToNetWebDevelopers.MicroUtilities.Extensions;

public static class ObjectExtensions
{
    /// <summary>
    /// Gets a formatted variable name from an object instance's type.
    /// </summary>
    /// <param name="obj">The object whose type name will be formatted.</param>
    /// <param name="targetType">The target variable naming convention.</param>
    /// <returns>A string representing the instance's type name in the requested format.</returns>
    public static string ToVariableName(this object obj, RequestedVariableNameTypeEnum targetType = RequestedVariableNameTypeEnum.CamelCase)
        => Utilities.Variable.GetVariableName(obj, targetType);

    /// <summary>
    /// Gets a PascalCase formatted class name from an object instance's type.
    /// </summary>
    /// <param name="obj">The object whose type name will be formatted.</param>
    /// <returns>A PascalCase formatted string of the instance's type name.</returns>
    public static string ToClassName(this object obj)
        => Utilities.Variable.GetClassName(obj);
}
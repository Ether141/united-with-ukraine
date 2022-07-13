using System;

/// <summary>
/// Attribute which indicates that class is member of <seealso cref="GameManager"/>. Class which is marked with this attribute, must have property in <seealso cref="GameManager"/>, to which it can be assigned.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class GameManagerMemberAttribute : Attribute
{
    public GameManagerMemberAttribute() { }
}
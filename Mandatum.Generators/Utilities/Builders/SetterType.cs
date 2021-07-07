namespace Mandatum.Generators.Utilities.Builders
{
	public enum SetterType
	{
		Public,
		Private,
		Internal,
		Protected,
		None
	}

	public static class SetterTypeExtensions
	{
		public static string GetKeyword(this SetterType getterOrSetterType)
		{
			return getterOrSetterType switch
			{
				SetterType.Public => "set;",
				SetterType.Private => "private set;",
				SetterType.Internal => "internal set;",
				SetterType.Protected => "protected set;",
				SetterType.None => ""
			};
		}
	}
}
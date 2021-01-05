namespace Mandatum.Generators.Utilities.Builders
{
	public enum GetterType
	{
		Public,
		Private,
		Internal,
		Protected,
		None
	}

	public static class GetterOrSetterTypeExtensions
	{
		public static string GetKeyword(this GetterType getterOrSetterType)
		{
			return getterOrSetterType switch
			{
				GetterType.Public => "get;",
				GetterType.Private => "private get;",
				GetterType.Internal => "internal get;",
				GetterType.Protected => "protected get;",
				GetterType.None => ""
			};
		}
	}
}
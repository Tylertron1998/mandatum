using System;

namespace Mandatum.Generators.Utilities.Builders
{
	public enum Accessibility
	{
		Public,
		Private,
		Internal,
		Protected
	}

	public static class AccessibilityExtensions
	{
		public static string ToKeyword(this Accessibility accessibility)
		{
			return accessibility switch
			{
				Accessibility.Public => "public",
				Accessibility.Internal => "internal",
				Accessibility.Private => "private",
				Accessibility.Protected => "protected",
				_ => throw new ArgumentOutOfRangeException(nameof(accessibility), accessibility, "Unexpected accessibility type")
			};
		}
	}
}
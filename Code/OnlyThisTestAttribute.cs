using System;

namespace Kerosene.Tools
{
	// ====================================================
	/// <summary>
	/// When used with a test method only those marked with this attribute are executed by the
	/// launcher.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class OnlyThisTestAttribute : Attribute { }
}

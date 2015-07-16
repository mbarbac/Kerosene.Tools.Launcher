using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Kerosene.Tools
{
	// ====================================================
	/// <summary>
	/// Launcher for an arbitrary number of test methods gathered from the current program in
	/// execution.
	/// </summary>
	public class TestsLauncher
	{
		/// <summary>
		/// Executes all the test methods in the current assembly. If any method is marked with
		/// the 'OnlyThisTest' attribute, then only those methods marked with the same attribute
		/// are executed.
		/// </summary>
		public static void Execute()
		{
			var asm = Assembly.GetCallingAssembly();

			var types = asm
				.GetTypes()
				.Where(x => x.GetCustomAttributes<TestClassAttribute>().Count() != 0)
				.ToArray();

			var methods = new List<MethodInfo>();
			foreach (var type in types) methods.AddRange(LocateMethods(type));

			Executor(methods);
			methods.Clear();
		}

		/// <summary>
		/// Executes the test methods found on the given type. If any method is marked with the
		/// 'OnlyThisTest' attribute, then only those methods marked with the same attribute are
		/// executed.
		/// </summary>
		/// <typeparam name="T">The test class where to find the test methods to execute.</typeparam>
		public static void Execute<T>()
		{
			Execute(typeof(T));
		}

		/// <summary>
		/// Executes the test methods found on the given type. If any method is marked with the
		/// 'OnlyThisTest' attribute, then only those methods marked with the same attribute are
		/// executed.
		/// </summary>
		/// <param name="type">The test class where to find the test methods to execute.</param>
		public static void Execute(Type type)
		{
			var methods = LocateMethods(type);
			Executor(methods);
			methods.Clear();
		}

		/// <summary>
		/// Locates the valid test methods on the given type.
		/// </summary>
		static List<MethodInfo> LocateMethods(Type type)
		{
			if (type == null) throw new ArgumentNullException("type", "Type of test class cannot be null.");

			if (type.GetCustomAttributes<TestClassAttribute>().Count() == 0)
				throw new NotSupportedException(
					"Class '{0'} is not marked with the '{1}' attribute."
					.FormatWith(type.EasyName(), typeof(TestClassAttribute).Name));

			var methods = type.GetMethods()
				.Where(x => x.GetCustomAttributes<TestMethodAttribute>().Count() != 0)
				.ToList();

			return methods;
		}

		/// <summary>
		/// Executes the list of methods given.
		/// </summary>
		static void Executor(List<MethodInfo> methods)
		{
			// If any method is marked with 'OnlyThisTest' then execute ONLY those marked methods...
			var temp = methods.Where(x => x.GetCustomAttributes<OnlyThisTestAttribute>().Count() != 0).ToList();
			if (temp.Count != 0) methods = temp;

			// executing with an instance of each test class type...
			Dictionary<Type, object> instances = new Dictionary<Type, object>();
			foreach (var method in methods)
			{
				Type type = method.DeclaringType;
				object instance = null; if (!instances.TryGetValue(type, out instance))
				{
					var con = type.GetConstructor(Type.EmptyTypes);
					if (con == null) throw new NotFoundException(
						"Cannot find parameterless constructor for test class '{0}'."
						.FormatWith(type.EasyName()));

					instance = con.Invoke(null);
					instances.Add(type, instance);
				}

				try
				{
					ConsoleEx.WriteLine("\n====== {0}()", method.EasyName());
					ConsoleEx.ReadLine("Press [Enter] to execute... ");
					method.Invoke(instance, null);
				}
				catch (Exception e)
				{
					var inner = e is TargetInvocationException && e.InnerException != null;
					if (inner) e = e.InnerException;

					ConsoleEx.WriteLine("\n----- {0}() -----", method.EasyName());
					ConsoleEx.WriteLine(e.ToDisplayString());

					if (inner) throw e;
					else throw;
				}
			}

			foreach (var obj in instances.Values) if (obj is IDisposable) ((IDisposable)obj).Dispose();
			instances.Clear();
			temp.Clear();
		}
	}
}
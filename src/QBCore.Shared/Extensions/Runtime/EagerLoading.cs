using System.Reflection;
using System.Runtime.CompilerServices;

namespace QBCore.Extensions.Runtime;

/// <summary>
/// Marks an assembly that contains the eager loading constructors.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public sealed class EagerLoadingAssemblyAttribute : Attribute { }

/// <summary>
/// Static constructor eager loading behaviour attribute.
/// Force to run all eager static constructors in the assemblies marked by
/// <c>EagerLoadingAssemblyAttribute</c> on the assembly load.
/// </summary>
/// <remarks>
/// In a way, this attribute is similar to <c>ModuleInitializerAttribute</c>.
/// </remarks>
[AttributeUsage(AttributeTargets.Constructor, Inherited = false)]
public sealed class EagerLoadingAttribute : Attribute
{
	public int SortOrder { get; set; }

	public EagerLoadingAttribute(int sortOrder = 0) => SortOrder = sortOrder;
}

/// <summary>
/// Eager loading static constructors loader.
/// </summary>
/// <remarks>
/// <c>Initialize()</c> should be called at program start at the very beginning and <c>Stop()</c> at the end.
/// </remarks>
public static class EagerLoading
{
	/// <summary>
	/// Immediately invokes constructors for all already loaded assemblies and watches for new ones to be loaded with
	/// <c>AppDomain.CurrentDomain.AssemblyLoad</c> event.
	/// </summary>
	public static void Initialize()
	{
		AppDomain.CurrentDomain.AssemblyLoad += OnCurrentDomainAssemblyLoad;
		AppDomain.CurrentDomain.DomainUnload += OnCurrentDomainUnload;

		foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
		{
			RunEagerStaticConstructors(asm);
		}
	}

	/// <summary>
	/// Stops watching the loading of new assemblies.
	/// </summary>
	public static void Dispose()
	{
		AppDomain.CurrentDomain.AssemblyLoad -= OnCurrentDomainAssemblyLoad;
		AppDomain.CurrentDomain.DomainUnload -= OnCurrentDomainUnload;
	}

	private static void RunEagerStaticConstructors(Assembly asm)
	{
		if (asm.IsDefined(typeof(EagerLoadingAssemblyAttribute), false))
		{
			foreach (var staticCtor in GetEagerStaticConstructors(asm))
			{
				RuntimeHelpers.RunClassConstructor(staticCtor);
			}
		}
	}

	private static void OnCurrentDomainAssemblyLoad(object? sender, AssemblyLoadEventArgs args)
		=> RunEagerStaticConstructors(args.LoadedAssembly);
	
	private static void OnCurrentDomainUnload(object? sender, EventArgs args)
		=> Dispose();

	private static IEnumerable<RuntimeTypeHandle> GetEagerStaticConstructors(Assembly asm)
		=> asm.DefinedTypes
			.Where(type => type.DeclaredConstructors.Any(constructorInfo => constructorInfo.IsStatic))
			.SelectMany(x => x.GetConstructors(BindingFlags.Static | BindingFlags.NonPublic))
			.Select(x => (x.DeclaringType?.TypeHandle, x.GetCustomAttribute<EagerLoadingAttribute>(false)?.SortOrder))
			.Where(x => x.TypeHandle != null && x.SortOrder != null)
			.OrderBy(x => x.SortOrder)
			.Select(x => x.TypeHandle!.Value);
}
using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace InstantiationDemo
{
    class Program
    {
        static void Main(string[] args)
        {
			var instance = new MyClass("Oh look, an instance");
			Console.WriteLine(instance.Value);

			//****** Activator ******
			const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			var instance2 = Activator.CreateInstance(typeof(MyClass), bindingFlags, null, new object[] { "This is slooooow" }, CultureInfo.CurrentCulture) as MyClass;
			Console.WriteLine(instance2.Value);

			//****** Invoking the constructor ******
			var ctorInfo = typeof(MyClass).GetConstructor(new[] { typeof(string) }); // Get the constructor with the parameter of type 'string'
			var instance3 = (ctorInfo.Invoke(new[] { "We're invoking the constructor!" }) as MyClass);
			Console.WriteLine(instance3.Value);

			//****** Emit IL ******
			var dynamicMethod = new DynamicMethod(
							"MyClassCreator",
							typeof(MyClass), // The return type of the delegate
							new Type[] { typeof(string) }, // The parameter types of the delegate.
							typeof(Program).Module, // Where it will be defined
							false); // Whether or not to ignore access modifier (i.e. public/private etc)

			var il = dynamicMethod.GetILGenerator();

			//Create the code for our dynamic method.
			il.Emit(OpCodes.Ldarg_0); //Push parameter 0 of the method onto the stack
			il.Emit(OpCodes.Newobj, ctorInfo); //instantiate our new object using the constructor info
			il.Emit(OpCodes.Ret); //return the object at the top of the stack

			//delegates are references to methods
			var delegateFromEmittingIL = dynamicMethod.CreateDelegate(typeof(Func<string, MyClass>)) as Func<string, MyClass>;

			Console.WriteLine(delegateFromEmittingIL("Hey, we did this the hard way!").Value);

			//****** Compiled expression ******
			var parameter = Expression.Parameter(typeof(string));
			var expression = Expression.New(ctorInfo, parameter);

			var delegateFromCompiledExpression = Expression.Lambda<Func<string, MyClass>>(expression, parameter).Compile();

			Console.WriteLine(delegateFromCompiledExpression("This is much easier").Value);

			Console.WriteLine("\r\nPress any key to exit.");
			Console.Read();
		}
    }

	public class MyClass
	{
		public string Value { get; }

		public MyClass(string myParameter)
		{
			Value = myParameter;
		}
	}
}

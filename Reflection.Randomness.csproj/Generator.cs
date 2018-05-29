using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reflection.Randomness
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    class FromDistribution : Attribute
	{
		public Type TypeOfDistribution { get; set; }
		public IContinousDistribution Distribution { get; set; }
		public FromDistribution(Type typeOfDistribution, params object[] args)
		{
			this.TypeOfDistribution = typeOfDistribution;
			Distribution = (IContinousDistribution)Activator.CreateInstance(typeOfDistribution, args);
		}

		public double InitializeNewClass(Random random)
		{
			return Distribution.Generate(random);
		}
	}

	class Generator<T>
		where T : new()
	{
		public T Generate(Random random)
		{
			T result = new T();
			Type currentClassType = typeof(T);
			var props = currentClassType.GetProperties().Where(p => Attribute.IsDefined(p, typeof(FromDistribution)));
			foreach (var prop in props)
			{
				FromDistribution attribute = (FromDistribution)Attribute.GetCustomAttribute(prop, typeof(FromDistribution));
				var x = attribute.InitializeNewClass(random);
				prop.SetValue(result, x);
			}

			return result;
		}
	}
}

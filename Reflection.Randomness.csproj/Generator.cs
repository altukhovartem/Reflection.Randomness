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

		#region Props

		public Type TypeOfDistribution { get; set; }
		public int X { get; set; }
		public int Y { get; set; }
		public int Z { get; set; }

		#endregion

		#region Ctors

		public FromDistribution(Type typeOfDistribution)
		{
			this.TypeOfDistribution = typeOfDistribution;
		}

		public FromDistribution(Type typeOfDistribution, int X)
			: this(typeOfDistribution)
		{
			this.X = X;
		}

		public FromDistribution(Type typeOfDistribution, int X, int Y)
			: this(typeOfDistribution, X)
		{
			this.Y = Y;
		}

		public FromDistribution(Type typeOfDistribution, int X, int Y, int Z)
			: this(typeOfDistribution, X, Y)
		{
			this.Z = Z;
		}

		#endregion

		public double InitializeNewClass(Random random)
		{
			IContinousDistribution distr = Activator.CreateInstance(TypeOfDistribution) as IContinousDistribution;

			return distr.Generate(random);
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
				FromDistribution attribute = prop.GetCustomAttributes(typeof(FromDistribution), false).FirstOrDefault() as FromDistribution;
				var x = attribute.InitializeNewClass(random);
				prop.SetValue(result, x);
			}

			return result;
		}
	}
}

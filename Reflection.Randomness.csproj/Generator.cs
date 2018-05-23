using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reflection.Randomness
{
	[AttributeUsage(AttributeTargets.Property)]
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

	}

	class Generator<T1>
	{
		public void 
	}
}

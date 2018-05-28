﻿using System;
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

		public IContinousDistribution Distribution { get; set; }


		#endregion

		#region Ctors

		public FromDistribution(Type typeOfDistribution)
		{
			this.TypeOfDistribution = typeOfDistribution;
			Distribution = CreateInstance();
		}

		public FromDistribution(Type typeOfDistribution, int X)
		{
			this.TypeOfDistribution = TypeOfDistribution;
			this.X = X;
			Distribution = CreateInstance(X);
		}

		public FromDistribution(Type typeOfDistribution, int X, int Y)
		{
			this.TypeOfDistribution = TypeOfDistribution;
			this.X = X;
			this.Y = Y;
			Distribution = CreateInstance(X,Y);
		}

		public FromDistribution(Type typeOfDistribution, int X, int Y, int Z)
		{
			this.TypeOfDistribution = TypeOfDistribution;
			this.X = X;
			this.Y = Y;
			this.Z = Z;
			Distribution = CreateInstance(X,Y,Z);
		}

		#endregion

		private IContinousDistribution CreateInstance(params object[] parametrs)
		{
			
			return (IContinousDistribution)Activator.CreateInstance(TypeOfDistribution, parametrs);
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

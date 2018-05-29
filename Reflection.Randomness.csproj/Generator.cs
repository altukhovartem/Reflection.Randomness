using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Reflection.Randomness
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class FromDistribution : Attribute
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

	public class Generator<T> 
		where T : new()
	{
		public IContinousDistribution Distribution { get; set; }


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

		public ISettable<T> For(Expression<Func<T, object>> p)
		{
			// возвращает новый типа на котором можно вызвать фор. Нужен новый типа
			// после вызова сет у нас будет готовый класс генератор
			// 
			var expression = p.Body;
			var unaryExpression = (UnaryExpression)expression;

			var memberExpression = (MemberExpression)unaryExpression.Operand;
			var name = memberExpression.Member.Name;

			//operandName = name;

			return new TempObj<T>(name);  
		}
	}

	public interface ISettable<T>
		where T : new()
	{
		Generator<T> Set(IContinousDistribution distribution);
	}

	public class TempObj<T> : ISettable<T>
		where T : new()
	{
		public string PropName { get; set; }

		public TempObj(string propName)
		{
			this.PropName = propName;
		}

		public Generator<T> Set(IContinousDistribution distribution)
		{
			Generator<T> currentGenerator = new Generator<T>();
			Type typeOfDistribution = distribution.GetType();
			var x = typeOfDistribution.GetFields(); 


			return new Generator<T>();
		}
	}
}

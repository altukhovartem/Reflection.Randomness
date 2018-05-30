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
		where T : class
	{
		public IContinousDistribution Distribution { get; set; }


		public T Generate(Random random)
		{
			Type generatorType = typeof(T);
			object generatorInstance = Activator.CreateInstance(generatorType);
			var props = generatorType.GetProperties().Where(p => Attribute.IsDefined(p, typeof(FromDistribution)));
			foreach (var prop in props)
			{
				FromDistribution attribute = (FromDistribution)Attribute.GetCustomAttribute(prop, typeof(FromDistribution));
				var x = attribute.InitializeNewClass(random);
				prop.SetValue(generatorInstance, x);
			}

			return generatorInstance as T;
		}

		public ISettable<T> For(Expression<Func<T, object>> p)
		{
			var type = p.GetType();

			var expression = p.Body;
			var unaryExpression = (UnaryExpression)expression;

			var memberExpression = (MemberExpression)unaryExpression.Operand;
			var name = memberExpression.Member.Name;

			return new TempObj<T>(name);  
		}
	}

	public interface ISettable<T>
		where T : class
	{
		Generator<T> Set(IContinousDistribution distribution);
	}

	public class TempObj<T> : ISettable<T>
		where T : class
	{
		public string PropName { get; set; }

		public TempObj(string propName)
		{
			this.PropName = propName;
		}

		public Generator<T> Set(IContinousDistribution distribution)
		{
			//Generator<T> currentGenerator = new Generator<T>();
			//Type typeOfDistribution = distribution.GetType();
			//var x = typeOfDistribution.GetFields(); 

			var generatorInst = Activator.CreateInstance(typeof(T));
			PropertyInfo prop = typeof(T).GetProperty(PropName);
		

			return new Generator<T>();
		}
	}
}

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

		public double GetDistributionValue(Random random)
		{
			return Distribution.Generate(random);
		}
	}

	public class Generator<T>
		where T : class
	{
		static Dictionary<PropertyInfo, IContinousDistribution> staticDictionary = new Dictionary<PropertyInfo, IContinousDistribution>();
		public Dictionary<PropertyInfo, IContinousDistribution> dynamicDictionary = new Dictionary<PropertyInfo, IContinousDistribution>();

		public Generator(){}

		static Generator()
		{
			PropertyInfo[] collectionOfProperties = typeof(T).GetProperties();
			foreach (var prop in collectionOfProperties)
			{
				FromDistribution attribute = Attribute.GetCustomAttribute(prop, typeof(FromDistribution)) as FromDistribution;
				staticDictionary.Add(prop, attribute?.Distribution);
			}
		}

		public T Generate(Random random)
		{
			object generatorInstance = Activator.CreateInstance(typeof(T));
			PropertyInfo[] props = typeof(T).GetProperties();
			double valueOfDistribution;

			foreach (var prop in props)
			{
				if (dynamicDictionary.ContainsKey(prop) && dynamicDictionary[prop] != null )
				{
					valueOfDistribution = dynamicDictionary[prop].Generate(random);
					prop.SetValue(generatorInstance, valueOfDistribution);

				}
				else if (staticDictionary[prop] != null)
				{
					valueOfDistribution = staticDictionary[prop].Generate(random);
					prop.SetValue(generatorInstance, valueOfDistribution);
				}
			}

			return generatorInstance as T;
		}
	
		public ISettable<T> For(Expression<Func<T, object>> p)
		{
			Type type = p.GetType();
			Expression currentExpression = p.Body;
			UnaryExpression unaryExpression = (UnaryExpression)currentExpression;
			MemberExpression memberExpression = (MemberExpression)unaryExpression.Operand;
			string name = memberExpression.Member.Name;
			PropertyInfo prop = typeof(T).GetProperty(name);

			return new TempObj<T>(prop, this);  
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
		public PropertyInfo classProperty { get; set; }
		public Generator<T> currentGenerator { get; set; }

		public TempObj(PropertyInfo property, Generator<T> generator)
		{
			this.classProperty = property;
			this.currentGenerator = generator;
		}

		public Generator<T> Set(IContinousDistribution distribution)
		{
			currentGenerator.dynamicDictionary.Add(classProperty, distribution);
			return currentGenerator;
		}
	}
}

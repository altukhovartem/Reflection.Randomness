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
		private Type _distrType;
		private object[] _args;

		public FromDistribution(Type typeOfDistribution, params object[] args)
		{
			_distrType = typeOfDistribution;
			_args = args;
		}

		public IContinousDistribution Create()
		{
			try
			{
				return (IContinousDistribution)Activator.CreateInstance(_distrType, _args);
			}
			catch
			{
				throw new ArgumentException(_distrType.Name);
			}
		}
	}

	public class Generator<T>
	{
		public static Dictionary<PropertyInfo, Lazy<IContinousDistribution>> staticDictionary = new Dictionary<PropertyInfo, Lazy<IContinousDistribution>>();		
        public Dictionary<PropertyInfo, IContinousDistribution> dynamicDictionary = new Dictionary<PropertyInfo, IContinousDistribution>();

        public Generator(){}

		static Generator()
		{
			PropertyInfo[] collectionOfProperties = typeof(T).GetProperties();
			foreach (var prop in collectionOfProperties)
			{
				FromDistribution attribute = Attribute.GetCustomAttribute(prop, typeof(FromDistribution)) as FromDistribution;
				if (attribute != null)
				{
					Lazy<IContinousDistribution> lazyDistribution = new Lazy<IContinousDistribution>(() => attribute.Create());
					staticDictionary.Add(prop, lazyDistribution);
				}
				else
				{
					staticDictionary.Add(prop, null);
				}
			}
		}

		public T Generate(Random random)
		{
			object generatorInstance = Activator.CreateInstance(typeof(T));
			double valueOfDistribution = 0;

			foreach (var key in staticDictionary.Keys)
			{
				if (dynamicDictionary.ContainsKey(key))
				{
					valueOfDistribution = dynamicDictionary[key].Generate(random);
					key.SetValue(generatorInstance, valueOfDistribution);
				}
				else if (staticDictionary[key] != null)
				{
					valueOfDistribution = staticDictionary[key].Value.Generate(random);
					key.SetValue(generatorInstance, valueOfDistribution);
				}
			}

			return (T)generatorInstance;
		}


	
		public ISettable<T> For(Expression<Func<T, object>> p)
		{
			try
			{
				Type type = p.GetType();
				Expression currentExpression = p.Body;
				UnaryExpression unaryExpression = (UnaryExpression)currentExpression;
				MemberExpression memberExpression = (MemberExpression)unaryExpression.Operand;
				string name = memberExpression.Member.Name;
				PropertyInfo prop = typeof(T).GetProperty(name);
				if (prop == null)
					throw new ArgumentException();
				return new TempObj<T>(prop, this);  
			}
			catch
			{
				throw new ArgumentException();
			}
		}
	}

	public interface ISettable<T>
		
	{
		Generator<T> Set(IContinousDistribution distribution);
	}

	public class TempObj<T> : ISettable<T>
		
	{
		private readonly PropertyInfo _classProperty;
		private readonly Generator<T> _currentGenerator;

		public TempObj(PropertyInfo property, Generator<T> generator)
		{
			_classProperty = property;
			_currentGenerator = generator;
		}

		public Generator<T> Set(IContinousDistribution distribution)
		{
			_currentGenerator.dynamicDictionary.Add(_classProperty, distribution);
			return _currentGenerator;
		}
	}
}

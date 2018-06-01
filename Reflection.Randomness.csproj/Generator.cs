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
		public int MyProperty { get; set; }

		static Dictionary<PropertyInfo, IContinousDistribution> staticDictionary = new Dictionary<PropertyInfo, IContinousDistribution>();
		public Dictionary<PropertyInfo, IContinousDistribution> dynamicDictionary = new Dictionary<PropertyInfo, IContinousDistribution>();


		static Generator()
		{
			Type typeOfCurrentClass = typeof(T);
			PropertyInfo[] collectionOfProperties = typeOfCurrentClass.GetProperties();
			foreach (var prop in collectionOfProperties)
			{
				FromDistribution attribute = (FromDistribution)Attribute.GetCustomAttribute(prop, typeof(FromDistribution));
				staticDictionary.Add(prop, attribute.Distribution);
			}
		}

		public Generator()
		{

		}


		public T Generate(Random random)
		{
			Type generatorType = typeof(T);
			object generatorInstance = Activator.CreateInstance(generatorType);
			var props = generatorType.GetProperties().Where(p => Attribute.IsDefined(p, typeof(FromDistribution)));
			foreach (var prop in props)
			{
				FromDistribution attribute = (FromDistribution)Attribute.GetCustomAttribute(prop, typeof(FromDistribution));
				var x = attribute.GetDistributionValue(random);
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
			// передать все свойство
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
			//у тебя есть два способа сообщить генератору о том какие распределения проассоциированы с какими полями
			//1. либо непосредственно на само поле повесить атрибут
			//2. либо лично генератору подсказать

			//1: первый способ хорош если этот класс на поле которого ты вешаешь в твоем распоряжении и ты можешь на его поля повесить атрибут
			//2: второй способ хорош когда ты не можешь повесить на поле атрибут, потому что это не твой класс, а класс из какой то другой библиотеки
			

			return new Generator<T>();
		}
	}
}

using System;
using System.Linq.Expressions;

namespace FxCommonStandard.Extensions
{
	public static class ExpressionExtensions
	{
		// ReSharper disable once UnusedParameter.Global
		public static string GetMemberName<T, TProperty>(this T source, Expression<Func<T, TProperty>> expression)
		{
			return GetMemberName(expression);
		}

		public static string GetMemberName<T, TProperty>(Expression<Func<T, TProperty>> expression)
		{
			var memberExpression = expression.Body as MemberExpression;

			return memberExpression?.Member.Name;
		}
	}
}
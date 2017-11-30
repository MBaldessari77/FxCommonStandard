using System;
using System.Collections.Generic;
using System.Linq;

namespace FxCommon.Extensions
{
	public static class EnumerableExtensions
	{
		public static T Next<T>(this IEnumerable<T> source, T current)
		{
			return source.SkipWhile(item => !Equals(item, current)).Skip(1).First();
		}

		public static T Next<T>(this IEnumerable<T> source, T current, Func<T, bool> predicate)
		{
			return source.SkipWhile(item => !Equals(item, current)).Skip(1).First(predicate);
		}

		public static T Previous<T>(this IEnumerable<T> source, T current)
		{
			return source.TakeWhile(item => !Equals(item, current)).Last();
		}

		public static T Previous<T>(this IEnumerable<T> source, T current, Func<T, bool> predicate)
		{
			return source.TakeWhile(item => !Equals(item, current)).Last(predicate);
		}

		public static T NextOrDefault<T>(this IEnumerable<T> source, T current)
		{
			return source.SkipWhile(item => !Equals(item, current)).Skip(1).FirstOrDefault();
		}

		public static T NextOrDefault<T>(this IEnumerable<T> source, T current, Func<T, bool> predicate)
		{
			return source.SkipWhile(item => !Equals(item, current)).Skip(1).FirstOrDefault(predicate);
		}

		public static T PreviousOrDefault<T>(this IEnumerable<T> source, T current)
		{
			return source.TakeWhile(item => !Equals(item, current)).LastOrDefault();
		}

		public static T PreviousOrDefault<T>(this IEnumerable<T> source, T current, Func<T, bool> predicate)
		{
			return source.TakeWhile(item => !Equals(item, current)).LastOrDefault(predicate);
		}

		public static IEnumerable<T> ToEnumerable<T>(this T item)
		{
			yield return item;
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using FxCommonStandard.Extensions;
using Xunit;

namespace FxCommonStandard.Tests.Extensions
{
	public class EnumerableExtensionsTest
	{
		[Fact]
		public void WhenSourceIsNullArgumentNullExceptionIsThrown()
		{
			IEnumerable<object> enumerable = null;

			Assert.Throws<ArgumentNullException>(() => enumerable.Next(null));
			Assert.Throws<ArgumentNullException>(() => enumerable.Next(null, null));
			Assert.Throws<ArgumentNullException>(() => enumerable.Previous(null));
			Assert.Throws<ArgumentNullException>(() => enumerable.Previous(null, null));
			Assert.Throws<ArgumentNullException>(() => enumerable.NextOrDefault(null));
			Assert.Throws<ArgumentNullException>(() => enumerable.NextOrDefault(null, null));
			Assert.Throws<ArgumentNullException>(() => enumerable.PreviousOrDefault(null));
			Assert.Throws<ArgumentNullException>(() => enumerable.PreviousOrDefault(null, null));
		}

		[Fact]
		public void WhenPredicateIsNullArgumentNullExceptionIsThrown()
		{
			// ReSharper disable once CollectionNeverUpdated.Local
			var list = new List<object>();

			Assert.Throws<ArgumentNullException>(() => list.Next(null, null));
			Assert.Throws<ArgumentNullException>(() => list.Previous(null, null));
			Assert.Throws<ArgumentNullException>(() => list.NextOrDefault(null, null));
			Assert.Throws<ArgumentNullException>(() => list.PreviousOrDefault(null, null));
		}

		[Fact]
		public void WhenCurrentElementDoesNotExistsInTheListInvalidOperationExceptionIsThrown()
		{
			// ReSharper disable once CollectionNeverUpdated.Local
			var list = new List<object>();

			Assert.Throws<InvalidOperationException>(() => list.Next(null));
			Assert.Throws<InvalidOperationException>(() => list.Next(null, item => true));
			Assert.Throws<InvalidOperationException>(() => list.Previous(null));
			Assert.Throws<InvalidOperationException>(() => list.Previous(null, item => true));
		}

		[Fact]
		public void WhenCurrentElementDoesNotExistsInTheListDefualtValueIsReturned()
		{
			// ReSharper disable once CollectionNeverUpdated.Local
			var list = new List<object>();

			Assert.Null(list.NextOrDefault(null));
			Assert.Null(list.NextOrDefault(null, item => true));
			Assert.Null(list.PreviousOrDefault(null));
			Assert.Null(list.PreviousOrDefault(null, item => true));
		}

		[Fact]
		public void FirstFromCurrentInValueTypeListReturnNextCorrectElement()
		{
			var numbers = new List<int> {1, 2, 3, 4};


			Assert.Equal(3, numbers.Next(2));
			Assert.Equal(4, numbers.Next(2, item => item > 3));
			Assert.Equal(3, numbers.NextOrDefault(2));
			Assert.Equal(4, numbers.NextOrDefault(2, item => item > 3));
			Assert.Equal(default(int), numbers.NextOrDefault(4));
			Assert.Equal(default(int), numbers.NextOrDefault(2, item => item > 4));
		}

		[Fact]
		public void FirstFromCurrentInReferenceTypeListReturnNextCorrectElement()
		{
			var objects = new List<object>();

			var objA = new object();
			var objB = new object();
			var objC = new object();
			var objD = new object();

			objects.Add(objA);
			objects.Add(objB);
			objects.Add(objC);
			objects.Add(objD);

			Assert.Equal(objC, objects.Next(objB));
			Assert.Equal(objD, objects.Next(objB, item => item != objC));
			Assert.Equal(objC, objects.NextOrDefault(objB));
			Assert.Equal(objD, objects.NextOrDefault(objB, item => item != objC));
			Assert.Null(objects.NextOrDefault(objD));
			Assert.Null(objects.NextOrDefault(objB, item => item != objC && item != objD));
		}

		[Fact]
		public void LastFromCurrentInValueTypeListReturnPreviousCorrectElement()
		{
			var numbers = new List<int> {1, 2, 3, 4};


			Assert.Equal(2, numbers.Previous(3));
			Assert.Equal(1, numbers.Previous(3, item => item < 2));
			Assert.Equal(2, numbers.PreviousOrDefault(3));
			Assert.Equal(1, numbers.PreviousOrDefault(3, item => item < 2));
			Assert.Equal(default(int), numbers.PreviousOrDefault(1));
			Assert.Equal(default(int), numbers.PreviousOrDefault(3, item => item < 1));
		}

		[Fact]
		public void LastFromCurrentInReferenceTypeListReturnPreviousCorrectElement()
		{
			var objects = new List<object>();

			var objA = new object();
			var objB = new object();
			var objC = new object();
			var objD = new object();

			objects.Add(objA);
			objects.Add(objB);
			objects.Add(objC);
			objects.Add(objD);

			Assert.Equal(objB, objects.Previous(objC));
			Assert.Equal(objA, objects.Previous(objC, item => item != objB));
			Assert.Equal(objB, objects.PreviousOrDefault(objC));
			Assert.Equal(objA, objects.PreviousOrDefault(objC, item => item != objB));
			Assert.Null(objects.PreviousOrDefault(objA));
			Assert.Null(objects.PreviousOrDefault(objC, item => item != objB && item != objA));
		}

		[Fact]
		public void ToEnumerableFromObjectReturnEnumerableWithOneElementEqualsToObject()
		{
			var obj = new object();

			IEnumerable<object> enumerable = obj.ToEnumerable().ToList();

			Assert.NotNull(enumerable);
			Assert.Single(enumerable);
			Assert.Equal(obj, enumerable.First());
		}

		[Fact]
		public void ToEnumerableFromNullObjectReferenceReturnEnumerableWithOneElementNull()
		{
			IEnumerable<object> enumerable = ((object) null).ToEnumerable().ToList();

			Assert.NotNull(enumerable);
			Assert.Single(enumerable);
			Assert.Null(enumerable.First());
		}

		[Fact]
		public void ToEnumerableFromValueTypeReturnEnumerableWithOneElementEqualsToValue()
		{
			int value = int.MaxValue;

			IEnumerable<int> enumerable = value.ToEnumerable().ToList();

			Assert.NotNull(enumerable);
			Assert.Single(enumerable);
			Assert.Equal(value, enumerable.First());
		}
	}
}
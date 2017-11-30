using System;
using System.Collections.Generic;
using System.Linq;
using FxCommon.Extensions;
using NUnit.Framework;

namespace FxCommonStandard.Tests
{
	[TestFixture]
	public class EnumerableExtensionsTest
	{
		[Test]
		public void WhenSourceIsNullArgumentNullExceptionIsThrown()
		{
			IEnumerable<object> enumerable = null;

			Assert.Catch<ArgumentNullException>(() => enumerable.Next(null));
			Assert.Catch<ArgumentNullException>(() => enumerable.Next(null, null));
			Assert.Catch<ArgumentNullException>(() => enumerable.Previous(null));
			Assert.Catch<ArgumentNullException>(() => enumerable.Previous(null, null));
			Assert.Catch<ArgumentNullException>(() => enumerable.NextOrDefault(null));
			Assert.Catch<ArgumentNullException>(() => enumerable.NextOrDefault(null, null));
			Assert.Catch<ArgumentNullException>(() => enumerable.PreviousOrDefault(null));
			Assert.Catch<ArgumentNullException>(() => enumerable.PreviousOrDefault(null, null));
		}

		[Test]
		public void WhenPredicateIsNullArgumentNullExceptionIsThrown()
		{
			var list = new List<object>();

			Assert.Catch<ArgumentNullException>(() => list.Next(null, null));
			Assert.Catch<ArgumentNullException>(() => list.Previous(null, null));
			Assert.Catch<ArgumentNullException>(() => list.NextOrDefault(null, null));
			Assert.Catch<ArgumentNullException>(() => list.PreviousOrDefault(null, null));
		}

		[Test]
		public void WhenCurrentElementDoesNotExistsInTheListInvalidOperationExceptionIsThrown()
		{
			var list = new List<object>();

			Assert.Catch<InvalidOperationException>(() => list.Next(null));
			Assert.Catch<InvalidOperationException>(() => list.Next(null, item => true));
			Assert.Catch<InvalidOperationException>(() => list.Previous(null));
			Assert.Catch<InvalidOperationException>(() => list.Previous(null, item => true));
		}

		[Test]
		public void WhenCurrentElementDoesNotExistsInTheListDefualtValueIsReturned()
		{
			var list = new List<object>();

			Assert.IsNull(list.NextOrDefault(null));
			Assert.IsNull(list.NextOrDefault(null, item => true));
			Assert.IsNull(list.PreviousOrDefault(null));
			Assert.IsNull(list.PreviousOrDefault(null, item => true));
		}

		[Test]
		public void FirstFromCurrentInValueTypeListReturnNextCorrectElement()
		{
			var numbers = new List<int>();

			numbers.Add(1);
			numbers.Add(2);
			numbers.Add(3);
			numbers.Add(4);

			Assert.AreEqual(3, numbers.Next(2));
			Assert.AreEqual(4, numbers.Next(2, item => item > 3));
			Assert.AreEqual(3, numbers.NextOrDefault(2));
			Assert.AreEqual(4, numbers.NextOrDefault(2, item => item > 3));
			Assert.AreEqual(default(int), numbers.NextOrDefault(4));
			Assert.AreEqual(default(int), numbers.NextOrDefault(2, item => item > 4));
		}

		[Test]
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

			Assert.AreEqual(objC, objects.Next(objB));
			Assert.AreEqual(objD, objects.Next(objB, item => item != objC));
			Assert.AreEqual(objC, objects.NextOrDefault(objB));
			Assert.AreEqual(objD, objects.NextOrDefault(objB, item => item != objC));
			Assert.IsNull(objects.NextOrDefault(objD));
			Assert.IsNull(objects.NextOrDefault(objB, item => item != objC && item != objD));
		}

		[Test]
		public void LastFromCurrentInValueTypeListReturnPreviousCorrectElement()
		{
			var numbers = new List<int>();

			numbers.Add(1);
			numbers.Add(2);
			numbers.Add(3);
			numbers.Add(4);

			Assert.AreEqual(2, numbers.Previous(3));
			Assert.AreEqual(1, numbers.Previous(3, item => item < 2));
			Assert.AreEqual(2, numbers.PreviousOrDefault(3));
			Assert.AreEqual(1, numbers.PreviousOrDefault(3, item => item < 2));
			Assert.AreEqual(default(int), numbers.PreviousOrDefault(1));
			Assert.AreEqual(default(int), numbers.PreviousOrDefault(3, item => item < 1));
		}

		[Test]
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

			Assert.AreEqual(objB, objects.Previous(objC));
			Assert.AreEqual(objA, objects.Previous(objC, item => item != objB));
			Assert.AreEqual(objB, objects.PreviousOrDefault(objC));
			Assert.AreEqual(objA, objects.PreviousOrDefault(objC, item => item != objB));
			Assert.IsNull(objects.PreviousOrDefault(objA));
			Assert.IsNull(objects.PreviousOrDefault(objC, item => item != objB && item != objA));
		}

		[Test]
		public void ToEnumerableFromObjectReturnEnumerableWithOneElementEqualsToObject()
		{
			var obj = new object();

			IEnumerable<object> enumerable = obj.ToEnumerable();

			Assert.IsNotNull(enumerable);
			Assert.AreEqual(1, enumerable.Count());
			Assert.AreEqual(obj, enumerable.First());
		}

		[Test]
		public void ToEnumerableFromNullObjectReferenceReturnEnumerableWithOneElementNull()
		{
			IEnumerable<object> enumerable = ((object) null).ToEnumerable();

			Assert.IsNotNull(enumerable);
			Assert.AreEqual(1, enumerable.Count());
			Assert.IsNull(enumerable.First());
		}

		[Test]
		public void ToEnumerableFromValueTypeReturnEnumerableWithOneElementEqualsToValue()
		{
			int value = int.MaxValue;

			IEnumerable<int> enumerable = value.ToEnumerable();

			Assert.IsNotNull(enumerable);
			Assert.AreEqual(1, enumerable.Count());
			Assert.AreEqual(value, enumerable.First());
		}
	}
}
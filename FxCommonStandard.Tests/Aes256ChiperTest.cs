using System;
using System.Linq;
using FxCommon.Services;
using NUnit.Framework;

namespace FxCommonStandard.Tests
{
	[TestFixture]
	public class Aes256ChiperTest
	{
		[Test]
		public void EveryCryptographicResultsIsDifferentEvenInCaseOfSamePassawordAndValue()
		{
			const string yes = "YES";
			const string no = "NO ";
			Guid pwd1 = Guid.NewGuid(), pwd2 = Guid.NewGuid();
			string[] values = {yes, yes, no, no, yes};

			Aes256Chiper chiper = new Aes256Chiper();

			string[] encriptedValues = values.Select(v => chiper.EncriptBase64(pwd1, pwd2, v)).ToArray();
			string[] decriptedValues = encriptedValues.Select(v => chiper.DecriptBase64(pwd1, pwd2, v)).ToArray();

			Assert.IsEmpty(encriptedValues.Intersect(values));
			Assert.IsTrue(decriptedValues.SequenceEqual(values));
			Assert.That(encriptedValues[0], Is.Not.EqualTo(encriptedValues[1]));
			Assert.That(encriptedValues[0], Is.Not.EqualTo(encriptedValues[2]));
			Assert.That(encriptedValues[0], Is.Not.EqualTo(encriptedValues[4]));
			Assert.That(encriptedValues[2], Is.Not.EqualTo(encriptedValues[3]));

#if DEBUG
			for (int i = 0; i < values.Length; i++)
				Console.WriteLine($"{values[i]} => {encriptedValues[i]}");
#endif
		}
	}
}
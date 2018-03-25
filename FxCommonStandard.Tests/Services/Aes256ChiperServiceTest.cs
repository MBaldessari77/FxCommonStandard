using System;
using System.Linq;
using FxCommonStandard.Services;
using Xunit;

namespace FxCommonStandard.Tests.Services
{
	public class Aes256ChiperServiceTest
	{
		[Fact]
		public void EveryCryptographicResultsIsDifferentEvenInCaseOfSamePassawordAndValue()
		{
			const string yes = "YES";
			const string no = "NO ";
			Guid pwd1 = Guid.NewGuid(), pwd2 = Guid.NewGuid();
			string[] values = {yes, yes, no, no, yes};

			Aes256ChiperService chiperService = new Aes256ChiperService();

			string[] encriptedValues = values.Select(v => chiperService.EncriptBase64(pwd1, pwd2, v)).ToArray();
			string[] decriptedValues = encriptedValues.Select(v => chiperService.DecriptBase64(pwd1, pwd2, v)).ToArray();

			Assert.Empty(encriptedValues.Intersect(values));
			Assert.True(decriptedValues.SequenceEqual(values));
			Assert.NotEqual(encriptedValues[0], encriptedValues[1]);
			Assert.NotEqual(encriptedValues[0], encriptedValues[2]);
			Assert.NotEqual(encriptedValues[0], encriptedValues[4]);
			Assert.NotEqual(encriptedValues[2], encriptedValues[3]);

#if DEBUG
			for (int i = 0; i < values.Length; i++)
				Console.WriteLine($"{values[i]} => {encriptedValues[i]}");
#endif
		}
	}
}
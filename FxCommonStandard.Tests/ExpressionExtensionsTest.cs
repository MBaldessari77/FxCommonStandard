using FxCommon.Extensions;
using NUnit.Framework;

namespace FxCommonStandard.Tests
{
	[TestFixture]
	public class ExpressionExtensionsTest
	{
		class MemberSourceStub
		{
			public string Property { get; set; }
#pragma warning disable 649
			public int Field;
#pragma warning restore 649
		}

		[Test]
		public void GetMemberNameFromExpressionsSucceed()
		{
			Assert.AreEqual("Property", ExpressionExtensions.GetMemberName((MemberSourceStub t) => t.Property));
			Assert.AreEqual("Field", ExpressionExtensions.GetMemberName((MemberSourceStub t) => t.Field));
		}

		[Test]
		public void GetMemberNameFromExpressionsViaObjectSucceed()
		{
			MemberSourceStub t = new MemberSourceStub();

			Assert.AreEqual("Property", t.GetMemberName(e => e.Property));
			Assert.AreEqual("Field", t.GetMemberName(e => e.Field));
		}
	}
}
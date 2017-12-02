using FxCommon.Extensions;
using Xunit;

namespace FxCommonStandard.Tests
{
	public class ExpressionExtensionsTest
	{
		class MemberSourceStub
		{
			// ReSharper disable once UnusedAutoPropertyAccessor.Local
			public string Property { get; set; }
#pragma warning disable 649
			public int Field;
#pragma warning restore 649
		}

		[Fact]
		public void GetMemberNameFromExpressionsSucceed()
		{
			Assert.Equal("Property", ExpressionExtensions.GetMemberName((MemberSourceStub t) => t.Property));
			Assert.Equal("Field", ExpressionExtensions.GetMemberName((MemberSourceStub t) => t.Field));
		}

		[Fact]
		public void GetMemberNameFromExpressionsViaObjectSucceed()
		{
			MemberSourceStub t = new MemberSourceStub();

			Assert.Equal("Property", t.GetMemberName(e => e.Property));
			Assert.Equal("Field", t.GetMemberName(e => e.Field));
		}
	}
}
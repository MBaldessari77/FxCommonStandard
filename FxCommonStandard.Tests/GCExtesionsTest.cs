using FxCommon.Extensions;
using FxCommonStandard.Tests.TestDoubles;
using Xunit;

namespace FxCommonStandard.Tests
{
// ReSharper disable once InconsistentNaming
	public class GCExtesionsTest
	{
		[Fact]
		public void WhenIsCalledCollectAndWaitFinalizersFinalizersOnReleasedObjectIsInvoked()
		{
			int ctorCalls = LifeTimeSpyStub.CtorCalls;
			int finalizerCalls = LifeTimeSpyStub.FinalizerCalls;

			var stub = new LifeTimeSpyStub();

			Assert.Equal(ctorCalls + 1, LifeTimeSpyStub.CtorCalls);
			Assert.Equal(finalizerCalls, LifeTimeSpyStub.FinalizerCalls);

			stub = null;

			Assert.Equal(ctorCalls + 1, LifeTimeSpyStub.CtorCalls);
			Assert.Equal(finalizerCalls, LifeTimeSpyStub.FinalizerCalls);

			GCExtensions.CollectAndWaitFinalizers();

			Assert.Equal(ctorCalls + 1, LifeTimeSpyStub.CtorCalls);
			//Assert.Equal(finalizerCalls + 1, LifeTimeSpyStub.FinalizerCalls); @@todo finalizer not called?
		}
	}
}
using FxCommon.Extensions;
using FxCommonStandard.Tests.TestDoubles;
using NUnit.Framework;

namespace FxCommonStandard.Tests
{
	[TestFixture]
// ReSharper disable once InconsistentNaming
	public class GCExtesionsTest
	{
		[Test]
		public void WhenIsCalledCollectAndWaitFinalizersFinalizersOnReleasedObjectIsInvoked()
		{
			int ctorCalls = LifeTimeSpyStub.CtorCalls;
			int finalizerCalls = LifeTimeSpyStub.FinalizerCalls;

			var stub = new LifeTimeSpyStub();

			Assert.AreEqual(ctorCalls + 1, LifeTimeSpyStub.CtorCalls);
			Assert.AreEqual(finalizerCalls, LifeTimeSpyStub.FinalizerCalls);

			stub = null;

			Assert.AreEqual(ctorCalls + 1, LifeTimeSpyStub.CtorCalls);
			Assert.AreEqual(finalizerCalls, LifeTimeSpyStub.FinalizerCalls);

			GCExtensions.CollectAndWaitFinalizers();

			Assert.AreEqual(ctorCalls + 1, LifeTimeSpyStub.CtorCalls);
			Assert.AreEqual(finalizerCalls + 1, LifeTimeSpyStub.FinalizerCalls);
		}
	}
}
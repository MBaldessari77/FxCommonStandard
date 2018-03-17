using FxCommonStandard.Services;
using Xunit;

namespace FxCommonStandard.Tests
{
	public class EventSourceServicesTest
	{
		[Fact]
		public void WhenAnEventIsRaisedTheListenerReceiveTheEvent()
		{
			int raisedCount=0;
			var service = new EventSourceService();

			service.RegisterListener((sender, args) => { raisedCount++; });
			service.RegisterEvent();

			Assert.Equal(1, raisedCount);
		}

		[Fact]
		public void WhenAParticularEventIsRaisedTheListenerRegisterToThisEventReceiveTheEvent()
		{
			int raisedCount=0;
			var service = new EventSourceService();

			service.RegisterListener((sender, args) => { raisedCount++; }, "ParticularEvent");
			service.RegisterListener((sender, args) => { raisedCount++; }, "AnotherEvent");
			service.RegisterEvent("ParticularEvent");

			Assert.Equal(1, raisedCount);
		}
	}
}
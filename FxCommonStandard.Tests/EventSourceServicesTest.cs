using System;
using FxCommonStandard.Services;
using Xunit;

namespace FxCommonStandard.Tests
{
	public class EventSourceServicesTest
	{
		[Fact]
		public void WhenAnEventIsRaisedTheListenerReceiveTheEvent()
		{
			int raisedCount = 0;
			var service = new EventSourceService();

			service.SubscribeEvent((sender, args) => { raisedCount++; });
			service.AddEvent();

			Assert.Equal(1, raisedCount);
		}

		[Fact]
		public void WhenAParticularEventIsRaisedTheListenerRegisterToThisEventReceiveTheEvent()
		{
			int raisedCount = 0;
			var service = new EventSourceService();

			service.SubscribeEvent((sender, args) => { raisedCount++; }, new CustomEventArgs());
			service.SubscribeEvent((sender, args) => { raisedCount++; });
			service.AddEvent(new CustomEventArgs());

			Assert.Equal(1, raisedCount);
		}

		class CustomEventArgs : EventArgs
		{
			public override bool Equals(object obj) { return obj != null && obj.GetType().IsAssignableFrom(GetType()); }
			public override int GetHashCode() { return GetType().FullName.GetHashCode(); }
		}
	}
}
using System;
using System.Threading;
using FxCommonStandard.Services;
using Xunit;

namespace FxCommonStandard.Tests
{
	public class EventSourceServicesTest
	{
		[Fact]
		public async void WhenAnEventIsRaisedTheListenerReceiveTheEvent()
		{
			int raisedCount = 0;
			var service = new EventSourceService();

			service.SubscribeEvent((sender, args) => { raisedCount++; });

			service.AddEvent();
			await service.EventsProcessedAsync();

			Assert.Equal(1, raisedCount);
		}

		[Fact]
		public async void WhenAParticularEventIsRaisedTheListenerRegisterToThisEventReceiveTheEvent()
		{
			int raisedCount = 0;
			var service = new EventSourceService();

			service.SubscribeEvent((sender, args) => { raisedCount++; }, new CustomEventArgs());
			service.SubscribeEvent((sender, args) => { raisedCount++; });
			
			service.AddEvent(new CustomEventArgs());
			await service.EventsProcessedAsync();

			Assert.Equal(1, raisedCount);
		}

		[Fact]
		public async void AStallOnListenerOnEventCantBlockOtherListener()
		{
			int raisedCount = 0;
			var service = new EventSourceService();

			service.SubscribeEvent((sender, args) => { Thread.Sleep(int.MaxValue); });
			service.SubscribeEvent((sender, args) => { raisedCount++; });

			service.AddEvent();
			await service.EventsProcessedAsync(100);

			Assert.Equal(1, raisedCount);
		}

		class CustomEventArgs : EventArgs
		{
			public override bool Equals(object obj) { return obj != null && obj.GetType().IsAssignableFrom(GetType()); }
			public override int GetHashCode() { return GetType().FullName.GetHashCode(); }
		}
	}
}
using System;
using System.Threading;
using System.Threading.Tasks;
using FxCommonStandard.Contracts;
using FxCommonStandard.Services;
using Moq;
using Xunit;

namespace FxCommonStandard.Tests.Services
{
	public class EventSourcingServicesTest
	{
		readonly IUnitOfWorkFactory<EventArgs> _unitOfWorkFactoryMock;

		public EventSourcingServicesTest()
		{
			var mock = new Mock<IUnitOfWorkFactory<EventArgs>>();
			mock.Setup(f => f.New()).Returns(() => new Mock<IUnitOfWork<EventArgs>>().Object);
			_unitOfWorkFactoryMock = mock.Object;
		}

		[Fact]
		public void WhenAnEventIsRaisedTheListenerReceiveTheEvent()
		{
			int raisedCount = 0;

			using (var service = new EventSourcingService(_unitOfWorkFactoryMock))
			{
				service.SubscribeEvent((sender, args) => { raisedCount++; });

				service.AddEvent();

				service.WaitEventsProcessed();
			}

			Assert.Equal(1, raisedCount);
		}

		[Fact]
		public void WhenAParticularEventIsRaisedTheListenerRegisterToThisEventReceiveTheEvent()
		{
			int raisedCount = 0;

			using (var service = new EventSourcingService(_unitOfWorkFactoryMock))
			{
				service.SubscribeEvent((sender, args) => { raisedCount++; }, new CustomEventArgs());
				service.SubscribeEvent((sender, args) => { raisedCount++; });

				service.AddEvent(new CustomEventArgs());

				service.WaitEventsProcessed();
			}

			Assert.Equal(1, raisedCount);
		}

		[Fact]
		public void WhenAnEventIsRaisedAllTheListenerReceiveTheEvent()
		{
			int raisedCount1 = 0;
			int raisedCount2 = 0;

			using (var service = new EventSourcingService(_unitOfWorkFactoryMock))
			{
				service.SubscribeEvent((sender, args) => { raisedCount1++; });
				service.SubscribeEvent((sender, args) => { raisedCount2++; });

				service.AddEvent();

				service.WaitEventsProcessed();
			}

			Assert.Equal(1, raisedCount1);
			Assert.Equal(1, raisedCount2);
		}

		[Fact]
		public void AStallOnAListenerOnEventCantBlockOtherListener()
		{
			int raisedCount = 0;
			using (var service = new EventSourcingService(_unitOfWorkFactoryMock))
			{
				service.SubscribeEvent((sender, args) => { Thread.Sleep(int.MaxValue); });
				service.SubscribeEvent((sender, args) => { raisedCount++; });

				service.AddEvent();

				service.WaitEventsProcessed(100);
			}

			Assert.Equal(1, raisedCount);
		}

		[Fact]
		public void TwoServiceSubriscribeAnEventReceivedEventOnTheirListener()
		{
			int raisedCount1 = 0;
			int raisedCount2 = 0;

			using (var service = new EventSourcingService(_unitOfWorkFactoryMock))
			{
				service.SubscribeEvent((sender, args) => ++raisedCount1);
				service.SubscribeEvent((sender, args) => ++raisedCount2);

				var th1 = new Thread(() =>
				  {
					  // ReSharper disable once AccessToDisposedClosure
					  service.WaitEventsProcessed();
				  });

				var th2 = new Thread(() =>
				  {
					  // ReSharper disable once AccessToDisposedClosure
					  service.WaitEventsProcessed();
				  });

				service.AddEvent();

				th1.Start();
				th2.Start();

				th1.Join();
				th2.Join();
			}

			Assert.Equal(1, raisedCount1);
			Assert.Equal(1, raisedCount2);
		}

		[Fact]
		public void AnAddedEventIsPersistedAsNewInAnUnitOfWork()
		{
			var factoryMock = new Mock<IUnitOfWorkFactory<EventArgs>>();
			var unitOfWorkMock = new Mock<IUnitOfWork<EventArgs>>();
			factoryMock.Setup(f => f.New()).Returns(unitOfWorkMock.Object);

			var e = new EventArgs();

			using (var service = new EventSourcingService(factoryMock.Object))
				service.AddEvent(e);

			factoryMock.Verify(f=>f.New());
			unitOfWorkMock.Verify(u => u.New(e));
			unitOfWorkMock.Verify(u => u.Commit());
			unitOfWorkMock.Verify(u => u.Dispose());
		}

		[Fact]
		public void IfAnEventSubscriberThrowAnExceptionTheEventMustBeUpdateTheProcessingQueueRequest()
		{
			using (var service = new EventSourcingService(_unitOfWorkFactoryMock))
			{
				service.SubscribeEvent((sender, args) => throw new NotImplementedException());

				service.AddEvent();

				service.WaitEventsProcessed();
			}
		}

		[Fact]
		public void TheEventRaisedToSusbscriberIsExactlyTheSameAddedToEventSourcing()
		{
			using (var service = new EventSourcingService(_unitOfWorkFactoryMock))
			{
				var @event = new CustomEventArgs();
				EventArgs receivedEvent = null;

				service.SubscribeEvent((sender, args) => { receivedEvent = args; }, new CustomEventArgs());

				service.AddEvent(@event);

				service.WaitEventsProcessed();

				Assert.Same(receivedEvent, @event);
			}
		}

		[Fact]
		public async Task TheListenerCanBeAlsoAsync()
		{
			int raisedCount = 0;

			using (var service = new EventSourcingService(_unitOfWorkFactoryMock))
			{
				service.SubscribeEvent(async (sender, args) => await Task.FromResult(raisedCount++));

				service.AddEvent();

				await service.WaitEventsProcessedAsync();
			}

			Assert.Equal(1, raisedCount);
		}

		class CustomEventArgs : EventArgs
		{
			public override bool Equals(object obj) { return obj.GetType() == GetType(); }
			public override int GetHashCode() { return GetType().FullName.GetHashCode(); }
		}
	}
}
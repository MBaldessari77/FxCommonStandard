using System;
using System.Collections.Generic;
using System.Threading;
using FxCommonStandard.Contracts;
using FxCommonStandard.Services;
using Moq;
using Xunit;

namespace FxCommonStandard.Tests
{
	public class SagaPatternOrderSmokeTest
	{
		[Fact]
		public void TryOrderApprovedSuccessfullySaga()
		{
			var eventSourcingService = new EventSourcingService(new Mock<IUnitOfWork<EventArgs>>().Object);
			var orderService = new OrderService(eventSourcingService);
			var customerService = new CustomerService(eventSourcingService);

			orderService.CreateOrder("SolventCustomer", 100m);

			while (eventSourcingService.ProcessingEvents > 0)
				Thread.Sleep(0);

			Assert.True(orderService.IsApproved("SolventCustomer"));
		}

		class OrderService
		{
			readonly Dictionary<string, decimal> _orders = new Dictionary<string, decimal>();
			readonly Dictionary<string, bool> _ordersApproved = new Dictionary<string, bool>();
			readonly EventSourcingService _eventSourcingService;

			public OrderService(EventSourcingService eventSourcingService)
			{
				_eventSourcingService = eventSourcingService;
				_eventSourcingService.SubscribeEvent(CreditReserved, CreditReservedEventArgs.Empty);
			}

			public void CreateOrder(string customer, decimal import)
			{
				_orders.Add(customer, import);
				_eventSourcingService.AddEvent(new OrderCreatedEventArgs(customer, import));
			}

			public bool IsApproved(string customer) { return _ordersApproved.TryGetValue(customer, out bool value) && value; }

			void CreditReserved(object sender, EventArgs e) { throw new NotImplementedException(); }
		}

		class CustomerService
		{
			readonly EventSourcingService _eventSourcingService;

			public CustomerService(EventSourcingService eventSourcingService)
			{
				_eventSourcingService = eventSourcingService;
				_eventSourcingService.SubscribeEvent(OrderCreated, OrderCreatedEventArgs.Empty);
			}

			void OrderCreated(object sender, EventArgs e) { throw new NotImplementedException(); }
		}

		class OrderCreatedEventArgs : EventArgs
		{
			public new static readonly OrderCreatedEventArgs Empty = new OrderCreatedEventArgs();

			OrderCreatedEventArgs() { }

			public OrderCreatedEventArgs(string customer, decimal import)
			{
				Customer = customer;
				Import = import;
			}

			public string Customer { get; }
			public decimal Import { get; }

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != this.GetType()) return false;
				return Equals((OrderCreatedEventArgs) obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return ((Customer != null ? Customer.GetHashCode() : 0) * 397) ^ Import.GetHashCode();
				}
			}

			protected bool Equals(OrderCreatedEventArgs other)
			{
				return other.GetType()==GetType();;
			}
		}

		class CreditReservedEventArgs : EventArgs
		{
			public new static readonly CreditReservedEventArgs Empty = new CreditReservedEventArgs();

			protected bool Equals(CreditReservedEventArgs other) { return other.GetType()==GetType(); }

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != GetType()) return false;
				return Equals((CreditReservedEventArgs) obj);
			}

			public override int GetHashCode() { return 0; }
		}

		class CreditLimitExceededEventArgs : EventArgs
		{
			public new static readonly CreditLimitExceededEventArgs Empty = new CreditLimitExceededEventArgs();

			protected bool Equals(CreditLimitExceededEventArgs other) { return other.GetType()==GetType(); }

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != GetType()) return false;
				return Equals((CreditLimitExceededEventArgs) obj);
			}

			public override int GetHashCode() { return 0; }

		}
	}
}
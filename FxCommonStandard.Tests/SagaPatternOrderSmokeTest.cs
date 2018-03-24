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
		const string SolventCustomer = "SolventCustomer";

		[Fact]
		public void TryOrderApprovedSuccessfullySaga()
		{
			using (var eventSourcingService = new EventSourcingService(new Mock<IUnitOfWork<EventArgs>>().Object))
			{
				var orderService = new OrderService(eventSourcingService);
				var customerService = new CustomerService(eventSourcingService);

				var customerCredit = customerService.GetCredit(SolventCustomer);
				orderService.CreateOrder(SolventCustomer, 100m);

				while (eventSourcingService.ProcessingEvents > 0)
					Thread.Sleep(0);

				Assert.True(orderService.IsApproved(SolventCustomer));
				Assert.Equal(customerCredit - 100m, customerService.GetCredit(SolventCustomer));
			}
		}

		class OrderService
		{
			readonly Dictionary<string, decimal> _orders = new Dictionary<string, decimal>();
			readonly Dictionary<string, bool> _approvedOrders = new Dictionary<string, bool>();
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

			public bool IsApproved(string customer) { return _approvedOrders.TryGetValue(customer, out bool value) && value; }

			void CreditReserved(object sender, EventArgs e)
			{
				if (!(e is CreditReservedEventArgs creditReserved))
					return;

				_approvedOrders[creditReserved.Customer] = true;
			}
		}

		class CustomerService
		{
			readonly Dictionary<string, decimal> _customers = new Dictionary<string, decimal>();
			readonly EventSourcingService _eventSourcingService;

			public CustomerService(EventSourcingService eventSourcingService)
			{
				_eventSourcingService = eventSourcingService;
				_eventSourcingService.SubscribeEvent(OrderCreated, OrderCreatedEventArgs.Empty);
				_customers.Add(SolventCustomer, 1000m);
			}

			public decimal GetCredit(string customer) { return _customers.TryGetValue(customer, out var credit) ? credit : 0m; }

			void OrderCreated(object sender, EventArgs e)
			{
				if (!(e is OrderCreatedEventArgs orderCreated))
					return;

				if (_customers.TryGetValue(orderCreated.Customer, out var credit))
				{
					if (credit >= orderCreated.Import)
					{
						_customers[orderCreated.Customer] = credit - orderCreated.Import;
						_eventSourcingService.AddEvent(new CreditReservedEventArgs(orderCreated.Customer));
					}
					else
						_eventSourcingService.AddEvent(new CreditLimitExceededEventArgs(orderCreated.Customer));
				}
			}
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

			public override bool Equals(object obj) { return obj.GetType() == GetType(); }
			public override int GetHashCode() { return GetType().FullName.GetHashCode(); }
		}

		class CreditReservedEventArgs : EventArgs
		{
			public new static readonly CreditReservedEventArgs Empty = new CreditReservedEventArgs();

			CreditReservedEventArgs() { }
			public CreditReservedEventArgs(string customer) { Customer = customer; }

			public string Customer { get; }

			public override bool Equals(object obj) { return obj.GetType() == GetType(); }
			public override int GetHashCode() { return GetType().FullName.GetHashCode(); }
		}

		class CreditLimitExceededEventArgs : EventArgs
		{
			public new static readonly CreditLimitExceededEventArgs Empty = new CreditLimitExceededEventArgs();

			CreditLimitExceededEventArgs() { }
			public CreditLimitExceededEventArgs(string customer) { Customer = customer; }

			public string Customer { get; }

			public override bool Equals(object obj) { return obj.GetType() == GetType(); }
			public override int GetHashCode() { return GetType().FullName.GetHashCode(); }
		}
	}
}
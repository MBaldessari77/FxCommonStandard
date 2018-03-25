using System;
using System.Collections.Generic;
using FxCommonStandard.Contracts;
using FxCommonStandard.Services;
using Moq;
using Xunit;

namespace FxCommonStandard.Tests
{
	public class SagaPatternOrderSmokeTest
	{
		const string SolventCustomer = "SolventCustomer";
		const string InsolventCustomer = "InsolventCustomer";

		[Fact]
		public void TryOrderApprovedSuccessfully()
		{
			using (var eventSourcingService = new EventSourcingService(new Mock<IUnitOfWork<EventArgs>>().Object))
			{
				var orderService = new OrderService(eventSourcingService);
				var customerService = new CustomerService(eventSourcingService);
				var customerCredit = customerService.GetCredit(SolventCustomer);
				bool? approved = null;

				orderService.OrderChekouted += (sender, args) => approved = args.Customer == SolventCustomer && args.Result;
				orderService.CreateOrder(SolventCustomer, 100m);

				eventSourcingService.WaitEventsProcessed();

				Assert.True(approved);
				Assert.Equal(customerCredit - 100m, customerService.GetCredit(SolventCustomer));
			}
		}

		[Fact]
		public void TryOrderNotApprovedSuccessfully()
		{
			using (var eventSourcingService = new EventSourcingService(new Mock<IUnitOfWork<EventArgs>>().Object))
			{
				var orderService = new OrderService(eventSourcingService);
				var customerService = new CustomerService(eventSourcingService);
				var customerCredit = customerService.GetCredit(InsolventCustomer);
				bool? approved = null;

				orderService.OrderChekouted += (sender, args) => approved = args.Customer == InsolventCustomer && args.Result;
				orderService.CreateOrder(InsolventCustomer, 100m);

				eventSourcingService.WaitEventsProcessed();

				Assert.False(approved);
				Assert.True(customerCredit < 100m);
				Assert.Equal(customerCredit, customerService.GetCredit(InsolventCustomer));
			}
		}

		class OrderService
		{
			public event EventHandler<OrderCheckoutedEventArgs> OrderChekouted;

			readonly Dictionary<string, decimal> _orders = new Dictionary<string, decimal>();
			readonly EventSourcingService _eventSourcingService;

			public OrderService(EventSourcingService eventSourcingService)
			{
				_eventSourcingService = eventSourcingService;
				_eventSourcingService.SubscribeEvent(CustomerCreditRervationResult, CreditReservedEventArgs.Empty);
				_eventSourcingService.SubscribeEvent(CustomerCreditRervationResult, CreditLimitExceededEventArgs.Empty);
			}

			public void CreateOrder(string customer, decimal import)
			{
				_orders.Add(customer, import);
				_eventSourcingService.AddEvent(new OrderCreatedEventArgs(customer, import));
			}

			void CustomerCreditRervationResult(object sender, EventArgs e)
			{
				if (e is CreditReservedEventArgs creditReserved)
					OnOrderChekouted(new OrderCheckoutedEventArgs(creditReserved.Customer, true));

				if (e is CreditLimitExceededEventArgs creditLimitExceeded)
					OnOrderChekouted(new OrderCheckoutedEventArgs(creditLimitExceeded.Customer, false));
			}

			void OnOrderChekouted(OrderCheckoutedEventArgs e) { OrderChekouted?.Invoke(this, e); }
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
				_customers.Add(InsolventCustomer, 50m);
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

		class OrderCheckoutedEventArgs : EventArgs
		{
			public OrderCheckoutedEventArgs(string customer, bool result)
			{
				Customer = customer;
				Result = result;
			}

			public string Customer { get; }
			public bool Result { get; }
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
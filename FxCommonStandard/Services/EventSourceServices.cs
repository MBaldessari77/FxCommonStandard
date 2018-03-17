using System;

namespace FxCommonStandard.Services
{
	public class EventSourceService
	{
		event EventHandler EventHappened;

		public void SubscribeEvent(EventHandler @delegate, EventArgs expected = null) { EventHappened += (sender, e) => RaiseEvent(@delegate, e, expected); }
		public void AddEvent(EventArgs e = null) { OnEventHappened(e); }

		void OnEventHappened(EventArgs e) { EventHappened?.Invoke(this, e); }

		void RaiseEvent(EventHandler @delegate, EventArgs e, EventArgs expected)
		{
			if (Equals(expected, e))
				@delegate(this, e);
		}
	}
}
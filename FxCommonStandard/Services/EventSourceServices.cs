using System;

namespace FxCommonStandard.Services
{
	public class EventSourceService
	{
		event EventHandler<string> EventHappened;

		public void RegisterListener(EventHandler<string> @delegate, string key = null) { EventHappened += (sender, e) => RaiseEvent(sender, e, key, @delegate); }
		public void RegisterEvent(string key = null) { OnEventHappened(key); }

		void OnEventHappened(string key) { EventHappened?.Invoke(this, key); }

		void RaiseEvent(object sender, string key, string expectedKey, EventHandler<string> @delegate)
		{
			if (Equals(expectedKey, key))
				@delegate(sender, key);
		}
	}
}
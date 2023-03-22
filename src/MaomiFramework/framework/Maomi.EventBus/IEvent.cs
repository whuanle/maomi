using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maomi.EventBus
{
	// 事件接口，通过事件传递参数
	public interface IEvent
	{
		// 事件唯一标识
		Guid GetEventId();

		void SetEventId(Guid eventId);

		DateTime GetCreationTime();

		void SetCreationTime(DateTime creationTime);
	}
}

using IGYM.Model.UserModule.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Interface
{
	public interface IEmailNotification
	{
		Task<NotificationResult> SendEmail(NotificationRequest notificationRequest);
	}
}

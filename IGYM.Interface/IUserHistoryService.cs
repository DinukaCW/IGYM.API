﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Interface
{
	public interface IUserHistoryService
	{
		Task LogUserActionAsync(int userId, string actionType, string entityType, string endpoint, string ipAddress);
	}
}

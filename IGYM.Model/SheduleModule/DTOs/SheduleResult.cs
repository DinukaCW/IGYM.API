﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.SheduleModule.DTOs
{
	public class SheduleResult
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public int? ScheduleId { get; set; }
	}
}

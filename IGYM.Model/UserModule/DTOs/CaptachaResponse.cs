﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.UserModule.DTOs
{
	public class CaptachaResponse
	{
		public bool Success { get; set; }
		public string Challenge_ts { get; set; }
		public string Hostname { get; set; }
		public IEnumerable<string> ErrorCodes { get; set; }
	}
}

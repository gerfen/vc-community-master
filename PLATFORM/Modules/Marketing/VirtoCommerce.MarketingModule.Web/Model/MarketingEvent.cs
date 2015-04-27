﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VirtoCommerce.MarketingModule.Web.Model
{
	public class MarketingEvent
	{
		public string EventType { get; set; }
		public Dictionary<string, string> EventParams { get; set; }
	}
}
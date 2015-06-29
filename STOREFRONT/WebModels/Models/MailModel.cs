﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtoCommerce.Web.Models
{
    public class MailModel
    {
        public string FullName { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string MailBody { get; set; }
        public string FullMailBody { get; set; }
    }
}

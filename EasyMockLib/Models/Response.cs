﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EasyMockLib.Models
{
    public class Response
    {
        public Body ResponseBody { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public int Delayms { get; set; }
    }
}

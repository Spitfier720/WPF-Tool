using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMockLib.Models
{
    public class Request
    {
        public Request()
        {
            RequestBody = new Body();
        }
        public Body RequestBody { get; set; }
    }
}

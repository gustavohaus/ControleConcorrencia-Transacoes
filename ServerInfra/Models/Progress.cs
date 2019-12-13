using System;
using System.Collections.Generic;
using System.Text;

namespace ServerInfra.Models
{
    public class Progress
    {
        public Guid GuidProgress { get; set; }

        public Progress(Guid progress)
        {
            GuidProgress = progress;
        }
    }


}

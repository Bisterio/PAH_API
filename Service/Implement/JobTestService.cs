﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement {
    public class JobTestService : IJobTestService {
        public void FireAndForgetJob() {
            Console.WriteLine("Hello from a Fire and Forget job!");
        }
        public void ReccuringJob() {
            Console.WriteLine("Hello from a Scheduled job!");
        }
        public void DelayedJob() {
            Console.WriteLine("Hello from a Delayed job!");
        }
        public void ContinuationJob() {
            Console.WriteLine("Hello from a Continuation job!");
        }
    }
}

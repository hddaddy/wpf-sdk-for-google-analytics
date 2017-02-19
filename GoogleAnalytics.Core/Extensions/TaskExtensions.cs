using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Threading.Tasks
{
    public static class TaskExtensions
    {
        public static Task WhenAll(this Task task, IEnumerable<Task> tasks)
        {
            return Task.Factory.ContinueWhenAll(tasks.ToArray(), _ => { });
        }
    }
}

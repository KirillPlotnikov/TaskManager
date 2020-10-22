using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManager.Models
{
    public class TaskTag
    {
        public int TaskId { get; set; }
        public int TagId { get; set; }

        public Task Task { get; set; }
        public Tag Tag { get; set; }
    }
}

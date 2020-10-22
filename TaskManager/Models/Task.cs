using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManager.Models
{
    public class Task : Entity
    {
        [Required, StringLength(200)]
        public string Note { get; set; }
        [Required, Display(Name = "Has to be done at")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MM-yyyy HH:mm}")]
        public DateTime HasToBeDoneTime { get; set; }

        public int? CategoryId { get; set; }
        public ICollection<Tag> Tags { get; set; }
        public Category Category { get; set; }
    }
}

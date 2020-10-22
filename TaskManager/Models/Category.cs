using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManager.Models
{
    public class Category : Entity
    {
        [Required]
        [RegularExpression(@"^#[a-zA-Z0-9]{6}$", ErrorMessage = "Wrong format")]
        public string Color { get; set; }

        public int? CategoryOrder { get; set; }
    }
}

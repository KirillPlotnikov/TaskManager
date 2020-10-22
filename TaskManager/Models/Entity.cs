using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManager.Models
{
    public class Entity
    {
        public int Id { get; set; }
        [Required, StringLength(30, ErrorMessage = "Title cannot be longer than 30 characters", MinimumLength = 2)]
        public string Title { get; set; }
        [Required, Display(Name = "Created at")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MM-yyyy HH:mm}")]
        public DateTime CreationTime { get; set; }
        [Required, Display(Name = "Last edited at")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MM-yyyy HH:mm}")]
        public DateTime LastEditedTime { get; set; }
        
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public IdentityUser User { get; set; } 
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoffeeShop.Models
{
    public class Coffee
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(25, MinimumLength = 2)]
        public string Title { get; set; }

        [Required]
        [StringLength(25, MinimumLength = 2)]
        public string BeanType { get; set; }
    }
}

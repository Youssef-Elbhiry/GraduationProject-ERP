using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal SalesPrice { get; set; }
        public decimal CostPrice { get; set; }
        public bool IsActive { get; set; } = true;
        public string ImageUrl { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;


        [NotMapped]
        public decimal NetPrice => SalesPrice - CostPrice;

    }
}

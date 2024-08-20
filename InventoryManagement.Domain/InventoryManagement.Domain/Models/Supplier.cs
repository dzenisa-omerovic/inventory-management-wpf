using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Domain.Models
{
    public abstract class Supplier
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public virtual string DisplayName
        {
            get { return string.Empty; }
        }
    }
}

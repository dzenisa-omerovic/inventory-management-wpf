using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Domain.Models
{
    public class PersonSupplier : Supplier
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public override string DisplayName
        {
            get { return $"{FirstName} {LastName}"; }
        }
    }
}

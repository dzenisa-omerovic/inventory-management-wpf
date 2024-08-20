using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Domain.Models
{
    public class CompanySupplier : Supplier
    {
        public string Name { get; set; }
        public override string DisplayName
        {
            get { return Name; }
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GearHawk.Core.Models
{
    public sealed class Category
    {
        private int id;
        private string name;
        private int companyId;

        public int Id { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
        public int CompanyId { get => companyId; set => companyId = value; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModel
{
    public class CategoryModel
    {
        public int CategoryId_PK;
        public string Category;
        public bool? Isactive;
        public bool? IsDeleted;
        public int ?CreatedBy;
        public DateTime? CreatedDate;
        public int? ModifiedBy;
        public DateTime? modifiedDate;
    }
}

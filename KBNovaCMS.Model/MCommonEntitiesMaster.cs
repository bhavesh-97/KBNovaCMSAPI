using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KBNovaCMS.Model
{
    public class MCommonEntitiesMaster
    {
        #region Common-Entities
        public bool IsActive { get; set; } = false;
        public int CreatedBy { get; set; } = 0;
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
        public int UpdatedBy { get; set; } = 0;
        public DateTime? UpdatedDate { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public int DeletedBy { get; set; } = 0;
        public DateTime? DeletedDate { get; set; } = DateTime.UtcNow;
        #endregion
    }
}

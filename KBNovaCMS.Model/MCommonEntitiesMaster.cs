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
        public bool isActive { get; set; } = false;
        public int createdBy { get; set; } = 0;
        public DateTime? createdDate { get; set; } = DateTime.UtcNow;
        public int updatedBy { get; set; } = 0;
        public DateTime? updatedDate { get; set; } = DateTime.UtcNow;
        public bool isDeleted { get; set; } = false;
        public int deletedBy { get; set; } = 0;
        public DateTime? deletedDate { get; set; } = DateTime.UtcNow;
        #endregion
    }
}

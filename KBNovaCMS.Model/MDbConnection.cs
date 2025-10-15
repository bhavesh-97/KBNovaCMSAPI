using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KBNovaCMS.Model
{
    public class MDbConnection
    {
        public static string ConnectionString { get; set; } = string.Empty;
        public static bool IsDB_Postgresql { get; set; } = false;
        public static bool IsDevlopment { get; set; } = false;
        public static bool IsProduction { get; set; } = false;
    }
}

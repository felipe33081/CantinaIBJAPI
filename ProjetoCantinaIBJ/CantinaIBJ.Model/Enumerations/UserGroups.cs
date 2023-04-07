using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CantinaIBJ.Model.Enumerations
{
    public enum UserGroups
    {
        [Description("admin")]
        Admin,

        [Description("user")]
        User
    }
}

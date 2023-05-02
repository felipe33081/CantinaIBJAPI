using System.ComponentModel;

namespace CantinaIBJ.Model.Enumerations
{
    public enum UserGroups
    {
        /// <summary>
        /// Administrador
        /// </summary>
        [Description("admin")]
        Admin = 0,

        /// <summary>
        /// Usuário
        /// </summary>
        [Description("user")]
        User = 1
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_Notas___Grupo_2.Models
{
    public class Audio
    {
        public int id { get; set; }
        public string title { get; set; }
        public string audio { get; set; }
        public DateTime fecha { get; set; }

        // Clave foránea
        public int userId { get; set; }
        public User user { get; set; }
    }
}

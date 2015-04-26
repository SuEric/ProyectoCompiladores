using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoCompiladores
{
    class TablaSimbolos
    {
        List<Clase_valores> clases;    // de las clases queremos saber su mod, y si es extendida de otra clase
        
        public TablaSimbolos()
        {
            clases = new List<Clase_valores>();
            
        }
        public List<Clase_valores> Clases
        {
            get { return clases; }
            //set { clases = value; }
        }
        
    }
}

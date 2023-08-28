using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekat_PR32_2019.Model
{
    public class PozicijaUMatrici
    {
        private PozicijaUMatrici roditelj;
        private int red;
        private int kolona;

        public PozicijaUMatrici()
        {
        }

        public PozicijaUMatrici Roditelj { get { return roditelj; } set { roditelj = value; } }
        public int Red { get { return red; } set { red = value; } }
        public int Kolona { get { return kolona; } set { kolona = value; } }
    }
}

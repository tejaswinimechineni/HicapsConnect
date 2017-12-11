using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace HicapsConnectControl
{
    [ComVisible(false)]
    public class ActiveXControlHelpers 
    {
        public static Color GetColorFromOleColor(int oleColor)
        {
            return ColorTranslator.FromOle((int)oleColor);
        }
       
       
        public static int GetOleColorFromColor(Color color)
        {
            return ColorTranslator.ToOle(color);
        }
        
    }
}

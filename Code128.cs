// Code128.cs created with MonoDevelop
// User: elektordi at 15:43 31/08/2008
//

using System;
using System.Collections.Generic;

namespace barcodereader
{
	
	public class Code128
	{
        
        protected static char[] Code128ComboAB = new char[] { 
            ' ', '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*',
            '+', ',', '-', '.', '/', '0',  '1', '2', '3', '4', '5',
            '6', '7', '8', '9', ':', ';',  '<', '=', '>', '?', '@',
            'A', 'B', 'C', 'D', 'E', 'F',  'G', 'H', 'I', 'J', 'K',
            'L', 'M', 'N', 'O', 'P', 'Q',  'R', 'S', 'T', 'U', 'V',
            'W', 'X', 'Y', 'Z', '[', '\\', ']', '^', '_'
        };

        protected static char[] Code128B = new char[] { 
            '`', 'a', 'b',  'c', 'd', 'e', 'f',  'g', 'h', 'i', 'j',
            'k', 'l', 'm',  'n', 'o', 'p', 'q',  'r', 's', 't', 'u',
            'v', 'w', 'x',  'y', 'z', '{', '|',  '}', '~'
        };

        protected static string[] Code128Bars = new string[] {
            "212222",
            "222122",
            "222221",
            "121223",
            "121322",
            "131222",
            "122213",
            "122312",
            "132212",
            "221213",
            "221312",
            "231212",
            "112232",
            "122132",
            "122231",
            "113222",
            "123122",
            "123221",
            "223211",
            "221132",
            "221231",
            "213212",
            "223112",
            "312131",
            "311222",
            "321122",
            "321221",
            "312212",
            "322112",
            "322211",
            "212123",
            "212321",
            "232121",
            "111323",
            "131123",
            "131321",
            "112313",
            "132113",
            "132311",
            "211313",
            "231113",
            "231311",
            "112133",
            "112331",
            "132131",
            "113123",
            "113321",
            "133121",
            "313121",
            "211331",
            "231131",
            "213113",
            "213311",
            "213131",
            "311123",
            "311321",
            "331121",
            "312113",
            "312311",
            "332111",
            "314111",
            "221411",
            "431111",
            "111224",
            "111422",
            "121124",
            "121421",
            "141122",
            "141221",
            "112214",
            "112412",
            "122114",
            "122411",
            "142112",
            "142211",
            "241211",
            "221114",
            "413111",
            "241112",
            "134111",
            "111242",
            "121142",
            "121241",
            "114212",
            "124112",
            "124211",
            "411212",
            "421112",
            "421211",
            "212141",
            "214121",
            "412121",
            "111143",
            "111341",
            "131141",
            "114113",
            "114311",
            "411113",
            "411311",
            "113141",
            "114131",
            "311141",
            "411131",
            "211412", // StartA
            "211214", // StartB
            "211232"  // StartC
        };

        protected static string Code128Stop = "2331112";
        protected enum Code128ChangeModes :uint { CodeA = 101, CodeB = 100, CodeC = 99 };
        protected enum Code128StartModes :uint { CodeUnset = 0, CodeA = 103, CodeB = 104, CodeC = 105 };
        protected enum Code128Modes :uint { CodeUnset = 0, CodeA = 1, CodeB = 2, CodeC = 3 };
		
        protected Dictionary<string, uint> codes;
        protected Code128Modes mode;
        
		public Code128()
		{
            codes = new Dictionary<string, uint>();
            uint i=0;
            
            foreach (string c in Code128Bars)
                codes.Add(c, i++);

		}
		
		public string Decode(int[] bars, int count)
		{
			int i;
            string bc = "", c = "", lenghts = "";
			for(i=0;i<count;i++)
			{
                c = (bars[i]/2).ToString();
                if(c=="0") c="1";
                lenghts+=c;
            }
            
			for(i=0;i<count;i+=6)
            {
                if(i+12<count)
                    c = lenghts.Substring(i, 6);
                else
                {
                    c = lenghts.Substring(i);
                    break;
                }
                
                //MainWindow.me.Log("Code: "+c);
                
                string v="";                
                
                try
                {
                    uint id = codes[c];
                    //MainWindow.me.Log("ID: "+id.ToString());
                    if(mode == Code128Modes.CodeUnset)
                    {
                        if(id==(uint)Code128StartModes.CodeA)
                            mode = Code128Modes.CodeA;
                        else if(id==(uint)Code128StartModes.CodeB)
                            mode = Code128Modes.CodeB;
                        else if(id==(uint)Code128StartModes.CodeC)
                            mode = Code128Modes.CodeC;
                        else
                        {
                            MainWindow.me.Log("L'initialisation du code-barres est incorrecte. Le décodage sera peut être éronné !");
                            mode = Code128Modes.CodeB; // Le plus courant...
                        }
                        //MainWindow.me.Log("START "+mode.ToString());
                        
                    }
                    else
                    {
                        if(id>=99) // Change mode
                        {
                            if(id==(uint)Code128StartModes.CodeA)
                                mode = Code128Modes.CodeA;
                            else if(id==(uint)Code128StartModes.CodeB)
                                mode = Code128Modes.CodeB;
                            else if(id==(uint)Code128StartModes.CodeC)
                            {
                                if(mode==Code128Modes.CodeC)
                                    v = "99"; // Recouvrement AB:ModeC / C:99
                                else
                                    mode = Code128Modes.CodeC;
                            }
                            //MainWindow.me.Log("SWITCH "+mode.ToString());
                        }
                        else if(mode==Code128Modes.CodeC)
                        {
                            if(id<10)
                                v = "0"+id.ToString();
                            else
                                v = id.ToString("");
                        }
                        else if(id<64)
                        {
                            v = Code128ComboAB[id].ToString();
                        }
                        else if(mode==Code128Modes.CodeB)
                        {
                            v = Code128B[id-64].ToString();
                        }
                        else
                            v="?";
                    }
                    //MainWindow.me.Log(id.ToString()+"("+mode.ToString()+")= "+v);
                    
                }
                catch(Exception)
                {
                    if(mode == Code128Modes.CodeC)
                        v="??";
                    else
                        v="?";
                    
                }
                
                bc+=v;
            }
                
            if(c!=Code128Stop)
            {
                MainWindow.me.Log("La terminaison du code-barres est incorrecte. Le décodage est peut être éronné !");
            }
            
			return bc;
		}
		
		public bool Try(int[] bars, int count)
		{
			if(count<14) return false;
			
            int i;
            string c = "", ini = "";
			for(i=0;i<6;i++)
			{
                c = (bars[i]/2).ToString();
                if(c=="0") c="1";
                ini+=c;
            }
            
            MainWindow.me.Log("INI="+ini);
            
            if(ini=="211412") return true; // StartA
            if(ini=="211214") return true; // StartB
            if(ini=="211232") return true; // StartC
            
            return false;
            
		}
	}
}

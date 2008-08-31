// Code39.cs created with MonoDevelop
// User: elektordi at 03:30 31/08/2008
//

using System;
using System.Collections.Generic;

namespace barcodereader
{
	
	public class Code39
	{
		
		protected Dictionary<string, char> codes;
		
		public Code39()
		{
            object[][] chars = new object[][] 
            {
                new object[] {'0', "nnnwwnwnn"},
                new object[] {'1', "wnnwnnnnw"},
                new object[] {'2', "nnwwnnnnw"},
                new object[] {'3', "wnwwnnnnn"},
                new object[] {'4', "nnnwwnnnw"},
                new object[] {'5', "wnnwwnnnn"},
                new object[] {'6', "nnwwwnnnn"},
                new object[] {'7', "nnnwnnwnw"},
                new object[] {'8', "wnnwnnwnn"},
                new object[] {'9', "nnwwnnwnn"},
                new object[] {'A', "wnnnnwnnw"},
                new object[] {'B', "nnwnnwnnw"},
                new object[] {'C', "wnwnnwnnn"},
                new object[] {'D', "nnnnwwnnw"},
                new object[] {'E', "wnnnwwnnn"},
                new object[] {'F', "nnwnwwnnn"},
                new object[] {'G', "nnnnnwwnw"},
                new object[] {'H', "wnnnnwwnn"},
                new object[] {'I', "nnwnnwwnn"},
                new object[] {'J', "nnnnwwwnn"},
                new object[] {'K', "wnnnnnnww"},
                new object[] {'L', "nnwnnnnww"},
                new object[] {'M', "wnwnnnnwn"},
                new object[] {'N', "nnnnwnnww"},
                new object[] {'O', "wnnnwnnwn"},
                new object[] {'P', "nnwnwnnwn"},
                new object[] {'Q', "nnnnnnwww"},
                new object[] {'R', "wnnnnnwwn"},
                new object[] {'S', "nnwnnnwwn"},
                new object[] {'T', "nnnnwnwwn"},
                new object[] {'U', "wwnnnnnnw"},
                new object[] {'V', "nwwnnnnnw"},
                new object[] {'W', "wwwnnnnnn"},
                new object[] {'X', "nwnnwnnnw"},
                new object[] {'Y', "wwnnwnnnn"},
                new object[] {'Z', "nwwnwnnnn"},
                new object[] {'-', "nwnnnnwnw"},
                new object[] {'.', "wwnnnnwnn"},
                new object[] {' ', "nwwnnnwnn"},
                new object[] {'*', "nwnnwnwnn"},
                new object[] {'$', "nwnwnwnnn"},
                new object[] {'/', "nwnwnnnwn"},
                new object[] {'+', "nwnnnwnwn"},
                new object[] {'%', "nnnwnwnwn"}
            };

            codes = new Dictionary<string, char>();
			
            foreach (object[] c in chars)
                codes.Add((string)c[1], (char)c[0]);

		}
		
		public string Decode(int[] bars, int count)
		{
			int i, j;
			string bc = "";
            char c = '?';
            
            if(count%10 != 9) MainWindow.me.Log("La longeur du code-barres est incorrecte. Le décodage sera peut être éronné !");     
            
			for(i=0;i<count;i+=10)
			{
				
				//MainWindow.me.Log("I="+i.ToString()+" Count="+count.ToString());
				string b = "";
				for(j=0;j<9;j++)
				{
					//MainWindow.me.Log("J="+j.ToString()+" I+J="+(i+j).ToString());
					if(i+j>=count) break;
					if(bars[i+j]>4)
						b+="w";
					else
						b+="n";
				}
				c = '?';
				try
				{
					c = codes[b];
				}
				catch (Exception)
				{
				}
                
                if(i==0 && c!='*') MainWindow.me.Log("L'initialisation du code-barres est incorrecte. Le décodage sera peut être éronné !");
                
				bc += c;
				//MainWindow.me.Log("Bars: "+b+" => "+c);
			}
            if(c!='*')
                MainWindow.me.Log("La terminaison du code-barres est incorrecte. Le décodage est peut être éronné !");
            
			return bc;
		}
		
		public bool Try(int[] bars, int count)
		{
			if(count<20) return false;
            if(count%10 != 9) return false;
			
			int i;
			string b = "";
			for(i=0;i<9;i++)
			{
				if(bars[i]>4)
					b+="w";
				else
					b+="n";
			}
			return (b=="nwnnwnwnn"); // "*"
		}
	}
}

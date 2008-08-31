// MainWindow.cs created with MonoDevelop
// User: elektordi at 17:27 30/08/2008
//
using System;
using System.Collections;
using Gtk;
using Gdk;
using barcodereader;

public partial class MainWindow: Gtk.Window
{	
	public string bc = "";
	public Clipboard pp;
	public static MainWindow me;
	
	protected bool ImgReady = false;
	protected int x1, y1, x2, y2;
    protected int nw, nh;
	protected Pixbuf baseimg, scaled;
	protected Gdk.Window draw;
	protected float factor;
	protected int movejump;
	
	protected Code39 code39;
    protected Code128 code128;
	
	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		me = this;
		
		Build ();
		
		pp = Clipboard.Get(Atom.Intern("PRIMARY",true));
		draw = img.GdkWindow;
        
		code39 = new Code39();
        code128 = new Code128();
	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
	
	public void Log(string line)
	{
		txtLog.Buffer.Text += line+"\n";
		txtLog.ScrollToIter(txtLog.Buffer.EndIter,0,true,1,1);
	}
	
	public void MsgBox(string msg)
	{
		MessageDialog dlg = new MessageDialog(this, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, msg);
		dlg.Title = "TODO";
		/*
		Dialog dlg = new Dialog("TODO",this,DialogFlags.Modal);
		dlg.VBox.Add(new Label("La fonction n'a pas encore été développée !"));
		dlg.VBox.Visible = true;
		dlg.AddButton("OK",0);
		*/
		dlg.Run();
		dlg.Destroy();
	}

	protected virtual void OnDepuisUnFichierActionActivated (object sender, System.EventArgs e)
	{
		
		FileChooserDialog dlg = new FileChooserDialog("Title", this, FileChooserAction.Open);
		dlg.AddButton("Annuler", 0);
		dlg.AddButton("Ouvrir", 1);
		FileFilter f = new FileFilter();
		f.Name = "Images";
		f.AddMimeType("image/*");
		dlg.AddFilter(f);
		int r = dlg.Run();
		
		if(r>0)
		{
			Log("Ouverture de: "+dlg.Filename);
			Gtk.Image i = new Gtk.Image(dlg.Filename);
			ImportImage(i.Pixbuf);
			i.Clear();
		}
		
		dlg.Destroy();
	}
	
	protected void ImportImage(Pixbuf i)
	{
		//Pixbuf i;
		ImgReady = false;
		draw.Clear();
		
		if(i == null)
		{
			Log("Erreur lors de la lecture de l'image !");
			return;
		}
		if(i.Width==0 || i.Height==0)
		{
			Log("Image invalide !");
			return;
		}
		if(i.NChannels != 3 && i.NChannels != 4)
		{
			Log("Informations de couleurs invalides !");
			return;
		}
		
		baseimg = i;
		RedrawImg();
		ImgReady = true;
		Log("Image prête !");
	}
	
	protected void RedrawImg()
	{
		//Log("Redraw !");
		draw.Clear();
		float fw, fh;
		//Log("W: "+baseimg.Width.ToString()+"/"+img.Allocation.Width.ToString());
		//Log("H: "+baseimg.Height.ToString()+"/"+img.Allocation.Height.ToString());
		fw = (float)baseimg.Width / (float)img.Allocation.Width;
		fh = (float)baseimg.Height / (float)img.Allocation.Height;  
		//Log("fw="+fw.ToString()+" fh="+fh.ToString());
		if(fw>fh) factor = fw;
		else factor = fh;
		nw = (int)(baseimg.Width/factor);
		nh = (int)(baseimg.Height/factor);
		scaled = baseimg.ScaleSimple(nw, nh, InterpType.Nearest);
		Gdk.GC gc = new Gdk.GC(draw);
		draw.DrawPixbuf(gc, scaled, 0, 0, 0, 0, nw, nh, RgbDither.Normal, 0, 0);
	}
	
	protected void ReadBarcode()
	{
		/*int x, y;
		draw.BeginPaintRegion(draw.VisibleRegion);
		for(x=0; x<scaled.Width; x++)
		{
			for(y=0; y<scaled.Height; y++)
			{
				IntPtr p = new IntPtr(baseimg.Pixels.ToInt32() + y * baseimg.Rowstride + x * baseimg.NChannels);
				byte r, g, b;
				unsafe
				{				
					r = (byte)p.ToPointer();
					p = new IntPtr(p.ToInt32()+1);
					g = (byte)p.ToPointer();
					p = new IntPtr(p.ToInt32()+1);
					b = (byte)p.ToPointer();
				}
				Gdk.GC gc = new Gdk.GC(draw);
				gc.Foreground = new Color(r,g,b);
				draw.DrawPoint(gc, x, y);
			}
		}
		draw.EndPaint();*/
		
        try
        {
    		bc = "";
    		int l, i;
    		l = (int)( Math.Sqrt( Math.Pow(x2-x1,2) + Math.Pow(y2-y1,2) ) *factor);
    		Log("Tentative de lecture du code-barres (Précision: "+l.ToString()+")");
    		
    		int w = (int)((x2-x1)*factor), h = (int)((y2-y1)*factor);

    		if(w<40)
    		{
    			Log("Code-barres trop petit !");
    			return;
    		}
    		
    		bool[] data = new bool[l]; // true = black, false = blank
    		bool previous = false;
    		ArrayList sizes = new ArrayList();
    		int count = 0, size = 0, bigger = 0;
    		
    		int Xs = (int)(x1*factor);
    		int Ys = (int)(y1*factor);
    		
    		Gdk.GC gc = new Gdk.GC(draw);
            draw.Clear();
    		draw.DrawPixbuf(gc, scaled, 0, 0, 0, 0, scaled.Width, scaled.Height, RgbDither.Normal, 0, 0);
    		Colormap cm = Colormap.System;
    		Color red = new Color(255, 0, 0);
    		Color green = new Color(0, 255, 0);
    		cm.AllocColor(ref red, true, true);
    		cm.AllocColor(ref green, true, true);

    		byte[] buffer = baseimg.SaveToBuffer("png");
    		System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(new System.IO.MemoryStream(buffer));
    		
    		for(i=0;i<l;i++)
    		{
    			int x = Xs + (w*i)/l;
    			int y = Ys + (h*i)/l;
                //Log("x="+x.ToString()+" y="+y.ToString());
    			
                bool val;
                
    			System.Drawing.Color c = bmp.GetPixel(x, y);
                
                byte r = c.R, g = c.G, b = c.B, a=c.A;
                //Log("R="+r.ToString()+" G="+g.ToString()+" B="+b.ToString());
                
    			double v = (double)(r+g+b)/765; // 765 = 3*255
                
                if(a<255)
                    v = v*((float)a/255)+255-(float)a;
                
    			v = ((v-.5)*128+0.5)*255; // Contraste max

    			if(v < 0) val = true;
    			else if(v > 255) val = false;
    			else val = previous;
                
                
    			size++;
    			if(val != previous)
    			{
    				if(count > 0)
    				{
    					//Log("#"+count.ToString()+" ("+previous.ToString()+") => "+size.ToString());
    					if(size > bigger) bigger = size;
    					sizes.Add(size);
    				}
    				size = 0;
    				count++;
    			}
    			
    			if(val) gc.Foreground = red;
    			else gc.Foreground = green;
    			
    			draw.DrawPoint(gc, (int)(x/factor), (int)(y/factor));
    						
    			previous = data[i] = val;
    			//Log(val.ToString());
    		}
    		count--;
    		
            if(count<1)
            {
                Log("Aucun code-barres trouvé sur la ligne !");
                return;
            }
            
    		Log("Nombre d'éléments détectés: "+count.ToString());
    		int[] bars = new int[count];
    		i = 0;
    		foreach(int s in sizes.ToArray(typeof(int)))
    		{
    			int b = (int)(9*(float)s/(float)bigger);
                if(b<1) b=1;
                if(b>8) b=8;
    			bars[i++] = b;
    			//Log("#"+i.ToString()+"="+b.ToString());
    		}
    		
    		Log("Code-barre identifié. Tentative de décodage...");
    		
    		if(Code39Action.Active || ( AutoDtectionAction.Active && code39.Try(bars, count) ))
    		{
    			// *CODE 39*
    			bc = code39.Decode(bars, count);
    			Log("Code-barres 'CODE 39' décodé: "+bc);
    		}
    		else if(Code128Action.Active || ( AutoDtectionAction.Active && code128.Try(bars, count) ))
    		{
    			// *CODE 128*
    			bc = code128.Decode(bars, count);
    			Log("Code-barres 'CODE 128' décodé: "+bc);
    		}
    		else
    		{
    			Log("Code-barre inconnu !");
    		}
        }
        catch(Exception e)
        {
            Log("FATAL ERROR: "+e.Message);
        }
        
        Log("____________________________________________________");
        Log("");
	}

	protected virtual void OnDepuisLePressePapierActionActivated (object sender, System.EventArgs e)
	{
		Log("Récupération de l'image du presse papier...");
		ImportImage(pp.WaitForImage());
	}

	protected virtual void OnCopierLaValeurActionActivated (object sender, System.EventArgs e)
	{
		pp.Text = bc;
	}

	protected virtual void OnRotationManuelleActionActivated (object sender, System.EventArgs e)
	{
		MsgBox("La fonction n'a pas encore été développée !");
	}
	
	protected virtual void OnDepuisUnScannerActionActivated (object sender, System.EventArgs e)
	{
		MsgBox("Je n'ai pas encore trouvé comment on gère les scanners dans Mono !");
	}
	
	protected virtual void OnEventboxMotionNotifyEvent (object o, Gtk.MotionNotifyEventArgs args)
	{
		if(!ImgReady) return;
		
		movejump++;
		
		if(movejump>5) movejump=0;
		else if(movejump>0) return;
		
		int X = (int)args.Event.X;
		int Y = (int)args.Event.Y;
		//Log("Motion at ("+X+","+Y+") !");
		
		draw.BeginPaintRegion(draw.VisibleRegion);
		
		Gdk.GC gc = new Gdk.GC(draw);
		gc.Foreground = new Color(0,0,0);
		draw.DrawPixbuf(gc, scaled, 0, 0, 0, 0, scaled.Width, scaled.Height, RgbDither.Normal, 0, 0);
		draw.DrawLine(gc, x1, y1, X, Y);
		
		draw.EndPaint();
		
	}

	protected virtual void OnEventboxButtonPressEvent (object o, Gtk.ButtonPressEventArgs args)
	{
		if(!ImgReady) return;
		
		int X = (int)args.Event.X;
		int Y = (int)args.Event.Y;

    	Log("Début sélection à ("+X+","+Y+") !");
		
		x1 = X; y1 = Y;
		movejump=-1;
	}

	protected virtual void OnEventboxButtonReleaseEvent (object o, Gtk.ButtonReleaseEventArgs args)
	{
		if(!ImgReady) return;
		
		int X = (int)args.Event.X;
		int Y = (int)args.Event.Y;
		Log("Fin sélection à ("+X+","+Y+") !");
		
        if(X>nw) X=nw;
        if(Y>nh) Y=nh;
        
		x2 = X; y2 = Y;
		
		ReadBarcode();
	}

	protected virtual void OnImgExposeEvent (object o, Gtk.ExposeEventArgs args)
	{
		if(!ImgReady) return;
		RedrawImg();
	}


}
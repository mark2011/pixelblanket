module PixelBlanket.GtkSupport
open Gtk
open PixelBlanket.Types
open Serilog
let paintDrawingArea (drawingArea:Gtk.DrawingArea) pixbuf markup =
        let cc = Gdk.CairoHelper.Create drawingArea.GdkWindow
        cc.SetSourceRGBA (0.0, 0.0, 0.0, 1.0)
        cc.Paint ()
        Gdk.CairoHelper.SetSourcePixbuf (cc, pixbuf, float (screenWidth - pixbuf.Width) / 2.0, float (screenHeight - pixbuf.Height) / 2.0)
        cc.Paint ()
        let textSurface = new Cairo.ImageSurface (Cairo.Format.Rgb24, screenWidth, screenHeight)
        let textContext = new Cairo.Context (textSurface)
        let textLayout = Pango.CairoHelper.CreateLayout textContext
        textLayout.Wrap <- Pango.WrapMode.Word
        textLayout.Alignment <- Pango.Alignment.Left
        textLayout.SetMarkup markup
        Pango.CairoHelper.UpdateLayout (textContext, textLayout)
        Pango.CairoHelper.ShowLayout (textContext, textLayout)
        cc.SetSourceSurface (textSurface, 0, 0)
        cc.Operator <- Cairo.Operator.Add
        cc.Paint ()
        textContext.Dispose ()
        textSurface.Dispose ()
        cc.Dispose ()


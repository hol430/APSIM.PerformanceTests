using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace APSIM.PerformanceTests.Portal
{
    public partial class WebForm1 : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string a = Request.QueryString["a"];
                string r = Request.QueryString["r"];
                string g = Request.QueryString["g"];
                string b = Request.QueryString["b"];
                if (r != null && g != null && b != null)
                {
                    int ai, ri, gi, bi; // i for int
                    if (int.TryParse(r, out ri) && int.TryParse(g, out gi) && int.TryParse(b, out bi))
                    {
                        Response.Clear();
                        Color colour;
                        if (a != null && int.TryParse(a, out ai))
                            colour = Color.FromArgb(ai, ri, gi, bi);
                        else
                            colour = Color.FromArgb(ri, gi, bi);
                        WriteImage(colour);
                    }
                }
            }
        }

        /// <summary>
        /// Generates an image and performs a binary write of the image to the response.
        /// </summary>
        /// <param name="colour"></param>
        private void WriteImage(Color colour)
        {
            int width = 10;
            int height = 10;
            using (Bitmap image = new Bitmap(width, height))
            {
                using (Graphics g = Graphics.FromImage(image))
                {
                    g.FillRectangle(new SolidBrush(colour), 0, 0, width, height);
                    using (MemoryStream stream = new MemoryStream())
                    {
                        image.Save(stream, ImageFormat.Png);
                        image.Save(@"C:\Users\hol430\Desktop\test.png", ImageFormat.Png);
                        Response.BinaryWrite(stream.ToArray());
                    }
                }
            }
        }
    }
}
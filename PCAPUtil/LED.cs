using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace EARS
{
    /// <summary>
    /// custom user control for an LED 
    /// </summary>
    public partial class LED : UserControl
    {
        Graphics graphics;
        Color ledGradColor1 = Color.LightGray;
        Color ledGradColor2 = Color.DarkGray;
        Color borderGradColor1 = Color.LightGray;
        Color borderGradColor2 = Color.Black;
        Color backColor = Color.LightSteelBlue;
        _Shape ledShape = _Shape.Ellipse;
        _GradientType ledGradType = _GradientType.Center;

        float ledSize;// instantiated in OnPaint() using SetLEDSize() so the ledOffset is calculated accordingly
        float ledOffset;

        bool gammaCorrection = false;

        /// <summary>
        /// used to set the shape of the LED
        /// </summary>
        public enum _Shape
        {
            Ellipse,
            Rectangle
        }

        /// <summary>
        /// used to set the way the led colors are drawn
        /// </summary>
        public enum _GradientType
        {
            Center,
            Linear
        }
        /// <summary>
        /// default constructor
        /// </summary>
        public LED()
        {
            InitializeComponent();
        }

        /// <summary>
        /// on first draw, set the graphics item and set led to "off" or gray
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            graphics = this.CreateGraphics();
            SetLEDSize(0.75f);//set a default ledSize here, so the calculations are made for ledOffset too.
            DrawLED();
            base.OnPaint(e);
        }

        /// <summary>
        /// set the LED color gradient from  left to  right and draw LED
        /// </summary>
        /// <param name="color1"> starting color in  left</param>
        /// <param name="color2">final color  right</param>
        /// <param name="force">set true to force redraw even if values haven't changed</param>
        public void SetLEDGradient(Color color1, Color color2, bool force = false)
        {
            if (ledGradColor1 != color1 || ledGradColor2 != color2 || force)
            {
                ledGradColor1 = color1;
                ledGradColor2 = color2;
                DrawLED();
            }
        }

        /// <summary>
        /// set the color gradient for the ring around the LED and draw LED
        /// </summary>
        /// <param name="color1">starting color left</param>
        /// <param name="color2">ending color right</param>
        /// <param name="force">set true to force redraw if colors are the same, false will not redraw if colors have not changed</param>
        public void SetLEDBorderGradient(Color color1, Color color2, bool force = false)
        {
            if (ledGradColor1 != color1 || ledGradColor2 != color2 || force)
            {
                borderGradColor1 = color1;
                borderGradColor2 = color2;
                DrawLED();
            }
        }

        /// <summary>
        /// set the background color of the usercontrol and draw LED
        /// </summary>
        /// <param name="color">background color</param>
        /// <param name="force">set true to force redraw even if the color is the same as the current</param>
        public void SetBackColor(Color color, bool force = false)
        {
            if (backColor != color || force)
            {
                backColor = color;
                DrawLED();
            }
        }

        public void SetLEDSize(float size, bool force= false)
        {
            if (ledSize != size || force)
            {
                ledSize = size;
                ledOffset = (1 - size) / 2;
                DrawLED();
            }
        }

        /// <summary>
        /// Set LED shape 
        /// </summary>
        /// <param name="shape">shape of rectangle</param>
        /// <param name="force">set true to force redraw even if value is the same</param>
        public void SetLEDShape(_Shape shape, bool force = false)
        {
            if (ledShape != shape || force)
            {
                ledShape = shape;
                DrawLED();
            }
        }

        /// <summary>
        /// set GammaCorrection, only applies to Linear Gradient
        /// </summary>
        /// <param name="gc"></param>
        /// <param name="force"></param>
        public void SetGammaCorrection(bool gc, bool force = false)
        {
            if(gammaCorrection != gc || force)
            {
                gammaCorrection = gc;
                DrawLED();
            }
        }

        /// <summary>
        /// Redraw the LED 
        /// </summary>
        private void DrawLED()
        {
            graphics.Clear(backColor);
            
            LinearGradientBrush borderGraBrush = new LinearGradientBrush(
                new Point(0, 0),
                new Point(this.ClientSize.Width, this.ClientSize.Height),
                borderGradColor1,
               borderGradColor2);

            Brush ledGraBrush = null;
            switch (ledGradType)
            {
                case _GradientType.Center:
                    GraphicsPath path = new GraphicsPath();
                    switch(ledShape)
                    {
                        case _Shape.Ellipse:
                            path.AddEllipse((this.ClientSize.Width * ledOffset),
                                (this.ClientSize.Height * ledOffset),
                                (this.ClientSize.Width * ledSize),
                                 (this.ClientSize.Height * ledSize));
                            break;
                        case _Shape.Rectangle:
                            path.AddRectangle(new Rectangle((int)(this.ClientSize.Width * ledOffset),
                                (int)(this.ClientSize.Height * ledOffset),
                                (int)(this.ClientSize.Width * ledSize),
                                 (int)(this.ClientSize.Height * ledSize)))
                                ;
                            break;
                    }

                    ledGraBrush = new PathGradientBrush(path)
                    {
                        CenterColor = ledGradColor1,
                        SurroundColors = new Color[] { ledGradColor2 }
                    };
                    break;
                case _GradientType.Linear:
                    ledGraBrush = new LinearGradientBrush(
                        new Point((int)(this.ClientSize.Width * ledOffset), this.ClientSize.Height / 2),
                        new Point(this.ClientSize.Width - ((int)(this.ClientSize.Width * ledOffset)), this.ClientSize.Height / 2),
                        ledGradColor1,
                       ledGradColor2)
                    {
                        GammaCorrection = gammaCorrection
                    };
                    break;
            }

            switch (ledShape)
            {
                case _Shape.Ellipse:
                    graphics.FillEllipse(borderGraBrush, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
                    graphics.FillEllipse(ledGraBrush, (float)(this.ClientSize.Width * ledOffset),
                        (float)(this.ClientSize.Height * ledOffset),
                        (float)(this.ClientSize.Width * ledSize),
                        (float)(this.ClientSize.Height *ledSize));
                    break;

                case _Shape.Rectangle:
                    graphics.FillRectangle(borderGraBrush, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
                    graphics.FillRectangle(ledGraBrush, (float)(this.ClientSize.Width * ledOffset),
                        (float)(this.ClientSize.Height * ledOffset),
                        (float)(this.ClientSize.Width * ledSize),
                        (float)(this.ClientSize.Height * ledSize));
                    break;
            }
        }
    }
}

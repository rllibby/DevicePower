/*
 *  Copyright © 2016 Russell Libby
 *  
 *  http://blogs.u2u.be/diederik/post/2013/06/24/A-Modern-UI-radial-gauge-control-for-Windows-8-Store-apps.aspx
 *  
 */

using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace DevicePower.UserControls
{
    /// <summary>
    /// A Modern UI Radial Gauge.
    /// </summary>
    [TemplatePart(Name = NeedlePartName, Type = typeof(Path))]
    [TemplatePart(Name = ScalePartName, Type = typeof(Path))]
    [TemplatePart(Name = TrailPartName, Type = typeof(Path))]
    [TemplatePart(Name = ValueTextPartName, Type = typeof(TextBlock))]
    public class Gauge : Control
    {
        #region Constants

        private const string NeedlePartName = "PART_Needle";
        private const string ScalePartName = "PART_Scale";
        private const string TrailPartName = "PART_Trail";
        private const string ValueTextPartName = "PART_ValueText";
        private const double Degrees2Radians = Math.PI / 180;

        #endregion Constants

        #region Dependency Property Registrations

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(Gauge), new PropertyMetadata(0.0));
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(Gauge), new PropertyMetadata(100.0));
        public static readonly DependencyProperty ScaleWidthProperty = DependencyProperty.Register("ScaleWidth", typeof(double), typeof(Gauge), new PropertyMetadata(26.0));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(Gauge), new PropertyMetadata(0.0, OnValueChanged));
        public static readonly DependencyProperty UnitProperty = DependencyProperty.Register("Unit", typeof(string), typeof(Gauge), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty NeedleBrushProperty = DependencyProperty.Register("NeedleBrush", typeof(Brush), typeof(Gauge), new PropertyMetadata(new SolidColorBrush(Colors.OrangeRed)));
        public static readonly DependencyProperty ScaleBrushProperty = DependencyProperty.Register("ScaleBrush", typeof(Brush), typeof(Gauge), new PropertyMetadata(new SolidColorBrush(Colors.LightGray)));
        public static readonly DependencyProperty TickBrushProperty = DependencyProperty.Register("TickBrush", typeof(Brush), typeof(Gauge), new PropertyMetadata(new SolidColorBrush(Colors.DimGray)));
        public static readonly DependencyProperty TrailBrushProperty = DependencyProperty.Register("TrailBrush", typeof(Brush), typeof(Gauge), new PropertyMetadata(new SolidColorBrush(Colors.Orange)));
        public static readonly DependencyProperty ValueBrushProperty = DependencyProperty.Register("ValueBrush", typeof(Brush), typeof(Gauge), new PropertyMetadata(new SolidColorBrush(Colors.DimGray)));
        public static readonly DependencyProperty ScaleTickBrushProperty = DependencyProperty.Register("ScaleTickBrush", typeof(Brush), typeof(Gauge), new PropertyMetadata(new SolidColorBrush(Colors.White)));
        public static readonly DependencyProperty UnitBrushProperty = DependencyProperty.Register("UnitBrush", typeof(Brush), typeof(Gauge), new PropertyMetadata(new SolidColorBrush(Colors.DimGray)));
        public static readonly DependencyProperty ValueStringFormatProperty = DependencyProperty.Register("ValueStringFormat", typeof(string), typeof(Gauge), new PropertyMetadata("N0"));
        protected static readonly DependencyProperty ValueAngleProperty = DependencyProperty.Register("ValueAngle", typeof(double), typeof(Gauge), new PropertyMetadata(null));
        protected static readonly DependencyProperty TicksProperty = DependencyProperty.Register("Ticks", typeof(IEnumerable<double>), typeof(Gauge), new PropertyMetadata(null));

        #endregion Dependency Property Registrations

        #region Private methods

        /// <summary>
        /// Method that is called when a value changes.
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="e">The changed event arguments.</param>
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (Gauge)d;

            if (double.IsNaN(c.Value)) return;

            var middleOfScale = 77 - c.ScaleWidth / 2;
            var needle = c.GetTemplateChild(NeedlePartName) as Path;
            var valueText = c.GetTemplateChild(ValueTextPartName) as TextBlock;

            c.ValueAngle = c.ValueToAngle(c.Value);

            if (needle != null) needle.RenderTransform = new RotateTransform() { Angle = c.ValueAngle };

            var trail = c.GetTemplateChild(TrailPartName) as Path;

            if (trail != null)
            {
                if (c.ValueAngle > -146)
                {
                    var pg = new PathGeometry();
                    var pf = new PathFigure();
                    var seg = new ArcSegment();

                    trail.Visibility = Visibility.Visible;
                    pf.IsClosed = false;
                    pf.StartPoint = c.ScalePoint(-150, middleOfScale);
                    seg.SweepDirection = SweepDirection.Clockwise;
                    seg.IsLargeArc = c.ValueAngle > 30;
                    seg.Size = new Size(middleOfScale, middleOfScale);
                    seg.Point = c.ScalePoint(c.ValueAngle, middleOfScale);
                    pf.Segments.Add(seg);
                    pg.Figures.Add(pf);
                    trail.Data = pg;
                }
                else
                {
                    trail.Visibility = Visibility.Collapsed;
                }
            }

            if (valueText != null) valueText.Text = c.Value.ToString(c.ValueStringFormat);
        }

        /// <summary>
        /// Returns a point based on the angle.
        /// </summary>
        /// <param name="angle">The angle used to calculate the point.</param>
        /// <param name="middleOfScale">The middle of the scale.</param>
        /// <returns>The canvas point.</returns>
        private Point ScalePoint(double angle, double middleOfScale)
        {
            return new Point(100 + Math.Sin(Degrees2Radians * angle) * middleOfScale, 100 - Math.Cos(Degrees2Radians * angle) * middleOfScale);
        }

        /// <summary>
        /// Converts the value to an angle.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The angle of the value.</returns>
        private double ValueToAngle(double value)
        {
            var minAngle = -150.0;
            var maxAngle = 150.0;

            if (value < Minimum) return minAngle - 7.5;
            if (value > Maximum) return maxAngle + 7.5;

            var angularRange = maxAngle - minAngle;

            return (value - Minimum) / (Maximum - Minimum) * angularRange + minAngle;
        }

        /// <summary>
        /// Generates the ticks based on min and max values.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<double> getTicks()
        {
            var tickSpacing = (Maximum - Minimum) / 10;

            for (var tick = Minimum; tick <= Maximum; tick += tickSpacing)
            {
                yield return ValueToAngle(tick);
            }
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Method that is triggered when the template needs to be applied.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            var scale = GetTemplateChild(ScalePartName) as Path;

            if (scale != null)
            {
                var pg = new PathGeometry();
                var pf = new PathFigure();

                pf.IsClosed = false;

                var middleOfScale = 77 - ScaleWidth / 2;

                pf.StartPoint = ScalePoint(-150, middleOfScale);

                var seg = new ArcSegment();

                seg.SweepDirection = SweepDirection.Clockwise;
                seg.IsLargeArc = true;
                seg.Size = new Size(middleOfScale, middleOfScale);
                seg.Point = ScalePoint(150, middleOfScale);
                pf.Segments.Add(seg);
                pg.Figures.Add(pf);
                scale.Data = pg;
            }

            OnValueChanged(this, null);

            base.OnApplyTemplate();
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public Gauge()
        {
            DefaultStyleKey = typeof(Gauge);
            Ticks = getTicks();
        }

        #endregion Constructors

        #region Protected properties

        /// <summary>
        /// The value angle.
        /// </summary>
        protected double ValueAngle
        {
            get { return (double)GetValue(ValueAngleProperty); }
            set { SetValue(ValueAngleProperty, value); }
        }

        /// <summary>
        /// The enumeration of ticks.
        /// </summary>
        protected IEnumerable<double> Ticks
        {
            get { return (IEnumerable<double>)GetValue(TicksProperty); }
            set { SetValue(TicksProperty, value); }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Minimum value for guage.
        /// </summary>
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        /// <summary>
        /// Maximum value for guage.
        /// </summary>
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        /// <summary>
        /// Thickness of the scale in pixels  
        /// </summary>
        public double ScaleWidth
        {
            get { return (double)GetValue(ScaleWidthProperty); }
            set { SetValue(ScaleWidthProperty, value); }
        }

        /// <summary>
        /// Current value.
        /// </summary>
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// The unit string.
        /// </summary>
        public string Unit
        {
            get { return (string)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        /// <summary>
        /// Brush for the needle.
        /// </summary>
        public Brush NeedleBrush
        {
            get { return (Brush)GetValue(NeedleBrushProperty); }
            set { SetValue(NeedleBrushProperty, value); }
        }

        /// <summary>
        /// The brush for the guage trail (current progress).
        /// </summary>
        public Brush TrailBrush
        {
            get { return (Brush)GetValue(TrailBrushProperty); }
            set { SetValue(TrailBrushProperty, value); }
        }

        /// <summary>
        /// The brush for the guage scale (remaining progress).
        /// </summary>
        public Brush ScaleBrush
        {
            get { return (Brush)GetValue(ScaleBrushProperty); }
            set { SetValue(ScaleBrushProperty, value); }
        }

        /// <summary>
        /// The scale tick brush.
        /// </summary>
        public Brush ScaleTickBrush
        {
            get { return (Brush)GetValue(ScaleTickBrushProperty); }
            set { SetValue(ScaleTickBrushProperty, value); }
        }

        /// <summary>
        /// The tick brush.
        /// </summary>
        public Brush TickBrush
        {
            get { return (Brush)GetValue(TickBrushProperty); }
            set { SetValue(TickBrushProperty, value); }
        }

        /// <summary>
        /// The value brush.
        /// </summary>
        public Brush ValueBrush
        {
            get { return (Brush)GetValue(ValueBrushProperty); }
            set { SetValue(ValueBrushProperty, value); }
        }

        /// <summary>
        /// The unit brush.
        /// </summary>
        public Brush UnitBrush
        {
            get { return (Brush)GetValue(UnitBrushProperty); }
            set { SetValue(UnitBrushProperty, value); }
        }

        /// <summary>
        /// The format for the value string.
        /// </summary>
        public string ValueStringFormat
        {
            get { return (string)GetValue(ValueStringFormatProperty); }
            set { SetValue(ValueStringFormatProperty, value); }
        }

        #endregion
    }
}

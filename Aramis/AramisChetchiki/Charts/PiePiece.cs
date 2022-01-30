namespace TMP.WORK.AramisChetchiki.Charts
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Shapes;

    /// <summary>
    /// Сектор круговой диаграммы
    /// </summary>
    internal class PiePiece : Shape
    {
        #region dependency properties

        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("RadiusProperty", typeof(double), typeof(PiePiece),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Радиус сектора
        /// </summary>
        public double Radius
        {
            get => (double)this.GetValue(RadiusProperty);
            set => this.SetValue(RadiusProperty, value);
        }

        public static readonly DependencyProperty PushOutProperty =
            DependencyProperty.Register("PushOutProperty", typeof(double), typeof(PiePiece),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Расстояния выноса сектора от центра
        /// </summary>
        public double PushOut
        {
            get => (double)this.GetValue(PushOutProperty);
            set => this.SetValue(PushOutProperty, value);
        }

        public static readonly DependencyProperty InnerRadiusProperty =
            DependencyProperty.Register("InnerRadiusProperty", typeof(double), typeof(PiePiece),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Внутренний радиус сектора
        /// </summary>
        public double InnerRadius
        {
            get => (double)this.GetValue(InnerRadiusProperty);
            set => this.SetValue(InnerRadiusProperty, value);
        }

        public static readonly DependencyProperty WedgeAngleProperty =
            DependencyProperty.Register("WedgeAngleProperty", typeof(double), typeof(PiePiece),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Угол сектора в градусах
        /// </summary>
        public double WedgeAngle
        {
            get => (double)this.GetValue(WedgeAngleProperty);
            set
            {
                this.SetValue(WedgeAngleProperty, value);
                this.Percentage = value / 360.0;
            }
        }

        public static readonly DependencyProperty RotationAngleProperty =
            DependencyProperty.Register("RotationAngleProperty", typeof(double), typeof(PiePiece),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Поворот по оси Y в градусах
        /// </summary>
        public double RotationAngle
        {
            get => (double)this.GetValue(RotationAngleProperty);
            set => this.SetValue(RotationAngleProperty, value);
        }

        public static readonly DependencyProperty CentreXProperty =
            DependencyProperty.Register("CentreXProperty", typeof(double), typeof(PiePiece),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Координата X центра окружности сектора
        /// </summary>
        public double CentreX
        {
            get => (double)this.GetValue(CentreXProperty);
            set => this.SetValue(CentreXProperty, value);
        }

        public static readonly DependencyProperty CentreYProperty =
            DependencyProperty.Register("CentreYProperty", typeof(double), typeof(PiePiece),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Координата Y центра окружности сектора
        /// </summary>
        public double CentreY
        {
            get => (double)this.GetValue(CentreYProperty);
            set => this.SetValue(CentreYProperty, value);
        }

        public static readonly DependencyProperty PercentageProperty =
            DependencyProperty.Register("PercentageProperty", typeof(double), typeof(PiePiece),
            new FrameworkPropertyMetadata(0.0));

        /// <summary>
        /// Процент значения ряда от всей суммы значений
        /// </summary>
        public double Percentage
        {
            get => (double)this.GetValue(PercentageProperty);
            private set => this.SetValue(PercentageProperty, value);
        }

        public static readonly DependencyProperty PieceValueProperty =
            DependencyProperty.Register("PieceValueProperty", typeof(double), typeof(PiePiece),
            new FrameworkPropertyMetadata(0.0));

        /// <summary>
        /// Значение ряда
        /// </summary>
        public double PieceValue
        {
            get => (double)this.GetValue(PieceValueProperty);
            set => this.SetValue(PieceValueProperty, value);
        }

        #endregion

        protected override Geometry DefiningGeometry
        {
            get
            {
                GeometryGroup group = new();

                StreamGeometry geometry = new();
                geometry.FillRule = FillRule.EvenOdd;

                using (StreamGeometryContext context = geometry.Open())
                {
                    this.DrawGeometry(context);
                }

                geometry.Freeze();

                group.Children.Add(geometry);

                // if (WedgeAngle > 35 && PieceValue > 0)
                // {
                //    FormattedText text = new FormattedText(PieceValue.ToString("N0"), System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                //        new Typeface("Tahoma"), 12, Brushes.Black);
                //    if (text.Width * 1.3 < geometry.Bounds.Width)
                //    {
                //        double angle = RotationAngle + WedgeAngle / 2;
                //        Point point = ComputeCartesianCoordinate(angle, Radius - text.Height);
                //        point.Offset(CentreX, CentreY);

                // Point textPoint = angle < 180
                //            ? new Point(point.X - text.Width * 0.6, point.Y - text.Height)
                //            : new Point(point.X - text.Width * 0.0, point.Y);

                // Geometry textGeometry = text.BuildGeometry(textPoint);
                //        textGeometry.Freeze();
                //        group.Children.Add(textGeometry);
                //    }
                // }
                group.Freeze();

                return group;
            }
        }

        /// <summary>
        /// Отрисовка сектора
        /// </summary>
        private void DrawGeometry(StreamGeometryContext context)
        {
            double angle = this.WedgeAngle == 360 ? 359.9 : this.WedgeAngle;
            Point startPoint = new(this.CentreX, this.CentreY);

            Point innerArcStartPoint = ComputeCartesianCoordinate(this.RotationAngle, this.InnerRadius);
            innerArcStartPoint.Offset(this.CentreX, this.CentreY);

            Point innerArcEndPoint = ComputeCartesianCoordinate(this.RotationAngle + angle, this.InnerRadius);
            innerArcEndPoint.Offset(this.CentreX, this.CentreY);

            Point outerArcStartPoint = ComputeCartesianCoordinate(this.RotationAngle, this.Radius);
            outerArcStartPoint.Offset(this.CentreX, this.CentreY);

            Point outerArcEndPoint = ComputeCartesianCoordinate(this.RotationAngle + angle, this.Radius);
            outerArcEndPoint.Offset(this.CentreX, this.CentreY);

            bool largeArc = angle > 180.0;

            if (this.PushOut > 0)
            {
                Point offset = ComputeCartesianCoordinate(this.RotationAngle + (angle / 2), this.PushOut);
                innerArcStartPoint.Offset(offset.X, offset.Y);
                innerArcEndPoint.Offset(offset.X, offset.Y);
                outerArcStartPoint.Offset(offset.X, offset.Y);
                outerArcEndPoint.Offset(offset.X, offset.Y);
            }

            Size outerArcSize = new(this.Radius, this.Radius);
            Size innerArcSize = new(this.InnerRadius, this.InnerRadius);

            context.BeginFigure(innerArcStartPoint, true, true);
            context.LineTo(outerArcStartPoint, true, true);
            context.ArcTo(outerArcEndPoint, outerArcSize, 0, largeArc, SweepDirection.Clockwise, true, true);
            context.LineTo(innerArcEndPoint, true, true);
            context.ArcTo(innerArcStartPoint, innerArcSize, 0, largeArc, SweepDirection.Counterclockwise, true, true);
        }

        /// <summary>
        /// Конвертация полярных координат
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static Point ComputeCartesianCoordinate(double angle, double radius)
        {
            double angleRad = Math.PI / 180.0 * (angle - 90);

            double x = radius * Math.Cos(angleRad);
            double y = radius * Math.Sin(angleRad);

            return new Point(x, y);
        }
    }
}

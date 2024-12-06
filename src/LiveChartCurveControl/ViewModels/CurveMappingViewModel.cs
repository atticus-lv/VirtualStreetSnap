using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Sketches;
using System.Collections.ObjectModel;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Drawing;
using SkiaSharp;
using System;
using System.Collections.Generic;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using MathNet.Numerics.Interpolation;

namespace LiveChartCurveControl.ViewModels;

public class BezierCurve
{
    private CubicSpline? _spline;
    private double _minX, _maxX, _minY, _maxY;

    public BezierCurve(List<Tuple<double, double>> points) => UpdateSpline(points);

    public void UpdateSpline(List<Tuple<double, double>> points)
    {
        var x = points.Select(p => p.Item1).ToArray();
        var y = points.Select(p => p.Item2).ToArray();

        _minX = x.First();
        _maxX = x.Last();
        _minY = Math.Min(y.First(), y.Last());
        _maxY = Math.Max(y.First(), y.Last());

        _spline = CubicSpline.InterpolateNatural(x, y);
    }

    public double CalcValue(double x) => Clamp(_spline.Interpolate(Clamp(x, _minX, _maxX)), _minY, _maxY);

    public List<Tuple<double, double>> GenerateCurvePoints(double step = 0.01)
    {
        var curvePoints = new List<Tuple<double, double>>();
        for (double t = _minX; t <= _maxX; t += step)
            curvePoints.Add(Tuple.Create(t, Clamp(_spline.Interpolate(t), _minY, _maxY)));
        return curvePoints;
    }

    private static double Clamp(double value, double min, double max) => Math.Max(min, Math.Min(max, value));
}

public partial class CurveMappingViewModel : ViewModelBase
{
    public ObservableCollection<ObservablePoint> Points { get; set; }
    public Axis[] XAxes { get; set; }
    public Axis[] YAxes { get; set; }
    public ISeries[] SeriesCollection { get; set; }
    private ObservablePoint? _selectedPoint;
    private const double SelectionRadius = 0.1;
    private BezierCurve _bezierCurve;

    public CurveMappingViewModel()
    {
        Points = [new(0, 0), new(1, 1)];
        _bezierCurve = new BezierCurve(Points.Select(p => Tuple.Create((double)p.X!, (double)p.Y!)).ToList());

        SeriesCollection =
        [
            new LineSeries<ObservablePoint>
            {
                Name = "GenerateCurve",
                Values = GenerateBezierCurve(),
                Fill = null,
                LineSmoothness = 1,
                GeometryFill = null,
                GeometryStroke = null,
                Stroke = new LinearGradientPaint([new SKColor(0, 0, 0), new SKColor(255, 255, 255)])
                    { StrokeThickness = 5 }
            },
            new LineSeries<ObservablePoint>
            {
                Name = "ControlPoint",
                Values = Points,
                Fill = null,
                LineSmoothness = 0,
                Stroke = new SolidColorPaint(SKColors.Transparent) { StrokeThickness = 1 },
                GeometryStroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 10 },
                DataPadding = new LvcPoint(5, 5)
            }
        ];

        XAxes =
        [
            new Axis
            {
                MinLimit = 0,
                MaxLimit = 1,
                MinStep = 0.25,
                SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
                    { StrokeThickness = 2, PathEffect = new DashEffect([5, 5]) }
            }
        ];
        YAxes =
        [
            new Axis
            {
                MinLimit = 0,
                MaxLimit = 1,
                MinStep = 0.25,
                SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
                    { StrokeThickness = 2, PathEffect = new DashEffect([5, 5]) }
            }
        ];
    }

    private ObservablePoint? FindNearestPoint(LvcPointD scaledPoint)
    {
        var point = Points.FirstOrDefault(p =>
            Math.Sqrt(Math.Pow((double)(p.X - scaledPoint.X)!, 2) + Math.Pow((double)(p.Y - scaledPoint.Y)!, 2)) <=
            SelectionRadius);
        return point == Points.First() || point == Points.Last() ? null : point;
    }

    [RelayCommand]
    public void PointerDown(PointerCommandArgs args)
    {
        if (Design.IsDesignMode) return;

        var chart = (ICartesianChartView<SkiaSharpDrawingContext>)args.Chart;
        var scaledPoint = chart.ScalePixelsToData(args.PointerPosition);

        if (scaledPoint.X < 0 || scaledPoint.X > 1 || scaledPoint.Y < 0 || scaledPoint.Y > 1) return;

        var avaloniaEventArgs = args.OriginalEventArgs as PointerPressedEventArgs;

        if (avaloniaEventArgs.GetCurrentPoint(null).Properties.IsRightButtonPressed)
        {
            var pointToRemove = FindNearestPoint(scaledPoint);
            if (pointToRemove == null) return;
            Points.Remove(pointToRemove);
            UpdateBezierCurve();
            return;
        }

        _selectedPoint = FindNearestPoint(scaledPoint);
        if (_selectedPoint != null) return;

        if (scaledPoint.Y > Points.Max(p => p.Y))
        {
            Points.Add(new ObservablePoint(scaledPoint.X, scaledPoint.Y));
        }
        else
        {
            int index = Points.TakeWhile(p => p.X <= scaledPoint.X).Count();
            Points.Insert(index, new ObservablePoint(scaledPoint.X, scaledPoint.Y));
        }

        UpdateBezierCurve();
    }

    [RelayCommand]
    public void PointerMove(PointerCommandArgs args)
    {
        if (_selectedPoint == null) return;

        var chart = (ICartesianChartView<SkiaSharpDrawingContext>)args.Chart;
        var scaledPoint = chart.ScalePixelsToData(args.PointerPosition);

        if (scaledPoint.X < 0 || scaledPoint.X > 1 || scaledPoint.Y < 0 || scaledPoint.Y > 1) return;

        _selectedPoint.X = scaledPoint.X;
        _selectedPoint.Y = scaledPoint.Y;

        if (_selectedPoint != Points.First() && _selectedPoint.X < Points[Points.IndexOf(_selectedPoint) - 1].X)
        {
            Points.Move(Points.IndexOf(_selectedPoint), Points.IndexOf(_selectedPoint) - 1);
        }
        else if (_selectedPoint != Points.Last() && _selectedPoint.X > Points[Points.IndexOf(_selectedPoint) + 1].X)
        {
            Points.Move(Points.IndexOf(_selectedPoint), Points.IndexOf(_selectedPoint) + 1);
        }

        UpdateBezierCurve();
    }

    [RelayCommand]
    public void PointerUp() => _selectedPoint = null;

    private void UpdateBezierCurve()
    {
        _bezierCurve.UpdateSpline(Points.Select(p => Tuple.Create((double)p.X!, (double)p.Y!)).ToList());
        SeriesCollection[0].Values = GenerateBezierCurve();
    }

    private List<ObservablePoint> GenerateBezierCurve() => _bezierCurve.GenerateCurvePoints()
        .Select(p => new ObservablePoint(p.Item1, p.Item2)).ToList();
}
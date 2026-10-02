using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace FileFlow.App.Views.Components;

/// <summary>
/// Fondo de papel milimetrado para el lienzo de flujos de nodos (NodifyEditor).
/// Dibuja una cuadrícula sutil y elegante con subdivisiones menores (papel milimetrado) y guías mayores
/// que se desplaza y escala en perfecta sincronía con el paneo (ViewportLocation) y zoom (ViewportZoom) del lienzo.
/// Adapta automáticamente sus colores al tema activo a través del token GridLineBrush del sistema de temas.
/// </summary>
public class GraphPaperGridControl : Control
{
    public static readonly StyledProperty<Point> ViewportLocationProperty =
        AvaloniaProperty.Register<GraphPaperGridControl, Point>(nameof(ViewportLocation));

    public static readonly StyledProperty<double> ViewportZoomProperty =
        AvaloniaProperty.Register<GraphPaperGridControl, double>(nameof(ViewportZoom), 1.0);

    public static readonly StyledProperty<IBrush?> GridLineBrushProperty =
        AvaloniaProperty.Register<GraphPaperGridControl, IBrush?>(nameof(GridLineBrush));

    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<GraphPaperGridControl, IBrush?>(nameof(Background));

    public static readonly StyledProperty<double> CellSizeProperty =
        AvaloniaProperty.Register<GraphPaperGridControl, double>(nameof(CellSize), 25.0);

    public static readonly StyledProperty<int> MajorDivisionsProperty =
        AvaloniaProperty.Register<GraphPaperGridControl, int>(nameof(MajorDivisions), 4);

    public Point ViewportLocation
    {
        get => GetValue(ViewportLocationProperty);
        set => SetValue(ViewportLocationProperty, value);
    }

    public double ViewportZoom
    {
        get => GetValue(ViewportZoomProperty);
        set => SetValue(ViewportZoomProperty, value);
    }

    public IBrush? GridLineBrush
    {
        get => GetValue(GridLineBrushProperty);
        set => SetValue(GridLineBrushProperty, value);
    }

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public double CellSize
    {
        get => GetValue(CellSizeProperty);
        set => SetValue(CellSizeProperty, value);
    }

    public int MajorDivisions
    {
        get => GetValue(MajorDivisionsProperty);
        set => SetValue(MajorDivisionsProperty, value);
    }

    static GraphPaperGridControl()
    {
        AffectsRender<GraphPaperGridControl>(
            ViewportLocationProperty,
            ViewportZoomProperty,
            GridLineBrushProperty,
            BackgroundProperty,
            CellSizeProperty,
            MajorDivisionsProperty);
    }

    public GraphPaperGridControl()
    {
        ClipToBounds = true;
        IsHitTestVisible = false;
    }

    private Color _lastBaseColor;
    private double _lastMinorOpacity = -1;
    private Pen? _cachedMajorPen;
    private Pen? _cachedMinorPen;

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        double width = Bounds.Width;
        double height = Bounds.Height;

        if (width <= 0 || height <= 0) return;

        // Opcional: fondo base si se especifica directamente en el control
        if (Background != null)
        {
            context.DrawRectangle(Background, null, new Rect(0, 0, width, height));
        }

        double zoom = ViewportZoom > 0.01 ? ViewportZoom : 1.0;
        Point location = ViewportLocation;

        double cellSize = CellSize > 5.0 ? CellSize : 25.0;
        int majorDiv = MajorDivisions > 1 ? MajorDivisions : 4;

        double screenMinorStep = cellSize * zoom;
        if (screenMinorStep <= 0) return;

        // Atenuación suave de líneas menores cuando el zoom se aleja mucho (evita efecto moiré o ruido visual denso)
        double minorOpacity = 1.0;
        if (screenMinorStep < 14.0)
        {
            minorOpacity = Math.Clamp((screenMinorStep - 8.0) / 6.0, 0.0, 1.0);
        }

        // Obtener color base de la rejilla
        Color baseColor = ResolveGridColor();

        // Actualizar pens cacheados si cambiaron color u opacidad
        EnsurePens(baseColor, minorOpacity);

        if (_cachedMajorPen == null) return;

        // 1. Líneas verticales
        double minCanvasX = location.X;
        double maxCanvasX = location.X + (width / zoom);

        long startCol = (long)Math.Floor(minCanvasX / cellSize);
        long endCol = (long)Math.Ceiling(maxCanvasX / cellSize);

        // Seguridad contra desbordes en caso de límites extremos
        if (endCol - startCol > 1000)
        {
            startCol = 0;
            endCol = 0;
        }

        for (long k = startCol; k <= endCol; k++)
        {
            double canvasX = k * cellSize;
            double screenX = (canvasX - location.X) * zoom;

            bool isMajor = (k % majorDiv == 0);

            if (isMajor)
            {
                context.DrawLine(_cachedMajorPen, new Point(screenX, 0), new Point(screenX, height));
            }
            else if (minorOpacity > 0 && _cachedMinorPen != null)
            {
                context.DrawLine(_cachedMinorPen, new Point(screenX, 0), new Point(screenX, height));
            }
        }

        // 2. Líneas horizontales
        double minCanvasY = location.Y;
        double maxCanvasY = location.Y + (height / zoom);

        long startRow = (long)Math.Floor(minCanvasY / cellSize);
        long endRow = (long)Math.Ceiling(maxCanvasY / cellSize);

        if (endRow - startRow > 1000)
        {
            startRow = 0;
            endRow = 0;
        }

        for (long k = startRow; k <= endRow; k++)
        {
            double canvasY = k * cellSize;
            double screenY = (canvasY - location.Y) * zoom;

            bool isMajor = (k % majorDiv == 0);

            if (isMajor)
            {
                context.DrawLine(_cachedMajorPen, new Point(0, screenY), new Point(width, screenY));
            }
            else if (minorOpacity > 0 && _cachedMinorPen != null)
            {
                context.DrawLine(_cachedMinorPen, new Point(0, screenY), new Point(width, screenY));
            }
        }
    }

    private Color ResolveGridColor()
    {
        if (GridLineBrush is ISolidColorBrush solid)
        {
            return solid.Color;
        }

        if (this.TryFindResource("GridLineBrush", out var res) && res is ISolidColorBrush resSolid)
        {
            return resSolid.Color;
        }

        return Color.FromArgb(255, 226, 232, 240);
    }

    private void EnsurePens(Color baseColor, double minorOpacity)
    {
        if (_cachedMajorPen != null &&
            _cachedMinorPen != null &&
            baseColor == _lastBaseColor &&
            Math.Abs(minorOpacity - _lastMinorOpacity) < 0.01)
        {
            return;
        }

        _lastBaseColor = baseColor;
        _lastMinorOpacity = minorOpacity;

        // Línea mayor: sutil pero definida (85% de opacidad base, grosor 1.0)
        byte majorA = (byte)(baseColor.A * 0.85);
        var majorColor = Color.FromArgb(majorA, baseColor.R, baseColor.G, baseColor.B);
        _cachedMajorPen = new Pen(new SolidColorBrush(majorColor), 1.0);

        // Línea menor: papel milimetrado muy tenue (40% de opacidad base, atenuada por zoom)
        byte minorA = (byte)(baseColor.A * 0.40 * minorOpacity);
        var minorColor = Color.FromArgb(minorA, baseColor.R, baseColor.G, baseColor.B);
        _cachedMinorPen = new Pen(new SolidColorBrush(minorColor), 0.8);
    }
}

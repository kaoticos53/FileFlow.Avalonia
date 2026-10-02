using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using FileFlow.App.ViewModels;
using FileFlow.App.Views.Components;
using FileFlow.Core.Plugins;
using FileFlow.Tests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace FileFlow.Tests.Unit.Views;

[Collection(VisualSnapshotsCollection.Name)]
public class GraphPaperGridControlTests
{
    [Fact]
    public void DefaultProperties_AreSetCorrectly()
    {
        var control = new GraphPaperGridControl();

        control.CellSize.Should().Be(25.0);
        control.MajorDivisions.Should().Be(4);
        control.ViewportZoom.Should().Be(1.0);
        control.ViewportLocation.Should().Be(new Point(0, 0));
        control.ClipToBounds.Should().BeTrue();
        control.IsHitTestVisible.Should().BeFalse();
    }

    [Fact]
    public void CustomProperties_CanBeSetAndRead()
    {
        var control = new GraphPaperGridControl
        {
            CellSize = 30.0,
            MajorDivisions = 5,
            ViewportZoom = 1.5,
            ViewportLocation = new Point(120, 250),
            GridLineBrush = new SolidColorBrush(Colors.LightSlateGray)
        };

        control.CellSize.Should().Be(30.0);
        control.MajorDivisions.Should().Be(5);
        control.ViewportZoom.Should().Be(1.5);
        control.ViewportLocation.Should().Be(new Point(120, 250));
        control.GridLineBrush.Should().NotBeNull();
    }

    [Fact]
    public void Render_OnLayout_DoesNotThrow()
    {
        AvaloniaTestHelper.RunOnUI(() =>
        {
            var control = new GraphPaperGridControl
            {
                Width = 800,
                Height = 600,
                ViewportLocation = new Point(50, 50),
                ViewportZoom = 1.0,
                GridLineBrush = new SolidColorBrush(Color.Parse("#E2E8F0"))
            };

            var window = new Window
            {
                Width = 800,
                Height = 600,
                Content = control
            };

            window.Show();
            control.UpdateLayout();
            window.Close();
        });
    }

    [Fact]
    public void Render_WithNegativeCoordinatesAndVariousZooms_DoesNotThrow()
    {
        AvaloniaTestHelper.RunOnUI(() =>
        {
            var control = new GraphPaperGridControl
            {
                Width = 1000,
                Height = 700,
                ViewportLocation = new Point(-245.7, -189.3),
                ViewportZoom = 0.2, // Zoom mínimo
                GridLineBrush = new SolidColorBrush(Color.Parse("#1A202C"))
            };

            var window = new Window
            {
                Width = 1000,
                Height = 700,
                Content = control
            };

            window.Show();
            control.UpdateLayout();

            // Probar zoom máximo
            control.ViewportZoom = 2.5;
            control.UpdateLayout();

            window.Close();
        });
    }

    [Fact]
    public void ToggleGridCommand_OnEditorViewModel_TogglesShowGridProperty()
    {
        var loader = new PluginLoader();
        var vm = new EditorViewModel(loader);

        vm.ShowGrid.Should().BeTrue("la cuadrícula de papel milimetrado debe estar visible por defecto");

        vm.ToggleGridCommand.Execute(null);
        vm.ShowGrid.Should().BeFalse("ejecutar ToggleGrid debe ocultar la cuadrícula");

        vm.ToggleGridCommand.Execute(null);
        vm.ShowGrid.Should().BeTrue("ejecutar ToggleGrid nuevamente debe volver a mostrar la cuadrícula");
    }
}

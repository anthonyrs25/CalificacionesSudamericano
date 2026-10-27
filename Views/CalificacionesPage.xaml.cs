using System.ComponentModel;
using CalificacionesSudamericano.Models;
using CalificacionesSudamericano.ViewModels;

namespace CalificacionesSudamericano.Views;

public partial class CalificacionesPage : ContentPage
{
    private readonly CalificacionesViewModel _vm;
    private readonly List<View> _bloques;
    private bool _yaAnimoEntrada;
    private bool? _panelAlLado;
    private bool? _momentosEnFila;
    private bool? _datosEnFila;
    private EstadoAcademico? _ultimoEstado;

    public CalificacionesPage()
    {
        InitializeComponent();

        _vm = new CalificacionesViewModel();
        BindingContext = _vm;

        // Bloques que entran animados, en este orden
        _bloques = new List<View> { Encabezado, SeccionEstudiante };
        _bloques.AddRange(GridMomentos.Children.OfType<View>());
        _bloques.Add(PanelResumen);

        foreach (var bloque in _bloques)
        {
            bloque.Opacity = 0;
            bloque.TranslationY = 40;
        }

        _vm.PropertyChanged += AlCambiarViewModel;
        SizeChanged += AlCambiarTamano;
    }

    // ---------- ANIMACIÓN DE ENTRADA ----------
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_yaAnimoEntrada) return;
        _yaAnimoEntrada = true;

        foreach (var bloque in _bloques)
        {
            _ = bloque.FadeToAsync(1, 450, Easing.CubicOut);
            _ = bloque.TranslateToAsync(0, 0, 450, Easing.CubicOut);
            await Task.Delay(90);
        }
    }

    // ---------- REACCIONAR A CAMBIOS DEL VIEWMODEL ----------
    private void AlCambiarViewModel(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CalificacionesViewModel.Progreso))
            AnimarBarra(_vm.Progreso);

        if (e.PropertyName == nameof(CalificacionesViewModel.Estado) && _vm.Estado != _ultimoEstado)
        {
            _ultimoEstado = _vm.Estado;
            _ = AnimarChipAsync();
        }
    }

    private void AnimarBarra(double destino)
    {
        this.AbortAnimation("AnimacionBarra");
        var animacion = new Animation(v => BarraProgreso.Progress = v,
                                      BarraProgreso.Progress, destino, Easing.CubicOut);
        animacion.Commit(this, "AnimacionBarra", length: 600);
    }

    private async Task AnimarChipAsync()
    {
        await ChipEstado.ScaleToAsync(1.18, 140, Easing.CubicOut);
        await ChipEstado.ScaleToAsync(1.0, 220, Easing.SpringOut);
    }

    // ---------- BOTÓN: validar, guardar y mostrar reporte ----------
    private async void AlPresionarCalcular(object? sender, EventArgs e)
    {
        await BotonCalcular.ScaleToAsync(0.96, 80);
        await BotonCalcular.ScaleToAsync(1.0, 80);

        if (!_vm.ValidarFormulario())
        {
            await DisplayAlertAsync("Datos incompletos",
                "Revise los campos marcados en rojo.\nCada nota debe estar entre 1 y 100.",
                "Entendido");
            return;
        }

        _vm.GuardarEstudianteActual();
        await DisplayAlertAsync("Notas registradas", _vm.ConstruirResumen(), "Aceptar");
    }

    // ---------- DISEÑO ADAPTABLE ----------
    private void AlCambiarTamano(object? sender, EventArgs e)
    {
        if (Width <= 0) return;

        // Panel de resultado a la derecha (PC) o fijo abajo (celular)
        bool panelAlLado = Width >= 900;
        if (_panelAlLado != panelAlLado)
        {
            _panelAlLado = panelAlLado;
            UbicarPanel(panelAlLado);
        }

        // 3 momentos lado a lado solo si hay espacio de verdad
        bool momentosEnFila = Width >= 1250;
        if (_momentosEnFila != momentosEnFila)
        {
            _momentosEnFila = momentosEnFila;
            AcomodarEnColumnas(GridMomentos, momentosEnFila ? 3 : 1);
        }

        // Estudiante y fecha lado a lado o apilados
        bool datosEnFila = Width >= 700;
        if (_datosEnFila != datosEnFila)
        {
            _datosEnFila = datosEnFila;
            AcomodarEnColumnas(GridDatos, datosEnFila ? 2 : 1);
        }
    }

    private void UbicarPanel(bool alLado)
    {
        Raiz.RowDefinitions.Clear();
        Raiz.ColumnDefinitions.Clear();

        if (alLado)
        {
            Raiz.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            Raiz.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            Raiz.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(380)));
            Grid.SetRow(PanelResumen, 0);
            Grid.SetColumn(PanelResumen, 1);
            PanelResumen.Margin = new Thickness(0, 24, 24, 24);
            PanelResumen.VerticalOptions = LayoutOptions.Start;
        }
        else
        {
            Raiz.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            Raiz.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            Raiz.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            Grid.SetRow(PanelResumen, 1);
            Grid.SetColumn(PanelResumen, 0);
            PanelResumen.Margin = new Thickness(12, 0, 12, 12);
            PanelResumen.VerticalOptions = LayoutOptions.Fill;
        }

        // En el celular el panel es compacto: sin desglose ni texto de ayuda
        Desglose.IsVisible = alLado;
        AyudaBoton.IsVisible = alLado;
    }

    private static void AcomodarEnColumnas(Grid grid, int columnas)
    {
        int total = grid.Children.Count;
        int filas = (int)Math.Ceiling(total / (double)columnas);

        grid.ColumnDefinitions.Clear();
        for (int c = 0; c < columnas; c++)
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        grid.RowDefinitions.Clear();
        for (int f = 0; f < filas; f++)
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        for (int i = 0; i < total; i++)
        {
            var hijo = (BindableObject)grid.Children[i];
            Grid.SetColumn(hijo, i % columnas);
            Grid.SetRow(hijo, i / columnas);
        }
    }
}
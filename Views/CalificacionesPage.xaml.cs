using System.ComponentModel;
using CalificacionesSudamericano.Models;
using CalificacionesSudamericano.ViewModels;

namespace CalificacionesSudamericano.Views;

public partial class CalificacionesPage : ContentPage
{
    private readonly CalificacionesViewModel _vm;
    private readonly List<View> _bloques;
    private bool _yaAnimoEntrada;
    private bool? _esPantallaAncha;
    private EstadoAcademico? _ultimoEstado;

    public CalificacionesPage()
    {
        InitializeComponent();

        // Conectamos la pantalla con su "cerebro"
        _vm = new CalificacionesViewModel();
        BindingContext = _vm;

        // Bloques que entrarán animados, en este orden
        _bloques = new List<View> { Encabezado, TarjetaDatos };
        _bloques.AddRange(GridMomentos.Children.OfType<View>());
        _bloques.Add(TarjetaResultado);
        _bloques.Add(BotonCalcular);

        // Empiezan invisibles y un poco más abajo
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
            await Task.Delay(80); // pequeño escalonado entre tarjetas
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

    // La barra se llena suavemente desde donde está hasta el nuevo valor
    private void AnimarBarra(double destino)
    {
        this.AbortAnimation("AnimacionBarra");
        var animacion = new Animation(v => BarraProgreso.Progress = v,
                                      BarraProgreso.Progress, destino, Easing.CubicOut);
        animacion.Commit(this, "AnimacionBarra", length: 600);
    }

    // La etiqueta de estado "salta" cuando cambia
    private async Task AnimarChipAsync()
    {
        await ChipEstado.ScaleToAsync(1.18, 140, Easing.CubicOut);
        await ChipEstado.ScaleToAsync(1.0, 220, Easing.SpringOut);
    }

    // ---------- BOTÓN ----------
    private async void AlPresionarCalcular(object? sender, EventArgs e)
    {
        // Pequeño efecto de "presionado"
        await BotonCalcular.ScaleToAsync(0.96, 80);
        await BotonCalcular.ScaleToAsync(1.0, 80);

        if (!_vm.ValidarFormulario())
        {
            await DisplayAlertAsync("Datos incompletos",
                "Revise los campos marcados en rojo.\nCada nota debe estar entre 1 y 100.",
                "Entendido");
            return;
        }

        await DisplayAlertAsync("Calificaciones del estudiante", _vm.ConstruirResumen(), "Aceptar");
    }

    // ---------- DISEÑO ADAPTABLE ----------
    private void AlCambiarTamano(object? sender, EventArgs e)
    {
        if (Width <= 0) return;

        bool esAncha = Width >= 900;
        if (_esPantallaAncha == esAncha) return; // solo reacomodar si cambió el modo
        _esPantallaAncha = esAncha;

        AcomodarEnColumnas(GridMomentos, esAncha ? 3 : 1); // 3 lado a lado o apiladas
        AcomodarEnColumnas(GridDatos, esAncha ? 2 : 1);
    }

    // Reparte los hijos de un Grid en N columnas
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
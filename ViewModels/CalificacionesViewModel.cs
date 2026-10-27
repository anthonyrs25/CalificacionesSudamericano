using System.ComponentModel;
using CalificacionesSudamericano.Models;
using CalificacionesSudamericano.Services;

namespace CalificacionesSudamericano.ViewModels;

// El "cerebro" de la pantalla completa.
public class CalificacionesViewModel : ViewModelBase
{
    private string? _estudianteSeleccionado;
    private DateTime? _fecha = DateTime.Today;
    private decimal? _notaFinal;
    private string _errorEstudiante = string.Empty;

    public CalificacionesViewModel()
    {
        // Datos fijos del Picker (5 compañeros)
        Estudiantes = new List<string>
        {
            "María Fernanda Quizhpe",
            "Juan Pablo Sarmiento",
            "Daniela Ordóñez",
            "Carlos Andrés Peña",
            "Valeria Cárdenas"
        };

        Momento1 = new MomentoViewModel(1);
        Momento2 = new MomentoViewModel(2);
        Momento3 = new MomentoViewModel(3);

        // Cuando cambia la nota de cualquier momento, recalculamos la final
        Momento1.PropertyChanged += AlCambiarMomento;
        Momento2.PropertyChanged += AlCambiarMomento;
        Momento3.PropertyChanged += AlCambiarMomento;
    }

    public List<string> Estudiantes { get; }
    public MomentoViewModel Momento1 { get; }
    public MomentoViewModel Momento2 { get; }
    public MomentoViewModel Momento3 { get; }

    public string? EstudianteSeleccionado
    {
        get => _estudianteSeleccionado;
        set
        {
            if (SetProperty(ref _estudianteSeleccionado, value) && value is not null)
                ErrorEstudiante = string.Empty; // al elegir uno, se borra el error
        }
    }

    public string ErrorEstudiante
    {
        get => _errorEstudiante;
        private set
        {
            if (SetProperty(ref _errorEstudiante, value))
                OnPropertyChanged(nameof(TieneErrorEstudiante));
        }
    }

    public bool TieneErrorEstudiante => !string.IsNullOrEmpty(ErrorEstudiante);

    public DateTime? Fecha
    {
        get => _fecha;
        set => SetProperty(ref _fecha, value);
    }

    // Cuando cambia la nota final, avisamos a TODO lo que depende de ella
    public decimal? NotaFinal
    {
        get => _notaFinal;
        private set
        {
            if (SetProperty(ref _notaFinal, value))
            {
                OnPropertyChanged(nameof(NotaFinalTexto));
                OnPropertyChanged(nameof(Progreso));
                OnPropertyChanged(nameof(Estado));
                OnPropertyChanged(nameof(EstadoTexto));
                OnPropertyChanged(nameof(ColorEstado));
                OnPropertyChanged(nameof(FondoEstado));
            }
        }
    }

    public string NotaFinalTexto => NotaFinal.HasValue ? NotaFinal.Value.ToString("0.00") : "—";

    // La barra va de 0 a 1
    public double Progreso => NotaFinal.HasValue ? (double)(NotaFinal.Value / 100m) : 0;

    public EstadoAcademico? Estado =>
        NotaFinal.HasValue ? CalculadoraNotas.ObtenerEstado(NotaFinal.Value) : null;

    public string EstadoTexto => Estado?.ToString().ToUpperInvariant() ?? "SIN CALCULAR";

    // Color del texto de la etiqueta y de la barra
    public Color ColorEstado => Estado switch
    {
        EstadoAcademico.Aprobado => Color.FromArgb("#1E8E5A"),
        EstadoAcademico.Complementario => Color.FromArgb("#C98A0B"),
        EstadoAcademico.Reprobado => Color.FromArgb("#D64545"),
        _ => Color.FromArgb("#8A94A6")
    };

    // Fondo suave de la etiqueta
    public Color FondoEstado => Estado switch
    {
        EstadoAcademico.Aprobado => Color.FromArgb("#E3F5EC"),
        EstadoAcademico.Complementario => Color.FromArgb("#FDF3DC"),
        EstadoAcademico.Reprobado => Color.FromArgb("#FBE6E6"),
        _ => Color.FromArgb("#EEF1F5")
    };

    // Se llama al presionar el botón: marca todos los errores pendientes
    public bool ValidarFormulario()
    {
        bool valido = true;

        if (string.IsNullOrEmpty(EstudianteSeleccionado))
        {
            ErrorEstudiante = "Seleccione un estudiante";
            valido = false;
        }

        valido &= Momento1.ValidarTodo();
        valido &= Momento2.ValidarTodo();
        valido &= Momento3.ValidarTodo();
        return valido;
    }

    // Texto del DisplayAlert, con \n para los saltos de línea (lo pide el enunciado)
    public string ConstruirResumen()
    {
        string fecha = (Fecha ?? DateTime.Today).ToString("dd/MM/yyyy");

        return $"Nombre: {EstudianteSeleccionado}\n" +
               $"Fecha: {fecha}\n" +
               $"Nota Momento 1: {Momento1.NotaTexto}\n" +
               $"Nota Momento 2: {Momento2.NotaTexto}\n" +
               $"Nota Momento 3: {Momento3.NotaTexto}\n" +
               $"Nota Final: {NotaFinalTexto}\n" +
               $"Estado: {EstadoTexto}";
    }

    private void AlCambiarMomento(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MomentoViewModel.Nota))
            return;

        if (Momento1.Nota.HasValue && Momento2.Nota.HasValue && Momento3.Nota.HasValue)
            NotaFinal = CalculadoraNotas.CalcularFinal(Momento1.Nota.Value, Momento2.Nota.Value, Momento3.Nota.Value);
        else
            NotaFinal = null;
    }
}
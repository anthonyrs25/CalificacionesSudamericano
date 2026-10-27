using System.ComponentModel;
using CalificacionesSudamericano.Models;
using CalificacionesSudamericano.Services;

namespace CalificacionesSudamericano.ViewModels;

public class CalificacionesViewModel : ViewModelBase
{
    private string? _estudianteSeleccionado;
    private DateTime? _fecha = DateTime.Today;
    private decimal? _notaFinal;
    private string _errorEstudiante = string.Empty;

    // "Archivador" en memoria: guarda las notas de cada estudiante mientras la app está abierta.
    // Clave = nombre del estudiante. Valor = sus 9 notas (como texto) y la fecha.
    private readonly Dictionary<string, (string[] Notas, DateTime? Fecha)> _guardados = new();

    public CalificacionesViewModel()
    {
        Estudiantes = new List<string>
        {
            "Diego Cabrera",
            "Boris Viracocha",
            "Jairo Arévalo",
            "Darío Guzmán",
            "Anthony Rodríguez"
        };

        Momento1 = new MomentoViewModel(1);
        Momento2 = new MomentoViewModel(2);
        Momento3 = new MomentoViewModel(3);

        // Los 9 campos en orden: M1 (clase, autónoma, evaluación), luego M2, luego M3
        TodosLosCampos = Momento1.Campos.Concat(Momento2.Campos).Concat(Momento3.Campos).ToList();

        Momento1.PropertyChanged += AlCambiarMomento;
        Momento2.PropertyChanged += AlCambiarMomento;
        Momento3.PropertyChanged += AlCambiarMomento;
    }

    public List<string> Estudiantes { get; }
    public MomentoViewModel Momento1 { get; }
    public MomentoViewModel Momento2 { get; }
    public MomentoViewModel Momento3 { get; }
    public IReadOnlyList<CampoNotaViewModel> TodosLosCampos { get; }

    public string? EstudianteSeleccionado
    {
        get => _estudianteSeleccionado;
        set
        {
            if (!SetProperty(ref _estudianteSeleccionado, value))
                return;

            ErrorEstudiante = string.Empty;
            CargarEstudiante(value); // vacía el formulario o muestra lo guardado

            OnPropertyChanged(nameof(HayEstudiante));
            OnPropertyChanged(nameof(NoHayEstudiante));
            OnPropertyChanged(nameof(TextoGuardado));
        }
    }

    // Habilita el paso 2 solo cuando hay estudiante elegido
    public bool HayEstudiante => EstudianteSeleccionado is not null;
    public bool NoHayEstudiante => !HayEstudiante;

    public string TextoGuardado =>
        EstudianteSeleccionado is not null && _guardados.ContainsKey(EstudianteSeleccionado)
            ? "Tiene notas guardadas: puede revisarlas o editarlas."
            : "Nuevo registro.";

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
    public double Progreso => NotaFinal.HasValue ? (double)(NotaFinal.Value / 100m) : 0;

    public EstadoAcademico? Estado =>
        NotaFinal.HasValue ? CalculadoraNotas.ObtenerEstado(NotaFinal.Value) : null;

    public string EstadoTexto => Estado?.ToString().ToUpperInvariant() ?? "SIN CALCULAR";

    public Color ColorEstado => Estado switch
    {
        EstadoAcademico.Aprobado => Color.FromArgb("#1E8E5A"),
        EstadoAcademico.Complementario => Color.FromArgb("#C98A0B"),
        EstadoAcademico.Reprobado => Color.FromArgb("#D64545"),
        _ => Color.FromArgb("#8A94A6")
    };

    public Color FondoEstado => Estado switch
    {
        EstadoAcademico.Aprobado => Color.FromArgb("#E3F5EC"),
        EstadoAcademico.Complementario => Color.FromArgb("#FDF3DC"),
        EstadoAcademico.Reprobado => Color.FromArgb("#FBE6E6"),
        _ => Color.FromArgb("#EEF1F5")
    };

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

    // Guarda las 9 notas y la fecha del estudiante actual en el archivador
    public void GuardarEstudianteActual()
    {
        if (EstudianteSeleccionado is null)
            return;

        string[] notas = TodosLosCampos.Select(campo => campo.Texto).ToArray();
        _guardados[EstudianteSeleccionado] = (notas, Fecha);
        OnPropertyChanged(nameof(TextoGuardado));
    }

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

    // Si el estudiante tiene notas guardadas, las muestra; si no, deja todo vacío
    private void CargarEstudiante(string? nombre)
    {
        if (nombre is not null && _guardados.TryGetValue(nombre, out var registro))
        {
            for (int i = 0; i < TodosLosCampos.Count; i++)
                TodosLosCampos[i].Establecer(registro.Notas[i]);
            Fecha = registro.Fecha;
        }
        else
        {
            foreach (var campo in TodosLosCampos)
                campo.Establecer(string.Empty);
            Fecha = DateTime.Today;
        }
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
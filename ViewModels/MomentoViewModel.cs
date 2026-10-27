using System.ComponentModel;
using CalificacionesSudamericano.Services;

namespace CalificacionesSudamericano.ViewModels;

// Una tarjeta "Momento": sus 3 campos + la nota del momento calculada en vivo.
public class MomentoViewModel : ViewModelBase
{
    private decimal? _nota;

    public MomentoViewModel(int numero)
    {
        Titulo = $"Momento {numero}";
        Clase = new CampoNotaViewModel("Tareas en clase", "40 %");
        Autonoma = new CampoNotaViewModel("Tareas autónomas", "40 %");
        Evaluacion = new CampoNotaViewModel("Evaluación", "20 %");

        // Lista para que la tarjeta dibuje los 3 campos con UNA sola plantilla
        Campos = new[] { Clase, Autonoma, Evaluacion };

        // Escuchamos cada campo: cuando su Valor cambie, recalculamos
        foreach (var campo in Campos)
            campo.PropertyChanged += AlCambiarCampo;
    }

    public string Titulo { get; }
    public CampoNotaViewModel Clase { get; }
    public CampoNotaViewModel Autonoma { get; }
    public CampoNotaViewModel Evaluacion { get; }
    public IReadOnlyList<CampoNotaViewModel> Campos { get; }

    public decimal? Nota
    {
        get => _nota;
        private set
        {
            if (SetProperty(ref _nota, value))
                OnPropertyChanged(nameof(NotaTexto));
        }
    }

    // "—" mientras falten datos; con 2 decimales cuando está completo
    public string NotaTexto => Nota.HasValue ? Nota.Value.ToString("0.00") : "—";

    // Valida los 3 campos marcando también los vacíos.
    // "&=" (sin cortocircuito) asegura que se validen TODOS, aunque el primero falle.
    public bool ValidarTodo()
    {
        bool valido = true;
        foreach (var campo in Campos)
            valido &= campo.Validar(mostrarVacio: true);
        return valido;
    }

    private void AlCambiarCampo(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(CampoNotaViewModel.Valor))
            return;

        if (Clase.Valor.HasValue && Autonoma.Valor.HasValue && Evaluacion.Valor.HasValue)
            Nota = CalculadoraNotas.CalcularMomento(Clase.Valor.Value, Autonoma.Valor.Value, Evaluacion.Valor.Value);
        else
            Nota = null;
    }
}
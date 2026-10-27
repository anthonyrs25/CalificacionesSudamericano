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
        // Etiquetas cortas: ahora los 3 campos van en una sola fila
        Clase = new CampoNotaViewModel("Clase", "40 %");
        Autonoma = new CampoNotaViewModel("Autónoma", "40 %");
        Evaluacion = new CampoNotaViewModel("Evaluación", "20 %");

        Campos = new[] { Clase, Autonoma, Evaluacion };

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

    public string NotaTexto => Nota.HasValue ? Nota.Value.ToString("0.00") : "—";

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
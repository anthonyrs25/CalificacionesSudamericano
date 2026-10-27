using System.Globalization;
using CalificacionesSudamericano.Services;

namespace CalificacionesSudamericano.ViewModels;

// Un campo de nota: lo que escribió el usuario, el número entendido y el error.
public class CampoNotaViewModel : ViewModelBase
{
    private string _texto = string.Empty;
    private string _error = string.Empty;

    public CampoNotaViewModel(string etiqueta, string porcentaje)
    {
        Etiqueta = etiqueta;
        Porcentaje = porcentaje;
    }

    public string Etiqueta { get; }
    public string Porcentaje { get; }
    public decimal? Valor { get; private set; }

    public string Texto
    {
        get => _texto;
        set
        {
            if (SetProperty(ref _texto, value ?? string.Empty))
                Validar(mostrarVacio: false);
        }
    }

    public string Error
    {
        get => _error;
        private set
        {
            if (SetProperty(ref _error, value))
                OnPropertyChanged(nameof(TieneError));
        }
    }

    public bool TieneError => !string.IsNullOrEmpty(Error);

    // Pone un texto desde el código (al cambiar de estudiante).
    // Siempre revalida, aunque el texto sea igual, para borrar errores viejos.
    public void Establecer(string texto)
    {
        _texto = texto ?? string.Empty;
        OnPropertyChanged(nameof(Texto));
        Validar(mostrarVacio: false);
    }

    public bool Validar(bool mostrarVacio)
    {
        string limpio = Texto.Trim().Replace(',', '.');

        if (limpio.Length == 0)
        {
            Valor = null;
            Error = mostrarVacio ? "Obligatorio" : string.Empty;
        }
        else if (!decimal.TryParse(limpio, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal numero))
        {
            Valor = null;
            Error = "Solo números";
        }
        else if (numero < CalculadoraNotas.NotaMinima || numero > CalculadoraNotas.NotaMaxima)
        {
            Valor = null;
            Error = "Entre 1 y 100";
        }
        else
        {
            Valor = numero;
            Error = string.Empty;
        }

        OnPropertyChanged(nameof(Valor));
        return Valor.HasValue;
    }
}
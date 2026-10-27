using System.Globalization;
using CalificacionesSudamericano.Services;

namespace CalificacionesSudamericano.ViewModels;

// Un campo de nota: lo que escribió el usuario, el número entendido y el error (si hay).
public class CampoNotaViewModel : ViewModelBase
{
    private string _texto = string.Empty;
    private string _error = string.Empty;

    public CampoNotaViewModel(string etiqueta, string porcentaje)
    {
        Etiqueta = etiqueta;
        Porcentaje = porcentaje;
    }

    // Textos fijos que se muestran en la tarjeta
    public string Etiqueta { get; }
    public string Porcentaje { get; }

    // El número válido (null si está vacío o es inválido)
    public decimal? Valor { get; private set; }

    // Lo que el usuario escribe en el Entry. Cada vez que cambia, se valida.
    public string Texto
    {
        get => _texto;
        set
        {
            if (SetProperty(ref _texto, value ?? string.Empty))
                Validar(mostrarVacio: false); // mientras escribe, un campo vacío no es error
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

    // La pantalla usa esto para pintar el borde rojo y mostrar el mensaje
    public bool TieneError => !string.IsNullOrEmpty(Error);

    // Valida el texto. mostrarVacio = true cuando se presiona "Calcular".
    public bool Validar(bool mostrarVacio)
    {
        // Acepta coma o punto: convertimos la coma en punto
        string limpio = Texto.Trim().Replace(',', '.');

        if (limpio.Length == 0)
        {
            Valor = null;
            Error = mostrarVacio ? "Campo obligatorio" : string.Empty;
        }
        else if (!decimal.TryParse(limpio, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal numero))
        {
            Valor = null;
            Error = "Solo se permiten números";
        }
        else if (numero < CalculadoraNotas.NotaMinima || numero > CalculadoraNotas.NotaMaxima)
        {
            Valor = null;
            Error = "Debe estar entre 1 y 100";
        }
        else
        {
            Valor = numero;
            Error = string.Empty;
        }

        // Avisamos que Valor cambió: el Momento escucha esto para recalcular
        OnPropertyChanged(nameof(Valor));
        return Valor.HasValue;
    }
}
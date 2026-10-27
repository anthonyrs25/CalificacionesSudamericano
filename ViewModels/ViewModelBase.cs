using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CalificacionesSudamericano.ViewModels;

// Clase base: todos los ViewModels heredan de aquí para poder "avisar" a la pantalla.
public abstract class ViewModelBase : INotifyPropertyChanged
{
    // Evento al que la pantalla se suscribe automáticamente (por los Bindings)
    public event PropertyChangedEventHandler? PropertyChanged;

    // Dispara el aviso "cambió la propiedad X" (como setState en React)
    protected void OnPropertyChanged([CallerMemberName] string? nombrePropiedad = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombrePropiedad));
    }

    // Guarda el valor nuevo y avisa SOLO si de verdad cambió. Devuelve true si cambió.
    protected bool SetProperty<T>(ref T campo, T valor, [CallerMemberName] string? nombrePropiedad = null)
    {
        if (EqualityComparer<T>.Default.Equals(campo, valor))
            return false;

        campo = valor;
        OnPropertyChanged(nombrePropiedad);
        return true;
    }
}
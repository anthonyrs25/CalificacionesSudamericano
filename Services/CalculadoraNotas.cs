using CalificacionesSudamericano.Models;

namespace CalificacionesSudamericano.Services;

// static = no se crean objetos de esta clase; se usa directo: CalculadoraNotas.CalcularMomento(...)
public static class CalculadoraNotas
{
    // Rango válido de cada nota
    public const decimal NotaMinima = 1m;
    public const decimal NotaMaxima = 100m;

    // Pesos de cada componente (la "m" indica que es decimal: exacto, sin errores de redondeo)
    private const decimal PesoClase = 0.40m;
    private const decimal PesoAutonoma = 0.40m;
    private const decimal PesoEvaluacion = 0.20m;

    // Nota del momento = Clase × 0,40 + Autónoma × 0,40 + Evaluación × 0,20
    public static decimal CalcularMomento(decimal clase, decimal autonoma, decimal evaluacion)
    {
        decimal nota = clase * PesoClase + autonoma * PesoAutonoma + evaluacion * PesoEvaluacion;
        // AwayFromZero = redondeo escolar (0,005 sube a 0,01)
        return Math.Round(nota, 2, MidpointRounding.AwayFromZero);
    }

    // Nota final = promedio de los 3 momentos
    public static decimal CalcularFinal(decimal momento1, decimal momento2, decimal momento3)
    {
        decimal promedio = (momento1 + momento2 + momento3) / 3m;
        return Math.Round(promedio, 2, MidpointRounding.AwayFromZero);
    }

    // El estado se decide con la nota YA redondeada
    public static EstadoAcademico ObtenerEstado(decimal notaFinal)
    {
        if (notaFinal >= 70m) return EstadoAcademico.Aprobado;
        if (notaFinal >= 50m) return EstadoAcademico.Complementario;
        return EstadoAcademico.Reprobado;
    }
}
using System;

[System.Serializable] // Permite serializar/deserializar desde JSON
public class Usuario
{
    // Campos que coinciden con la base de datos
    public int id_usuario;
    public string nombre_usuario;
    public string fecha_registro;
    public string tiempo_jugado;
    public int veces_ganadas;
    public int derrotas;
    public string ultima_partida;
    public int enemigos_eliminados;

    // Campo de contraseña (¡NO lo uses en el cliente! Solo para ejemplos internos)
    [NonSerialized] // Evita que se incluya en la serialización
    public string contraseña;

    // Constructor vacío (necesario para la deserialización)
    public Usuario() { }
    // Añade tiempo de juego al total acumulado
    public void AgregarTiempoJugado(int segundosAdicionales)
    {
        if (segundosAdicionales < 0)
        {
            return;
        }

        try
        {
            // Parsear el tiempo existente
            string[] partes = tiempo_jugado.Split(':');

            if (partes.Length != 3)
            {
                throw new FormatException("Formato de tiempo inválido");
            }

            int horas = int.Parse(partes[0]);
            int minutos = int.Parse(partes[1]);
            int segundos = int.Parse(partes[2]);

            // Convertir a segundos totales
            int totalSegundos = horas * 3600 + minutos * 60 + segundos;

            // Sumar los nuevos segundos
            totalSegundos += segundosAdicionales;

            // Convertir de vuelta a HH:MM:SS
            horas = totalSegundos / 3600;
            minutos = (totalSegundos % 3600) / 60;
            segundos = totalSegundos % 60;

            // Actualizar el campo
            tiempo_jugado = $"{horas:00}:{minutos:00}:{segundos:00}";
        }
        catch (Exception ex)
        {
            // Si hay error, inicializar con el tiempo actual
            tiempo_jugado = $"{segundosAdicionales / 3600:00}:{(segundosAdicionales / 60) % 60:00}:{segundosAdicionales % 60:00}";
        }
    }

    /// Obtiene el tiempo jugado en segundos
    public int ObtenerTiempoEnSegundos()
    {
        try
        {
            string[] partes = tiempo_jugado.Split(':');
            return int.Parse(partes[0]) * 3600 +
                   int.Parse(partes[1]) * 60 +
                   int.Parse(partes[2]);
        }
        catch
        {
            return 0;
        }
    }


    // Formatea segundos a HH:MM:SS
    public static string FormatearTiempo(int segundos)
    {
        int horas = segundos / 3600;
        int minutos = (segundos % 3600) / 60;
        int segs = segundos % 60;
        return $"{horas:00}:{minutos:00}:{segs:00}";
    }

    public override string ToString()
    {
        return $"ID: {id_usuario}\n" +
               $"Nombre: {nombre_usuario}\n" +
               $"Tiempo jugado: {tiempo_jugado}\n" +
               $"Ganadas: {veces_ganadas}\n" +
               $"Derrotas: {derrotas}\n" +
               $"Enemigos: {enemigos_eliminados}\n" +
               $"Última partida: {ultima_partida}";
    }
}
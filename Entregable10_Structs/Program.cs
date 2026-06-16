using System;

// --- MÓDULO A y C: EL STRUCT CON VALIDACIÓN ---
readonly struct CoordenadaGPS 
{  
    public double Latitud { get; }  
    public double Longitud { get; }

    public CoordenadaGPS(double lat, double lon)  
    {  
        // Validación de rangos geográficos reales de la Tierra
        if (lat < -90 || lat > 90)
            throw new ArgumentOutOfRangeException(nameof(lat), "Latitud fuera de rango [-90, 90]");
            
        if (lon < -180 || lon > 180)   
            throw new ArgumentOutOfRangeException(nameof(lon), "Longitud fuera de rango [-180, 180]");

        Latitud = lat;
        Longitud = lon;  
    }

    public void ImprimirUbicacion()  
    {
        Console.WriteLine($"Latitud: {Latitud}, Longitud: {Longitud}");
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("=== MÓDULO C: SISTEMA DE TELEMETRÍA SEGURO ===\n");
        
        try
        {  
            Console.Write("Latitud: ");  
            double lat = double.Parse(Console.ReadLine() ?? "0");  
            
            Console.Write("Longitud: ");
            double lon = double.Parse(Console.ReadLine() ?? "0");
            
            // Si los datos son inválidos, el struct lanzará el error antes de crearse
            var coord = new CoordenadaGPS(lat, lon);
            
            Console.WriteLine("\n✅ Coordenada registrada con éxito:");
            coord.ImprimirUbicacion(); 
        } 
        catch (ArgumentOutOfRangeException ex) 
        {  
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ ERROR DE TELEMETRÍA: {ex.Message}");
            Console.ResetColor();
        }
        catch (FormatException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n❌ ERROR: Por favor ingresa números válidos.");
            Console.ResetColor();
        }
    }
}
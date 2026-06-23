using System;
using System.Collections.Generic;
using System.Linq;

// 1. PRIMERO: Struct Inmutable (PuntoDeRed)
public readonly struct PuntoDeRed 
{
    public double Latitud { get; }
    public double Longitud { get; }

    public PuntoDeRed(double lat, double lon) 
    {
        if (lat < -90 || lat > 90) throw new ArgumentOutOfRangeException(nameof(lat), "Latitud fuera de rango.");
        if (lon < -180 || lon > 180) throw new ArgumentOutOfRangeException(nameof(lon), "Longitud fuera de rango.");
        Latitud = lat;
        Longitud = lon;
    }
    public override string ToString() => $"Lat: {Latitud}, Lon: {Longitud}";
}

// 2. SEGUNDO: Clase Principal (ServidorConexion)
public class ServidorConexion 
{
    public int ID { get; set; }
    public string Nombre { get; set; }
    public PuntoDeRed Ubicacion { get; set; }
    public List<int> CodigosRespuesta { get; set; }

    private readonly long[] _cache = new long[100];

    public ServidorConexion(int id, string nombre, PuntoDeRed ubicacion, List<int> codigos)
    {
        ID = id; Nombre = nombre; Ubicacion = ubicacion; CodigosRespuesta = codigos;
    }

    public long DiagnosticarLatencia(int n, out string alerta) 
    {
        if (n < 0 || n >= 100) throw new ArgumentOutOfRangeException(nameof(n), "n debe estar entre 0 y 99.");
        if (n <= 1) { alerta = string.Empty; return n; }
        
        if (_cache[n] != 0) { alerta = string.Empty; return _cache[n]; }
        
        _cache[n] = DiagnosticarLatencia(n - 1, out _) + DiagnosticarLatencia(n - 2, out _);
        
        if (_cache[n] > 10000) alerta = "ALERTA: Índice de estrés crítico en " + Nombre;
        else alerta = string.Empty;
        
        return _cache[n];
    }
    
    public override string ToString() => $"{Nombre} ({Ubicacion})";
}

// 3. TERCERO: Orquestador y Consultas LINQ
public class Program 
{
    public static void Main() 
    {
        try 
        {
            Console.WriteLine("=== SISTEMA DE MONITOREO DE RED ===");
            Console.Write("Ingresa la latitud del servidor CDMX: ");
            
            string? input = Console.ReadLine();
            
            if (!double.TryParse(input, out double latitud)) 
                throw new FormatException("No ingresaste un número válido.");
            
            var punto = new PuntoDeRed(latitud, -99.1);
            var srv1 = new ServidorConexion(1, "Servidor-CDMX", punto, new List<int> { 200, 500 });
            var srv2 = new ServidorConexion(2, "Servidor-Sydney", new PuntoDeRed(-33.8, 151.2), new List<int> { 500, 404 });
            
            var listaServidores = new List<ServidorConexion> { srv1, srv2 };
            
            var criticos = listaServidores.Where(s => s.Ubicacion.Latitud > 0 && s.CodigosRespuesta.Contains(500)).ToList();
            
            Console.WriteLine("\n=== SERVIDORES CRÍTICOS ===");
            criticos.ForEach(s => Console.WriteLine(s));

            long estres = srv1.DiagnosticarLatencia(25, out string alerta);
            if (!string.IsNullOrEmpty(alerta)) Console.WriteLine($"\n{alerta}");
            else Console.WriteLine($"\nÍndice de estrés: {estres}");
        }
        catch (Exception ex) 
        {
            Console.WriteLine($"\n[ERROR DETECTADO] {ex.Message}");
        }
    }
}
using System;
using System.Diagnostics;

readonly struct RegistroDatos
{
    public int Id { get; }
    public string HashValidacion { get; }
    public int PesoBytes { get; }

    public RegistroDatos(int id, string hashValidacion, int pesoBytes)
    {
        if (id <= 0) throw new ArgumentException("Id debe ser mayor que cero", nameof(id));
        if (string.IsNullOrWhiteSpace(hashValidacion)) throw new ArgumentException("HashValidacion no puede ser nulo o vacío", nameof(hashValidacion));
        if (pesoBytes <= 0) throw new ArgumentException("PesoBytes debe ser mayor que cero", nameof(pesoBytes));

        Id = id;
        HashValidacion = hashValidacion;
        PesoBytes = pesoBytes;
    }

    public override string ToString() => $"Id={Id},Hash={HashValidacion},Peso={PesoBytes}";
}

class Program
{
    // SelectionSort instrumentado: cuenta comparaciones, swaps y devuelve tiempo en ms
    static void SelectionSort(RegistroDatos[] arr, out long comparaciones, out long intercambios, out long elapsedMilliseconds)
    {
        comparaciones = 0;
        intercambios = 0;
        var sw = Stopwatch.StartNew();

        int n = arr.Length;
        for (int i = 0; i < n - 1; i++)
        {
            int min = i;
            for (int j = i + 1; j < n; j++)
            {
                comparaciones++; // contamos cada comparación de claves
                if (arr[j].Id < arr[min].Id)
                    min = j;
            }

            if (min != i)
            {
                // intercambio elegante sin variable temporal
                (arr[i], arr[min]) = (arr[min], arr[i]);
                intercambios++;
            }
        }

        sw.Stop();
        elapsedMilliseconds = sw.ElapsedMilliseconds;
    }

    static void PrintSample(RegistroDatos[] arr, int sampleSize = 10)
    {
        int n = Math.Min(arr.Length, sampleSize);
        for (int i = 0; i < n; i++)
        {
            Console.WriteLine($"[{i}] {arr[i]}");
        }
        if (arr.Length > sampleSize)
            Console.WriteLine($"... (mostrando {sampleSize} de {arr.Length} elementos)");
    }

    static void Main()
    {
        RegistroDatos[] datos;
        try
        {
            var rnd = new Random();
            int n = 40; // exactamente 40 registros según la rúbrica
            datos = new RegistroDatos[n];

            for (int i = 0; i < n; i++)
            {
                int id = rnd.Next(1, 10_001); // Id > 0
                string hash = Guid.NewGuid().ToString("N").Substring(0, 16); // Hash no nulo
                int peso = rnd.Next(1, 1_000_001); // PesoBytes > 0

                datos[i] = new RegistroDatos(id, hash, peso);
            }
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine("Error creando registros: " + ex.Message);
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error inesperado creando registros: " + ex.Message);
            return;
        }

        Console.WriteLine("Antes (muestra parcial):");
        PrintSample(datos, sampleSize: 10);

        SelectionSort(datos, out long comparaciones, out long intercambios, out long ms);

        Console.WriteLine("\nDespués (muestra parcial):");
        PrintSample(datos, sampleSize: 10);

        Console.WriteLine("\nMétricas:");
        Console.WriteLine($"Comparaciones: {comparaciones}");
        Console.WriteLine($"Intercambios: {intercambios}");
        Console.WriteLine($"Tiempo (ms): {ms}");
    }
}
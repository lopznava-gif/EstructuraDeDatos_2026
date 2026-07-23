using System;

struct Transaccion
{
    public int Id;
    public override string ToString() => $"Transacción ID: {Id}";
}

class Program
{
    static void Main()
    {
        Console.WriteLine("=== OPTIMIZADOR DE BITÁCORAS (INSERTION SORT) ===\n");
        
        try
        {
            // Ajusta aquí el tamaño real del arreglo
            int n = 50; // <-- si querías 50 transacciones, pon 50; antes usabas 9
            Transaccion[] bitacora = new Transaccion[n];
            Random rng = Random.Shared;
            
            for (int i = 0; i < bitacora.Length; i++)
            {
                bitacora[i] = new Transaccion { Id = rng.Next(1, 1000) };
            }

            int totalDesplazamientos = 0;
            int totalComparaciones = 0;

            // Algoritmo Insertion Sort
            for (int i = 1; i < bitacora.Length; i++)
            {
                Transaccion clave = bitacora[i];
                int j = i - 1;

                // Desplazamiento de elementos mayores que la clave
                while (j >= 0)
                {
                    totalComparaciones++;
                    if (bitacora[j].Id > clave.Id)
                    {
                        bitacora[j + 1] = bitacora[j];
                        j--;
                        totalDesplazamientos++;
                    }
                    else
                    {
                        break;
                    }
                }
                // Inserción en la posición correcta (j + 1)
                bitacora[j + 1] = clave;
            }

            Console.WriteLine("Transacciones ordenadas por ID:");
            foreach (var t in bitacora)
            {
                Console.WriteLine(t);
            }

            Console.WriteLine($"\nTotal de desplazamientos realizados: {totalDesplazamientos}");
            Console.WriteLine($"Total de comparaciones realizadas: {totalComparaciones}");

            // Cálculo del porcentaje de eficiencia respecto al peor caso (n*(n-1)/2)
            double peorCaso = (double)bitacora.Length * (bitacora.Length - 1) / 2.0;
            double eficiencia;
            if (peorCaso > 0)
                eficiencia = (1.0 - (double)totalDesplazamientos / peorCaso) * 100.0;
            else
                eficiencia = 100.0; // n < 2 => trivialmente "100% mejor" que el peor caso vacío

            Console.WriteLine($"Eficiencia: {eficiencia:F1}% mejor que el peor caso.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR inesperado]: {ex.Message}");
        }
    }
}
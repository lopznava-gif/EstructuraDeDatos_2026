using System;
using System.Diagnostics;
using System.Text;

// Programa principal que compara QuickSort (recursivo, Lomuto, pivote central)
// contra SelectionSort (Fase 1) mediante un benchmark reproducible.
#nullable enable

// RegistroDatos: readonly struct inmutable de la Fase 1.
// Valida que Id > 0, HashValidacion no nulo/ vacío y PesoBytes > 0.
public readonly struct RegistroDatos
{
    public int Id { get; }
    public string HashValidacion { get; }
    public int PesoBytes { get; }

    public RegistroDatos(int id, string hashValidacion, int pesoBytes)
    {
        if (id <= 0) throw new ArgumentException("Id debe ser mayor que cero", nameof(id));
        if (string.IsNullOrWhiteSpace(hashValidacion)) throw new ArgumentException("HashValidacion no puede ser nulo ni vacío", nameof(hashValidacion));
        if (pesoBytes <= 0) throw new ArgumentException("PesoBytes debe ser mayor que cero", nameof(pesoBytes));

        Id = id;
        HashValidacion = hashValidacion;
        PesoBytes = pesoBytes;
    }

    public override string ToString() => $"Id={Id},Hash={HashValidacion},Peso={PesoBytes}";
}

class Program
{
    // ----------------------------
    // Métricas acumuladas QuickSort
    // ----------------------------
    // Variables que almacenan métricas medidas durante QuickSort.
    // Se usan por simplicidad como out/retorno desde la función pública que invoca la recursiva.
    private static long qsComparaciones;
    private static long qsSwaps;
    private static long qsRecursivas;

    // QuickSort público: arranque y medición de llamadas recursivas.
    public static void QuickSort(RegistroDatos[] arr)
    {
        qsComparaciones = 0;
        qsSwaps = 0;
        qsRecursivas = 0;

        QuickSortRec(arr, 0, arr.Length - 1);
    }

    // QuickSort recursivo con particionado de Lomuto.
    // Selección de pivote: elemento central -> movido al final -> particionado por Lomuto.
    private static void QuickSortRec(RegistroDatos[] arr, int low, int high)
    {
        // Contamos cada entrada a la función recursiva
        qsRecursivas++;

        if (low < high)
        {
            // Escoger el pivote central para reducir probabilidad de peor caso en arreglos ordenados.
            int pivotIndex = low + ((high - low) / 2);

            // Mover pivote al final para usar el esquema de Lomuto
            (arr[pivotIndex], arr[high]) = (arr[high], arr[pivotIndex]);
            qsSwaps++;

            // Pivot value (comparamos por Id)
            int pivotValue = arr[high].Id;

            // Lomuto partition
            int i = low;
            for (int j = low; j <= high - 1; j++)
            {
                qsComparaciones++;
                if (arr[j].Id <= pivotValue)
                {
                    (arr[i], arr[j]) = (arr[j], arr[i]); // intercambio in-place con tuplas
                    qsSwaps++;
                    i++;
                }
            }

            // Colocar pivote en su posición final
            (arr[i], arr[high]) = (arr[high], arr[i]);
            qsSwaps++;

            // Recursión en subarreglos
            QuickSortRec(arr, low, i - 1);
            QuickSortRec(arr, i + 1, high);
        }
    }

    // Devuelve las métricas acumuladas de QuickSort
    public static (long Comparaciones, long Swaps, long Recursivas) GetQuickSortMetrics()
        => (qsComparaciones, qsSwaps, qsRecursivas);

    // ----------------------------
    // SelectionSort (Fase 1)
    // ----------------------------
    // Implementación instrumentada: cuenta comparaciones e intercambios.
    public static void SelectionSort(RegistroDatos[] arr, out long comparaciones, out long swaps)
    {
        comparaciones = 0;
        swaps = 0;

        int n = arr.Length;
        for (int i = 0; i < n - 1; i++)
        {
            int min = i;
            for (int j = i + 1; j < n; j++)
            {
                comparaciones++;
                if (arr[j].Id < arr[min].Id)
                    min = j;
            }

            if (min != i)
            {
                (arr[i], arr[min]) = (arr[min], arr[i]); // intercambio por tuplas
                swaps++;
            }
        }
    }

    // ----------------------------
    // Utilidades
    // ----------------------------
    // Valida que el arreglo esté ordenado por Id ascendente.
    public static bool EstaOrdenado(RegistroDatos[] arr)
    {
        for (int i = 0; i + 1 < arr.Length; i++)
        {
            if (arr[i].Id > arr[i + 1].Id) return false;
        }
        return true;
    }

    // Genera un hash pseudoaleatorio reproducible de longitud 'len' usando Random dado.
    private static string RandomHash(Random rnd, int len = 16)
    {
        // Generamos bytes y los convertimos a hex; esto es determinista dado el Random con semilla fija.
        int byteLen = (len + 1) / 2;
        var bytes = new byte[byteLen];
        rnd.NextBytes(bytes);
        var hex = new StringBuilder(byteLen * 2);
        foreach (var b in bytes) hex.Append(b.ToString("x2"));
        // Aseguramos longitud exacta (trim o pad si es necesario)
        if (hex.Length > len) return hex.ToString(0, len);
        if (hex.Length < len) return hex.Append('0', len - hex.Length).ToString();
        return hex.ToString();
    }

    // Formatea y muestra un reporte tabular limpio en consola.
    private static void ImprimirReporte(
        int elementos,
        long selComparaciones, long selSwaps, long selTimeMs,
        long qsComparaciones, long qsSwaps, long qsRecursivas, long qsTimeMs)
    {
        // Cabecera
        Console.WriteLine();
        Console.WriteLine("Comparativa de algoritmos (ordenamiento por Id)");
        Console.WriteLine(new string('-', 92));
        Console.WriteLine("{0,-12} | {1,8} | {2,14} | {3,10} | {4,14} | {5,10}", "Algoritmo", "Elementos", "Comparaciones", "Swaps", "Recursivas", "Tiempo(ms)");
        Console.WriteLine(new string('-', 92));

        // Selección
        Console.WriteLine("{0,-12} | {1,8} | {2,14:N0} | {3,10:N0} | {4,14} | {5,10:N0}",
            "Selection", elementos, selComparaciones, selSwaps, "N/A", selTimeMs);

        // QuickSort
        Console.WriteLine("{0,-12} | {1,8} | {2,14:N0} | {3,10:N0} | {4,14:N0} | {5,10:N0}",
            "QuickSort", elementos, qsComparaciones, qsSwaps, qsRecursivas, qsTimeMs);

        Console.WriteLine(new string('-', 92));

        // Ratio de aceleración: SelectionTime / QuickTime
        double ratio = qsTimeMs == 0 ? double.PositiveInfinity : (double)selTimeMs / qsTimeMs;
        Console.WriteLine("Ratio de aceleración (SelectionTime / QuickTime): {0:N2}x", ratio);
        Console.WriteLine();
    }

    // ----------------------------
    // Main - Orquestador de benchmark
    // ----------------------------
    static void Main()
    {
        const int N = 10_000; // exactamente 10,000 elementos según la rúbrica
        RegistroDatos[] original = new RegistroDatos[N];

        // Semilla fija para reproducibilidad científica
        var rnd = new Random(42);

        // Generación de datos dentro de try-catch para manejar posibles invalidaciones.
        try
        {
            for (int i = 0; i < N; i++)
            {
                // Id > 0
                int id = rnd.Next(1, 1_000_001);

                // Hash reproducible generado desde Random
                string hash = RandomHash(rnd, 16);

                // PesoBytes > 0
                int peso = rnd.Next(1, 1_000_001);

                original[i] = new RegistroDatos(id, hash, peso);
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

        // Clonar el arreglo original para asegurar condiciones idénticas
        // Ambos clones provienen estrictamente del mismo original.
        var arrForQuick = (RegistroDatos[])original.Clone();
        var arrForSelection = (RegistroDatos[])original.Clone();

        // WARMUP / estabilización (opcional prudente): forrar JIT y minimizar ruido de primera invocación.
        // Ejecutamos una operación ligera para reducir jitter inicial.
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // ----------------------------
        // Ejecutar QuickSort y medir
        // ----------------------------
        var sw = Stopwatch.StartNew();
        QuickSort(arrForQuick);
        sw.Stop();
        long quickMs = sw.ElapsedMilliseconds;
        var (qsComp, qsSwp, qsRec) = GetQuickSortMetrics();

        // Validar que QuickSort dejó el arreglo ordenado
        bool quickOrdenado = EstaOrdenado(arrForQuick);
        if (!quickOrdenado)
        {
            Console.WriteLine("ERROR: QuickSort no ordenó correctamente.");
            return;
        }

        // ----------------------------
        // Ejecutar SelectionSort y medir
        // ----------------------------
        // Para reducir efectos secundarios del primer benchmark (GC, JIT), volvemos a forzar
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        sw.Restart();
        SelectionSort(arrForSelection, out long selComp, out long selSwp);
        sw.Stop();
        long selMs = sw.ElapsedMilliseconds;

        // Validar SelectionSort
        bool selOrdenado = EstaOrdenado(arrForSelection);
        if (!selOrdenado)
        {
            Console.WriteLine("ERROR: SelectionSort no ordenó correctamente.");
            return;
        }

        // ----------------------------
        // Reporte
        // ----------------------------
        ImprimirReporte(N, selComp, selSwp, selMs, qsComp, qsSwp, qsRec, quickMs);

        // Mensaje final con confirmación reproducible
        Console.WriteLine("Semilla usada: 42. Arreglo original y clones generados reproduciblemente con Random(42).");
        Console.WriteLine("Validación: QuickSort ordenado = {0}, SelectionSort ordenado = {1}", quickOrdenado, selOrdenado);
    }
}
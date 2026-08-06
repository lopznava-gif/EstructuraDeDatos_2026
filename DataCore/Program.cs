using System;
using System.Diagnostics;
using System.Text;

#nullable enable

// Programa integrado: RegistroDatos, Lista Dinámica, QuickSort/SelectionSort instrumentados
// y orquestador de benchmark reproducible (10,000 elementos, Random(42)).

namespace DataCoreIntegrated
{
    // -----------------------
    // Modelo de dato inmutable
    // -----------------------
    /// <summary>
    /// RegistroDatos: readonly struct inmutable reutilizable en Fase 1/2/3.
    /// Validaciones: Id > 0, HashValidacion no nulo/ vacío, PesoBytes > 0.
    /// </summary>
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

        public override string ToString() => $"Id={Id,6} | Hash={HashValidacion} | Peso={PesoBytes}B";
    }

    // -----------------------
    // Nodo y Tabla Dinámica
    // -----------------------
    /// <summary> Nodo de la lista enlazada (tipo referencia). </summary>
    public class NodoRegistro
    {
        public RegistroDatos Dato { get; set; }
        public NodoRegistro? Siguiente { get; set; }

        public NodoRegistro(RegistroDatos dato)
        {
            Dato = dato;
            Siguiente = null;
        }
    }

    /// <summary>
    /// TablaDinamica: lista simplemente enlazada con métodos defensivos y utilidades.
    /// </summary>
    public class TablaDinamica
    {
        private NodoRegistro? cabeza;
        private int contadorRegistros;

        public int Cantidad => contadorRegistros;

        public TablaDinamica()
        {
            cabeza = null;
            contadorRegistros = 0;
        }

        public void InsertarInicio(RegistroDatos nuevoRegistro)
        {
            var nuevoNodo = new NodoRegistro(nuevoRegistro) { Siguiente = cabeza };
            cabeza = nuevoNodo;
            contadorRegistros++;
        }

        public void InsertarFinal(RegistroDatos nuevoRegistro)
        {
            var nuevoNodo = new NodoRegistro(nuevoRegistro);
            if (cabeza == null)
            {
                cabeza = nuevoNodo;
            }
            else
            {
                var actual = cabeza;
                while (actual.Siguiente != null) actual = actual.Siguiente;
                actual.Siguiente = nuevoNodo;
            }
            contadorRegistros++;
        }

        // Eliminación defensiva por Id. Retorna true si se eliminó, false si no se encontró o id inválido.
        public bool EliminarPorId(int idTarget)
        {
            if (idTarget <= 0) return false;
            if (cabeza == null) return false;

            if (cabeza.Dato.Id == idTarget)
            {
                cabeza = cabeza.Siguiente;
                contadorRegistros--;
                return true;
            }

            var anterior = cabeza;
            var actual = cabeza.Siguiente;
            while (actual != null)
            {
                if (actual.Dato.Id == idTarget)
                {
                    anterior.Siguiente = actual.Siguiente;
                    contadorRegistros--;
                    return true;
                }
                anterior = actual;
                actual = actual.Siguiente;
            }

            return false;
        }

        // Copia la lista a un arreglo compacto, corrige contador si detecta inconsistencia.
        public RegistroDatos[] ObtenerComoArreglo()
        {
            if (contadorRegistros == 0) return Array.Empty<RegistroDatos>();

            var resultado = new RegistroDatos[contadorRegistros];
            var actual = cabeza;
            int i = 0;
            while (actual != null && i < resultado.Length)
            {
                resultado[i++] = actual.Dato;
                actual = actual.Siguiente;
            }

            if (i == resultado.Length) return resultado;

            // inconsistencia detectada: reconstruir y corregir contador de forma defensiva
            var lista = new System.Collections.Generic.List<RegistroDatos>(i + 4);
            for (int k = 0; k < i; k++) lista.Add(resultado[k]);
            while (actual != null)
            {
                lista.Add(actual.Dato);
                actual = actual.Siguiente;
            }

            contadorRegistros = lista.Count;
            return lista.ToArray();
        }

        // Vacía la lista (quita referencias para ayudar al GC)
        public void Clear()
        {
            cabeza = null;
            contadorRegistros = 0;
        }
    }

    // -----------------------
    // Instrumentación y sort
    // -----------------------
    /// <summary> Contenedor de métricas devueltas por los algoritmos. </summary>
    public readonly struct SortMetrics
    {
        public long Comparaciones { get; }
        public long Swaps { get; }
        public long Recursivas { get; } // útil solo para QuickSort
        public long TiempoMs { get; }

        public SortMetrics(long comparaciones, long swaps, long recursivas, long tiempoMs)
        {
            Comparaciones = comparaciones;
            Swaps = swaps;
            Recursivas = recursivas;
            TiempoMs = tiempoMs;
        }
    }

    public static class Sorter
    {
        // QuickSort instrumentado (recursivo, Lomuto, pivote central).
        public static SortMetrics QuickSort(RegistroDatos[] arr)
        {
            long comps = 0;
            long swaps = 0;
            long recursivas = 0;

            void QuickRec(int low, int high)
            {
                recursivas++;
                if (low < high)
                {
                    int pivotIndex = Particionar(arr, low, high, ref comps, ref swaps);
                    QuickRec(low, pivotIndex - 1);
                    QuickRec(pivotIndex + 1, high);
                }
            }

            var sw = Stopwatch.StartNew();
            QuickRec(0, arr.Length - 1);
            sw.Stop();
            return new SortMetrics(comps, swaps, recursivas, sw.ElapsedMilliseconds);
        }

        // Particionado de Lomuto con pivote central movido al final.
        private static int Particionar(RegistroDatos[] arr, int low, int high, ref long comps, ref long swaps)
        {
            int medio = low + (high - low) / 2;
            // mover pivote central al final (swap in-place con tuplas)
            (arr[medio], arr[high]) = (arr[high], arr[medio]);
            swaps++;

            int pivotValue = arr[high].Id;
            int i = low - 1;
            for (int j = low; j < high; j++)
            {
                comps++;
                if (arr[j].Id <= pivotValue)
                {
                    i++;
                    (arr[i], arr[j]) = (arr[j], arr[i]);
                    swaps++;
                }
            }

            (arr[i + 1], arr[high]) = (arr[high], arr[i + 1]);
            swaps++;
            return i + 1;
        }

        // SelectionSort instrumentado (desde Fase 1)
        public static SortMetrics SelectionSort(RegistroDatos[] arr)
        {
            long comps = 0;
            long swaps = 0;
            var sw = Stopwatch.StartNew();

            int n = arr.Length;
            for (int i = 0; i < n - 1; i++)
            {
                int min = i;
                for (int j = i + 1; j < n; j++)
                {
                    comps++;
                    if (arr[j].Id < arr[min].Id) min = j;
                }
                if (min != i)
                {
                    (arr[i], arr[min]) = (arr[min], arr[i]);
                    swaps++;
                }
            }

            sw.Stop();
            return new SortMetrics(comps, swaps, 0, sw.ElapsedMilliseconds);
        }

        // Valida orden ascendente por Id
        public static bool EstaOrdenado(RegistroDatos[] arr)
        {
            for (int i = 0; i + 1 < arr.Length; i++)
            {
                if (arr[i].Id > arr[i + 1].Id) return false;
            }
            return true;
        }
    }

    // -----------------------
    // Utilidades
    // -----------------------
    public static class Utils
    {
        // Hash reproducible usando Random dado
        public static string RandomHash(Random rnd, int len = 16)
        {
            int byteLen = (len + 1) / 2;
            var bytes = new byte[byteLen];
            rnd.NextBytes(bytes);
            var hex = new StringBuilder(byteLen * 2);
            foreach (var b in bytes) hex.Append(b.ToString("x2"));
            if (hex.Length > len) return hex.ToString(0, len);
            if (hex.Length < len) return hex.Append('0', len - hex.Length).ToString();
            return hex.ToString();
        }

        // Imprime una muestra parcial (primeros sampleSize elementos)
        public static void PrintSample(RegistroDatos[] arr, int sampleSize = 10)
        {
            int n = Math.Min(arr.Length, sampleSize);
            for (int i = 0; i < n; i++) Console.WriteLine($"[{i,3}] {arr[i]}");
            if (arr.Length > sampleSize) Console.WriteLine($"... (mostrando {sampleSize} de {arr.Length} elementos)");
        }

        // Imprime reporte tabulado comparativo
        public static void ImprimirReporte(int elementos, SortMetrics sel, SortMetrics qs)
        {
            Console.WriteLine();
            Console.WriteLine("Comparativa de algoritmos (ordenamiento por Id)");
            Console.WriteLine(new string('-', 100));
            Console.WriteLine("{0,-12} | {1,9} | {2,16} | {3,10} | {4,12} | {5,12}", "Algoritmo", "Elementos", "Comparaciones", "Swaps", "Recursivas", "Tiempo(ms)");
            Console.WriteLine(new string('-', 100));

            Console.WriteLine("{0,-12} | {1,9} | {2,16:N0} | {3,10:N0} | {4,12} | {5,12:N0}",
                "Selection", elementos, sel.Comparaciones, sel.Swaps, "N/A", sel.TiempoMs);

            Console.WriteLine("{0,-12} | {1,9} | {2,16:N0} | {3,10:N0} | {4,12:N0} | {5,12:N0}",
                "QuickSort", elementos, qs.Comparaciones, qs.Swaps, qs.Recursivas, qs.TiempoMs);

            Console.WriteLine(new string('-', 100));
            double ratio = qs.TiempoMs == 0 ? double.PositiveInfinity : (double)sel.TiempoMs / qs.TiempoMs;
            Console.WriteLine("Ratio de aceleración (SelectionTime / QuickTime): {0:N2}x", ratio);
            Console.WriteLine();
        }
    }

    // -----------------------
    // Main: demo + benchmark
    // -----------------------
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== DataCore Integrated: Demo de TablaDinamica y Benchmark QuickSort vs SelectionSort ===\n");

            // -----------------------
            // DEMO: TablaDinamica (Fase 3)
            // -----------------------
            var demoRnd = new Random(42); // semilla fija para demo
            var tabla = new TablaDinamica();

            Console.WriteLine("Demo TablaDinamica: insertando 15 registros (IDs 1..15)...");
            for (int i = 1; i <= 15; i++)
            {
                var reg = new RegistroDatos(i, Utils.RandomHash(demoRnd, 12), demoRnd.Next(10, 5001));
                tabla.InsertarFinal(reg);
                Console.WriteLine($"[INSERT] ID={i} insertado en lista dinámica.");
            }

            Console.WriteLine("\nEliminando IDs 5 y 11 (si existen)...");
            bool e5 = tabla.EliminarPorId(5);
            bool e11 = tabla.EliminarPorId(11);
            Console.WriteLine($"Eliminar 5: {(e5 ? "OK" : "No encontrado")}; Eliminar 11: {(e11 ? "OK" : "No encontrado")}");

            var arregloDemo = tabla.ObtenerComoArreglo();
            Console.WriteLine($"\nArreglo obtenido desde TablaDinamica: {arregloDemo.Length} elementos. (Demo)");
            Utils.PrintSample(arregloDemo, 10);

            Console.WriteLine("\nOrdenando arreglo demo con QuickSort...");
            var demoQsMetrics = Sorter.QuickSort(arregloDemo);
            Console.WriteLine($"Demo QuickSort: tiempo {demoQsMetrics.TiempoMs} ms, comparaciones {demoQsMetrics.Comparaciones:N0}, swaps {demoQsMetrics.Swaps:N0}, recursivas {demoQsMetrics.Recursivas:N0}");
            Console.WriteLine("Estado ordenado: " + (Sorter.EstaOrdenado(arregloDemo) ? "Sí" : "NO"));

            Console.WriteLine("\n--- Fin demo TablaDinamica ---\n");

            // -----------------------
            // BENCHMARK: QuickSort vs SelectionSort (Fase 2)
            // -----------------------
            const int N = 10_000; // exactamente 10,000 elementos según la rúbrica
            var rnd = new Random(42); // semilla fija para reproducibilidad científica
            var original = new RegistroDatos[N];

            Console.WriteLine($"Generando {N:N0} registros aleatorios con Random(42)...");
            for (int i = 0; i < N; i++)
            {
                int id = rnd.Next(1, 1_000_001);
                string hash = Utils.RandomHash(rnd, 16);
                int peso = rnd.Next(1, 1_000_001);
                original[i] = new RegistroDatos(id, hash, peso);
            }

            Console.WriteLine("\nMuestra parcial del arreglo original (primeros 10):");
            Utils.PrintSample(original, 10);

            // Duplicar con Clone() para condiciones idénticas
            var arrForQuick = (RegistroDatos[])original.Clone();
            var arrForSelection = (RegistroDatos[])original.Clone();

            // Warm-up/estabilización: reducir jitter de JIT y GC
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Console.WriteLine("\nEjecutando QuickSort (instrumentado)...");
            var qsMetrics = Sorter.QuickSort(arrForQuick);

            // validar orden
            bool quickOrdenado = Sorter.EstaOrdenado(arrForQuick);
            Console.WriteLine($"QuickSort ordenado: {quickOrdenado} (tiempo {qsMetrics.TiempoMs} ms)");

            if (!quickOrdenado)
            {
                Console.WriteLine("ERROR: QuickSort no ordenó correctamente. Abortando benchmark.");
                return;
            }

            // Otro GC antes de Selection para reducir efectos de ruido
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Console.WriteLine("\nEjecutando SelectionSort (instrumentado)...");
            var selMetrics = Sorter.SelectionSort(arrForSelection);
            bool selOrdenado = Sorter.EstaOrdenado(arrForSelection);
            Console.WriteLine($"SelectionSort ordenado: {selOrdenado} (tiempo {selMetrics.TiempoMs} ms)");

            if (!selOrdenado)
            {
                Console.WriteLine("ERROR: SelectionSort no ordenó correctamente. Abortando benchmark.");
                return;
            }

            Console.WriteLine("\nMuestra parcial del arreglo resultado QuickSort (primeros 10):");
            Utils.PrintSample(arrForQuick, 10);

            // Imprimir reporte comparativo tabulado
            Utils.ImprimirReporte(N, selMetrics, qsMetrics);

            Console.WriteLine("Semilla usada: 42. Ambos algoritmos corrieron sobre clones idénticos del arreglo original.");

            Console.WriteLine("\nProceso completo.");
        }
    }
}
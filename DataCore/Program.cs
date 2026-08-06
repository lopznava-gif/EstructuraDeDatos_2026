// Program.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

#nullable enable

namespace DataCoreEngine
{
    // =========================================================================
    // 1. MODELO DE DATOS INMUTABLE
    // =========================================================================
    /// <summary>
    /// RegistroDatos: readonly struct inmutable.
    /// Nota: RegistroDatos es un tipo valor (struct) y se copia por valor. Su
    /// ubicación física (stack o heap) depende del contexto: variables locales
    /// pequeñas normalmente residen en el stack, mientras que cuando forman
    /// parte de un arreglo o de una referencia, sus bytes se incrustan en el heap.
    /// Esto evita afirmaciones absolutas sobre "vive en el stack" y explica por qué
    /// trabajar con arreglos de structs cambia la localización física.
    /// </summary>
    public readonly struct RegistroDatos
    {
        /// <summary>Identificador (debe ser > 0).</summary>
        public int Id { get; }

        /// <summary>Hash de validación (no nulo ni vacío).</summary>
        public string HashValidacion { get; }

        /// <summary>Peso en bytes (debe ser > 0).</summary>
        public int PesoBytes { get; }

        /// <summary>
        /// Crea una nueva instancia validando invariantes: Id>0, Hash no nulo, Peso>0.
        /// </summary>
        /// <exception cref="ArgumentException">Si alguna precondición no se cumple.</exception>
        public RegistroDatos(int id, string hashValidacion, int pesoBytes)
        {
            if (id <= 0) throw new ArgumentException("Id debe ser mayor que cero", nameof(id));
            if (string.IsNullOrWhiteSpace(hashValidacion)) throw new ArgumentException("HashValidacion no puede ser nulo o vacío", nameof(hashValidacion));
            if (pesoBytes <= 0) throw new ArgumentException("PesoBytes debe ser mayor que cero", nameof(pesoBytes));

            Id = id;
            HashValidacion = hashValidacion;
            PesoBytes = pesoBytes;
        }

        /// <summary>Impresión alineada para consola.</summary>
        public override string ToString() => $"Id={Id,6} | Hash={HashValidacion} | Peso={PesoBytes}B";
    }

    // =========================================================================
    // 2. COMPONENTE DE MEMORIA DINÁMICA (Heap)
    // =========================================================================
    /// <summary> Nodo contenedor en el heap para la lista dinámica. </summary>
    public sealed class NodoRegistro
    {
        /// <summary>Dato de tipo valor almacenado.</summary>
        public RegistroDatos Dato { get; set; }

        /// <summary>Siguiente nodo (puede ser null).</summary>
        public NodoRegistro? Siguiente { get; set; }

        /// <summary>Constructor.</summary>
        public NodoRegistro(RegistroDatos dato)
        {
            Dato = dato;
            Siguiente = null;
        }
    }

    /// <summary>
    /// TablaDinamica: lista simplemente enlazada con API defensiva y puente a arreglo.
    /// </summary>
    public sealed class TablaDinamica
    {
        private NodoRegistro? cabeza;
        private int contadorRegistros;

        /// <summary>Cantidad física de nodos presentes.</summary>
        public int Cantidad => contadorRegistros;

        /// <summary>Constructor.</summary>
        public TablaDinamica()
        {
            cabeza = null;
            contadorRegistros = 0;
        }

        /// <summary>Inserción al inicio en O(1).</summary>
        public void InsertarInicio(RegistroDatos nuevoRegistro)
        {
            var nuevoNodo = new NodoRegistro(nuevoRegistro) { Siguiente = cabeza };
            cabeza = nuevoNodo;
            contadorRegistros++;
        }

        /// <summary>Inserción al final en O(n).</summary>
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

        /// <summary>
        /// Elimina el primer nodo que tenga Id == idTarget.
        /// Retorna true si se eliminó, false si no se encontró o id inválido.
        /// </summary>
        public bool EliminarPorId(int idTarget)
        {
            if (idTarget <= 0) return false;
            if (cabeza == null) return false;

            // Caso cabeza
            if (cabeza.Dato.Id == idTarget)
            {
                cabeza = cabeza.Siguiente;
                contadorRegistros--;
                return true;
            }

            // Caso general
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

        /// <summary>
        /// Devuelve un arreglo compacto con la dimensión física exacta que contiene los datos
        /// de la lista en orden secuencial. Si se detecta inconsistencia entre contador y nodos,
        /// se reconstrulle defensivamente y corrige el contador.
        /// </summary>
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

            // Inconsistencia detectada -> reconstrucción defensiva
            var lista = new List<RegistroDatos>(i + 4);
            for (int k = 0; k < i; k++) lista.Add(resultado[k]);
            while (actual != null)
            {
                lista.Add(actual.Dato);
                actual = actual.Siguiente;
            }

            contadorRegistros = lista.Count;
            return lista.ToArray();
        }

        /// <summary>Imprime la lista en consola.</summary>
        public void ImprimirLista()
        {
            if (cabeza == null)
            {
                Console.WriteLine("La lista dinámica está vacía.");
                return;
            }

            var actual = cabeza;
            int idx = 0;
            while (actual != null)
            {
                Console.WriteLine($"[{idx,3}] {actual.Dato}");
                actual = actual.Siguiente;
                idx++;
            }
        }

        /// <summary>Vacía la lista y ayuda al GC eliminando referencias externas.</summary>
        public void Clear()
        {
            cabeza = null;
            contadorRegistros = 0;
        }
    }

    // =========================================================================
    // 3. MOTORES DE ORDENAMIENTO INSTRUMENTADOS
    // =========================================================================
    /// <summary> Métricas de ordenamiento inmutables (solo lectura). </summary>
    public readonly struct SortMetrics
    {
        /// <summary>Cantidad de comparaciones realizadas.</summary>
        public long Comparaciones { get; }

        /// <summary>Cantidad de swaps realizados.</summary>
        public long Swaps { get; }

        /// <summary>Cantidad total de llamadas recursivas (QuickSort).</summary>
        public long Recursivas { get; }

        /// <summary>Tiempo transcurrido en milisegundos.</summary>
        public double TiempoMs { get; }

        /// <summary>Tiempo transcurrido en microsegundos.</summary>
        public double TiempoUs { get; }

        public SortMetrics(long comparaciones, long swaps, long recursivas, double tiempoMs, double tiempoUs)
        {
            Comparaciones = comparaciones;
            Swaps = swaps;
            Recursivas = recursivas;
            TiempoMs = tiempoMs;
            TiempoUs = tiempoUs;
        }
    }

    /// <summary>Clase estática que contiene los algoritmos instrumentados.</summary>
    public static class Sorter
    {
        /// <summary>SelectionSort instrumentado. El arreglo se ordena in-place.</summary>
        public static SortMetrics SelectionSort(RegistroDatos[] arr)
        {
            if (arr is null) throw new ArgumentNullException(nameof(arr));

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
            double ms = (double)sw.ElapsedTicks / Stopwatch.Frequency * 1000.0;
            double us = (double)sw.ElapsedTicks / Stopwatch.Frequency * 1_000_000.0;
            return new SortMetrics(comps, swaps, 0, ms, us);
        }

        /// <summary>QuickSort público que prepara contadores y llama a la recursiva interna.</summary>
        public static SortMetrics QuickSort(RegistroDatos[] arr)
        {
            if (arr is null) throw new ArgumentNullException(nameof(arr));

            long comps = 0;
            long swaps = 0;
            long recursivas = 0;
            var sw = Stopwatch.StartNew();

            void QuickRec(int low, int high)
            {
                recursivas++;
                if (low < high)
                {
                    int p = Particionar(arr, low, high, ref comps, ref swaps);
                    QuickRec(low, p - 1);
                    QuickRec(p + 1, high);
                }
            }

            if (arr.Length > 1) QuickRec(0, arr.Length - 1);
            sw.Stop();
            double ms = (double)sw.ElapsedTicks / Stopwatch.Frequency * 1000.0;
            double us = (double)sw.ElapsedTicks / Stopwatch.Frequency * 1_000_000.0;
            return new SortMetrics(comps, swaps, recursivas, ms, us);
        }

        /// <summary>Particionado de Lomuto que usa pivote central y swaps por tuplas.</summary>
        private static int Particionar(RegistroDatos[] arr, int low, int high, ref long comps, ref long swaps)
        {
            int medio = low + (high - low) / 2;
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

        /// <summary>Valida si el arreglo está ordenado ascendentemente por Id.</summary>
        public static bool EstaOrdenado(RegistroDatos[] arr)
        {
            if (arr is null) throw new ArgumentNullException(nameof(arr));
            for (int i = 0; i + 1 < arr.Length; i++)
            {
                if (arr[i].Id > arr[i + 1].Id) return false;
            }
            return true;
        }

        /// <summary>Buscar binario iterativo sobre arreglo ya ordenado; devuelve index o -1. Comparaciones vía out.</summary>
        public static int BuscarRegistroIndexado(RegistroDatos[]? arr, int idTarget, out int comparaciones)
        {
            comparaciones = 0;
            if (arr is null || arr.Length == 0) return -1;

            int left = 0;
            int right = arr.Length - 1;
            while (left <= right)
            {
                comparaciones++;
                int mid = left + (right - left) / 2;
                int id = arr[mid].Id;
                if (id == idTarget) return mid;
                if (id < idTarget) left = mid + 1;
                else right = mid - 1;
            }
            return -1;
        }
    }

    // =========================================================================
    // 4. UTILIDADES Y ESTADÍSTICAS
    // =========================================================================
    /// <summary>Funciones utilitarias y estadísticas para el benchmark.</summary>
    public static class Utils
    {
        /// <summary>Genera un hash reproducible (hex) a partir de Random dado.</summary>
        public static string RandomHash(Random rnd, int len = 16)
        {
            if (rnd is null) throw new ArgumentNullException(nameof(rnd));
            int byteLen = (len + 1) / 2;
            var bytes = new byte[byteLen];
            rnd.NextBytes(bytes);
            var sb = new StringBuilder(byteLen * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            var hex = sb.ToString();
            if (hex.Length > len) return hex.Substring(0, len);
            if (hex.Length < len) return hex.PadRight(len, '0');
            return hex;
        }

        /// <summary>Imprime los primeros sampleSize elementos del arreglo.</summary>
        public static void PrintSample(RegistroDatos[] arr, int sampleSize = 10)
        {
            if (arr is null) throw new ArgumentNullException(nameof(arr));
            int n = Math.Min(arr.Length, sampleSize);
            for (int i = 0; i < n; i++) Console.WriteLine($"[{i,4}] {arr[i]}");
            if (arr.Length > sampleSize) Console.WriteLine($"... (mostrando {sampleSize} de {arr.Length} elementos)");
        }

        /// <summary>Media aritmética.</summary>
        public static double Mean(double[] values) => values.Length == 0 ? 0.0 : values.Average();

        /// <summary>Mediana (robusta).</summary>
        public static double Median(double[] values)
        {
            if (values.Length == 0) return 0.0;
            var s = values.OrderBy(x => x).ToArray();
            int n = s.Length;
            return (n % 2 == 1) ? s[n / 2] : (s[n / 2 - 1] + s[n / 2]) / 2.0;
        }

        /// <summary>Desviación estándar muestral (n-1) para vectores no vacíos; si n==1 devuelve 0.</summary>
        public static double SampleStdDev(double[] values)
        {
            if (values.Length <= 1) return 0.0;
            double mean = Mean(values);
            double sumSq = values.Select(x => (x - mean) * (x - mean)).Sum();
            return Math.Sqrt(sumSq / (values.Length - 1));
        }

        /// <summary>Convierte long[] a double[] (helper).</summary>
        public static double[] ToDoubleArray(long[] src) => src.Select(x => (double)x).ToArray();

        /// <summary>Imprime tabla resumida de resultados estadísticos.</summary>
        public static void ImprimirTablaBenchmark(
            int elementos,
            double[] selTimes, long[] selComps, long[] selSwaps,
            double[] qsTimes, long[] qsComps, long[] qsSwaps, long[] qsRecursivas)
        {
            Console.WriteLine();
            Console.WriteLine("===========================================================================================");
            Console.WriteLine("                         BENCHMARK ROBUSTO: RESUMEN ESTADÍSTICO                            ");
            Console.WriteLine("===========================================================================================");
            Console.WriteLine($"Elementos: {elementos:N0}   Repeticiones por algoritmo: {selTimes.Length}");
            Console.WriteLine();

            // Times
            Console.WriteLine("Tiempos (ms):");
            Console.WriteLine("            |  Mean  |  Median  |  StdDev  |   Min   |   Max   ");
            Console.WriteLine("Selection   | {0,6:N2} | {1,7:N2} | {2,8:N2} | {3,7:N2} | {4,7:N2}",
                Mean(selTimes), Median(selTimes), SampleStdDev(selTimes), selTimes.Min(), selTimes.Max());
            Console.WriteLine("QuickSort   | {0,6:N2} | {1,7:N2} | {2,8:N2} | {3,7:N2} | {4,7:N2}",
                Mean(qsTimes), Median(qsTimes), SampleStdDev(qsTimes), qsTimes.Min(), qsTimes.Max());

            Console.WriteLine();
            // Comparaciones y Swaps (median + stddev)
            var selCompD = ToDoubleArray(selComps);
            var selSwapD = ToDoubleArray(selSwaps);
            var qsCompD = ToDoubleArray(qsComps);
            var qsSwapD = ToDoubleArray(qsSwaps);
            var qsRecD = ToDoubleArray(qsRecursivas);

            Console.WriteLine("Comparaciones (median ± sd):");
            Console.WriteLine("Selection: {0:N0} ± {1:N0}", Median(selCompD), SampleStdDev(selCompD));
            Console.WriteLine("QuickSort: {0:N0} ± {1:N0}", Median(qsCompD), SampleStdDev(qsCompD));

            Console.WriteLine();
            Console.WriteLine("Swaps (median ± sd):");
            Console.WriteLine("Selection: {0:N0} ± {1:N0}", Median(selSwapD), SampleStdDev(selSwapD));
            Console.WriteLine("QuickSort: {0:N0} ± {1:N0}", Median(qsSwapD), SampleStdDev(qsSwapD));

            Console.WriteLine();
            Console.WriteLine("Recursivas QuickSort (median ± sd): {0:N0} ± {1:N0}", Median(qsRecD), SampleStdDev(qsRecD));

            Console.WriteLine();
            double ratio = Median(selTimes) / (Median(qsTimes) == 0.0 ? double.Epsilon : Median(qsTimes));
            Console.WriteLine("Ratio de aceleración (Selection median / Quick median): {0:N2}x", ratio);

            Console.WriteLine("===========================================================================================\n");
        }
    }

    // =========================================================================
    // 5. INTERFAZ CLI Y ORQUESTADOR
    // =========================================================================
    internal static class Program
    {
        private const int BenchmarkN = 10_000;
        private const int BenchmarkR = 5;

        /// <summary>Entrada principal - ciclo interactivo defensivo.</summary>
        private static void Main()
        {
            var tabla = new TablaDinamica();
            var rnd = new Random(42);

            while (true)
            {
                try
                {
                    Console.WriteLine();
                    Console.WriteLine("================================ DataCore Engine ================================");
                    Console.WriteLine("Opciones:");
                    Console.WriteLine("  [6]  Insertar nuevo registro manual (Heap)");
                    Console.WriteLine("  [7]  Eliminar registro por ID (Heap)");
                    Console.WriteLine("  [8]  Mostrar lista dinámica actual (Heap)");
                    Console.WriteLine("  [9]  Ordenar colección actual (Telemetría de alta resolución)");
                    Console.WriteLine("  [10] Búsqueda Binaria Indexada por ID");
                    Console.WriteLine("  [11] Cargar lote de prueba predeterminado (15 registros)");
                    Console.WriteLine("  [12] Ejecutar Benchmark Científico Robusto (10,000 registros)");
                    Console.WriteLine("  [0]  Salir de DataCore Engine");
                    Console.Write("Seleccione una opción: ");

                    string? raw = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(raw) || !int.TryParse(raw.Trim(), out int opcion))
                    {
                        Console.WriteLine("[ERROR] Entrada inválida. Ingrese un número.");
                        continue;
                    }

                    switch (opcion)
                    {
                        case 6: EjecutarInsercion(tabla, rnd); break;
                        case 7: EjecutarEliminacion(tabla); break;
                        case 8: EjecutarMostrar(tabla); break;
                        case 9: EjecutarOrdenacionTelemetry(tabla); break;
                        case 10: EjecutarBusquedaIndexada(tabla); break;
                        case 11: EjecutarLotePrueba(tabla, rnd); break;
                        case 12: EjecutarBenchmarkCientifico(); break;
                        case 0:
                            Console.WriteLine("Saliendo de DataCore Engine. Hasta luego.");
                            return;
                        default:
                            Console.WriteLine("[ERROR] Opción no reconocida.");
                            break;
                    }
                }
                catch (FormatException fex)
                {
                    Console.WriteLine($"[ERROR FORMATO] {fex.Message}");
                }
                catch (ArgumentException aex)
                {
                    Console.WriteLine($"[ERROR ARGUMENTO] {aex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Excepción no controlada: {ex.Message}");
                }
            }
        }

        /// <summary>Inserta manualmente un registro en la tabla (Heap).</summary>
        private static void EjecutarInsercion(TablaDinamica tabla, Random rnd)
        {
            Console.Write("Ingrese ID (entero positivo): ");
            string? sId = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(sId) || !int.TryParse(sId.Trim(), out int id) || id <= 0)
            {
                Console.WriteLine("[ERROR] ID inválido."); return;
            }

            Console.Write("Ingrese PesoBytes (entero positivo): ");
            string? sPeso = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(sPeso) || !int.TryParse(sPeso.Trim(), out int peso) || peso <= 0)
            {
                Console.WriteLine("[ERROR] Peso inválido."); return;
            }

            var hash = Utils.RandomHash(rnd, 12);
            var reg = new RegistroDatos(id, hash, peso);
            tabla.InsertarFinal(reg);
            Console.WriteLine("[OK] Registro insertado en heap: " + reg);
        }

        /// <summary>Elimina un registro por ID en la lista dinámica.</summary>
        private static void EjecutarEliminacion(TablaDinamica tabla)
        {
            Console.Write("Ingrese ID a eliminar: ");
            string? sId = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(sId) || !int.TryParse(sId.Trim(), out int idTarget))
            {
                Console.WriteLine("[ERROR] ID inválido."); return;
            }

            bool removed = tabla.EliminarPorId(idTarget);
            Console.WriteLine(removed ? "[OK] Eliminado correctamente." : "[INFO] ID no encontrado.");
        }

        /// <summary>Muestra la lista dinámica actual en consola.</summary>
        private static void EjecutarMostrar(TablaDinamica tabla)
        {
            Console.WriteLine($"Cantidad: {tabla.Cantidad}");
            tabla.ImprimirLista();
        }

        /// <summary>Ordena la colección actual (obtiene arreglo desde la lista y ejecuta ambos sorts, single-run).</summary>
        private static void EjecutarOrdenacionTelemetry(TablaDinamica tabla)
        {
            if (tabla.Cantidad == 0) { Console.WriteLine("[INFO] No hay registros para ordenar."); return; }

            var original = tabla.ObtenerComoArreglo();
            var aSel = (RegistroDatos[])original.Clone();
            var aQs = (RegistroDatos[])original.Clone();

            var sel = Sorter.SelectionSort(aSel);
            var qs = Sorter.QuickSort(aQs);

            Utils.ImprimirTablaBenchmark(original.Length,
                new[] { sel.TiempoMs }, new[] { sel.Comparaciones }, new[] { sel.Swaps },
                new[] { qs.TiempoMs }, new[] { qs.Comparaciones }, new[] { qs.Swaps }, new[] { qs.Recursivas });
        }

        /// <summary>Ejecuta búsqueda binaria indexada (obliga a ordenar previamente con QuickSort).</summary>
        private static void EjecutarBusquedaIndexada(TablaDinamica tabla)
        {
            if (tabla.Cantidad == 0) { Console.WriteLine("[INFO] No hay registros."); return; }

            Console.Write("Ingrese ID a buscar: ");
            string? sId = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(sId) || !int.TryParse(sId.Trim(), out int idBuscado)) { Console.WriteLine("[ERROR] ID inválido."); return; }

            var index = tabla.ObtenerComoArreglo();
            Sorter.QuickSort(index); // ordena antes de buscar

            var sw = Stopwatch.StartNew();
            int idx = Sorter.BuscarRegistroIndexado(index, idBuscado, out int comps);
            sw.Stop();

            double micros = (double)sw.ElapsedTicks / Stopwatch.Frequency * 1_000_000.0;
            if (idx != -1)
            {
                Console.WriteLine("[OK] Encontrado en índice {0}. Registro: {1}", idx, index[idx]);
                Console.WriteLine("Comparaciones: {0}, Tiempo: {1:N2} μs", comps, micros);
            }
            else
            {
                Console.WriteLine("[INFO] ID no encontrado. Comparaciones: {0}, Tiempo: {1:N2} μs", comps, micros);
            }
        }

        /// <summary>Carga lote predeterminado de 15 registros.</summary>
        private static void EjecutarLotePrueba(TablaDinamica tabla, Random rnd)
        {
            tabla.Clear();
            for (int i = 1; i <= 15; i++) tabla.InsertarFinal(new RegistroDatos(i, Utils.RandomHash(rnd, 12), rnd.Next(10, 5001)));
            Console.WriteLine("[OK] Lote de 15 registros cargado (IDs 1..15).");
        }

        /// <summary>Ejecuta benchmark científico robusto: N=10,000, R repeticiones.</summary>
        private static void EjecutarBenchmarkCientifico()
        {
            const int N = BenchmarkN;
            const int R = BenchmarkR;
            var rnd = new Random(42);
            var original = new RegistroDatos[N];

            Console.WriteLine($"Generando arreglo reproducible de {N:N0} registros (Random(42))...");
            for (int i = 0; i < N; i++)
            {
                int id = rnd.Next(1, 1_000_001);
                string hash = Utils.RandomHash(rnd, 16);
                int peso = rnd.Next(1, 1_000_001);
                original[i] = new RegistroDatos(id, hash, peso);
            }

            // Warm-up JIT + pequeño calentamiento GC
            {
                var w1 = (RegistroDatos[])original.Clone();
                var w2 = (RegistroDatos[])original.Clone();
                Sorter.QuickSort(w1);
                Sorter.SelectionSort(w2);
            }

            // Arrays para recolectar métricas
            var selTimes = new double[R];
            var selComps = new long[R];
            var selSwaps = new long[R];

            var qsTimes = new double[R];
            var qsComps = new long[R];
            var qsSwaps = new long[R];
            var qsRecursivas = new long[R];

            Console.WriteLine($"Ejecutando {R} repeticiones por algoritmo. Esto puede tardar algunos segundos...");

            for (int r = 0; r < R; r++)
            {
                // QuickSort: forzar GC previo
                GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
                var aQs = (RegistroDatos[])original.Clone();
                var mQs = Sorter.QuickSort(aQs);
                qsTimes[r] = mQs.TiempoMs;
                qsComps[r] = mQs.Comparaciones;
                qsSwaps[r] = mQs.Swaps;
                qsRecursivas[r] = mQs.Recursivas;

                // SelectionSort: forzar GC previo
                GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
                var aSel = (RegistroDatos[])original.Clone();
                var mSel = Sorter.SelectionSort(aSel);
                selTimes[r] = mSel.TiempoMs;
                selComps[r] = mSel.Comparaciones;
                selSwaps[r] = mSel.Swaps;

                Console.WriteLine($"Repetición {r + 1}/{R}: Quick {qsTimes[r]:N2} ms | Sel {selTimes[r]:N2} ms");
            }

            // Validación final
            var finalQs = (RegistroDatos[])original.Clone(); Sorter.QuickSort(finalQs);
            var finalSel = (RegistroDatos[])original.Clone(); Sorter.SelectionSort(finalSel);
            if (!Sorter.EstaOrdenado(finalQs) || !Sorter.EstaOrdenado(finalSel))
            {
                Console.WriteLine("[ERROR] Algoritmo no ordenó correctamente durante verificación final.");
                return;
            }

            Utils.ImprimirTablaBenchmark(N, selTimes, selComps, selSwaps, qsTimes, qsComps, qsSwaps, qsRecursivas);
            Console.WriteLine("[OK] Benchmark finalizado y validado.");
        }
    }
}
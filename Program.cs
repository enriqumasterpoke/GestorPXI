using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks; // <-- NUEVO: Librería para Multihilo

class Program
{
    // --- CONEXIONES AL NÚCLEO DE WINDOWS ---
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("ntdll.dll", PreserveSig = false)]
    public static extern void NtSuspendProcess(IntPtr processHandle);

    [DllImport("ntdll.dll", PreserveSig = false)]
    public static extern void NtResumeProcess(IntPtr processHandle);

    [DllImport("psapi.dll")]
    private static extern int EmptyWorkingSet(IntPtr hwProc);

    // --- VARIABLES GLOBALES DEL MOTOR ---
    private static readonly HashSet<string> listaBlanca = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "explorer", "Taskmgr", "Code", "devenv", "System", "Idle", "Registry", "spoolsv", "LogonUI"
    };
    private static List<Process> listaNegra = new List<Process>();
    
    // NUEVO: El interruptor del Piloto Automático
    private static bool pilotoAutomaticoActivo = false;
    private static uint idProcesoActivo;

    static void Main()
    {
        Console.Clear();
        Console.Title = "PXI RACING ENGINE - High Performance Optimization";
        
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(@"
    __________   ___     ____  ___   __________   __________
   / ____/ __ \/  |/ /  / __ \/   | / ____/  _/  / ____/ __ \
  / /_  / /_/ / /|_/ /  / /_/ / /| |/ /    / /   / __/ / / / /
 / __/ / _, _/ /  / /  / _, _/ ___ / /____/ /   / /___/ /_/ / 
/_/   /_/ |_/_/  /_/  /_/ |_/_/  |_\____/___/  /_____/\____/  
        ");
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(" [ PIXIE ECOSYSTEM ] - Versión 1.3 AUTO");
        Console.WriteLine(" [ STATUS ] Kernel: Windows | Optimizer: Active");
        Console.WriteLine(" --------------------------------------------------");
        Console.ResetColor();

        IntPtr ventanaActiva = GetForegroundWindow();
        GetWindowThreadProcessId(ventanaActiva, out idProcesoActivo);
        
        try 
        {
            Process procesoProtegido = Process.GetProcessById((int)idProcesoActivo);
            procesoProtegido.PriorityClass = ProcessPriorityClass.High;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[MODO DIOS] {procesoProtegido.ProcessName} protegido.\n");
            Console.ResetColor();
        } 
        catch { }

        while (true)
        {
            Console.WriteLine("=========================================");
            Console.WriteLine("Elige una opción:");
            Console.WriteLine("1 - Activar Radar Manual (Boost)");
            Console.WriteLine("2 - Descongelar y Restaurar Sistema");
            Console.WriteLine("3 - Salir del Motor PXI");
            Console.WriteLine("4 - Purga Profunda (RAM Global y Red)");
            // NUEVA OPCIÓN EN EL MENÚ
            Console.WriteLine($"5 - Piloto Automático: {(pilotoAutomaticoActivo ? "ENCENDIDO [ON]" : "APAGADO [OFF]")}");
            Console.Write("Opción: ");
            string opcion = Console.ReadLine();

            if (opcion == "1")
            {
                EjecutarRadar();
                Console.WriteLine("\n[RADAR] Escaneo manual completado.");
            }
            else if (opcion == "2")
            {
                foreach (Process prisionero in listaNegra)
                {
                    try { NtResumeProcess(prisionero.Handle); } catch { }
                }
                listaNegra.Clear();
                Console.WriteLine("\n[SISTEMA RESTAURADO] Procesos liberados.");
            }
            else if (opcion == "4")
            {
                Console.WriteLine("\n[INFO] Ejecutando purga...");
                long ramTotalLiberada = 0;
                int procesosPurgados = 0;

                foreach (Process p in Process.GetProcesses())
                {
                    if (listaBlanca.Contains(p.ProcessName)) continue;
                    try 
                    {
                        long ramAntes = p.WorkingSet64;
                        EmptyWorkingSet(p.Handle);
                        if (p.WorkingSet64 < ramAntes)
                        {
                            ramTotalLiberada += (ramAntes - p.WorkingSet64);
                            procesosPurgados++;
                        }
                    } 
                    catch { }
                }
                try { Process.Start(new ProcessStartInfo("cmd.exe", "/c ipconfig /flushdns") { CreateNoWindow = true }); } catch { }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[REPORTE] {procesosPurgados} procesos optimizados. {(ramTotalLiberada / 1048576.0):F2} MB recuperados.\n");
                Console.ResetColor();
            }
            else if (opcion == "5")
            {
                // INVERTIMOS EL ESTADO DEL PILOTO
                pilotoAutomaticoActivo = !pilotoAutomaticoActivo; 
                
                if (pilotoAutomaticoActivo)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n[PILOTO AUTOMÁTICO ACTIVADO] El motor trabajará en las sombras cada 5 segundos.");
                    Console.ResetColor();
                    
                    // CREAMOS EL SEGUNDO CEREBRO (Multihilo)
                    Task.Run(() => 
                    {
                        while (pilotoAutomaticoActivo)
                        {
                            EjecutarRadar();
                            Thread.Sleep(5000); // Espera 5 segundos antes del próximo escaneo
                        }
                    });
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n[PILOTO AUTOMÁTICO DESACTIVADO] Control devuelto a modo manual.");
                    Console.ResetColor();
                }
            }
            else if (opcion == "3") break;
        }
    }

    // --- EL MOTOR SEPARADO EN UN MÓDULO RECICLABLE ---
    static void EjecutarRadar()
    {
        foreach (Process objetivo in Process.GetProcesses())
        {
            if (objetivo.Id == idProcesoActivo || 
                objetivo.Id == Process.GetCurrentProcess().Id || 
                listaBlanca.Contains(objetivo.ProcessName) ||
                listaNegra.Exists(p => p.Id == objetivo.Id))
            {
                continue;
            }

            try 
            {
                TimeSpan tiempoInicial = objetivo.TotalProcessorTime;
                DateTime lecturaInicial = DateTime.Now;
                Thread.Sleep(100); 
                TimeSpan tiempoFinal = objetivo.TotalProcessorTime;
                DateTime lecturaFinal = DateTime.Now;

                double usoCPU = (tiempoFinal.TotalMilliseconds - tiempoInicial.TotalMilliseconds) / 
                                (lecturaFinal.Subtract(lecturaInicial).TotalMilliseconds * Environment.ProcessorCount) * 100;

                bool amenazaRAM = objetivo.WorkingSet64 > 300000000;
                bool amenazaCPU = usoCPU > 15;

                if (amenazaRAM || amenazaCPU)
                {
                    EmptyWorkingSet(objetivo.Handle);
                    NtSuspendProcess(objetivo.Handle);
                    listaNegra.Add(objetivo);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    if (amenazaCPU) Console.WriteLine($"\n[AUTO-DEFENSA CPU] {objetivo.ProcessName} neutralizado.");
                    else Console.WriteLine($"\n[AUTO-DEFENSA RAM] {objetivo.ProcessName} neutralizado.");
                    Console.ResetColor();
                }
            }
            catch { }
        }
    }
}
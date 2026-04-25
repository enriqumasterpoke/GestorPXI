using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic; // Necesario para crear listas de objetivos

class Program
{
    // --- CONEXIÓN AL NÚCLEO DE WINDOWS (user32.dll) ---
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    // --- LAS LLAVES DE LA CÁMARA CRIOGÉNICA (ntdll.dll) ---
    // Estas son las funciones secretas de Windows para congelar y descongelar
    [DllImport("ntdll.dll")]
    private static extern int NtSuspendProcess(IntPtr processHandle);

    [DllImport("ntdll.dll")]
    private static extern int NtResumeProcess(IntPtr processHandle);

    static void Main()
    {
        Console.WriteLine("=== GESTOR PXI: MODO BOOST (CRIOGENIZACIÓN) ===");
        
        IntPtr ventanaActiva = GetForegroundWindow();
        GetWindowThreadProcessId(ventanaActiva, out uint idProcesoActivo);
        
        Console.WriteLine($"[ESCUDO ACTIVO] ID protegido: {idProcesoActivo}\n");

        //---INYECCION DE PRIORIDAD CPU---
        try
        {
            Process procesoProtegido = Process.GetProcessById((int)idProcesoActivo);
            procesoProtegido.PriorityClass = ProcessPriorityClass.High;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[MODO DIOS ACTIVADO] {procesoProtegido.ProcessName} tiene ahora maxima prioridad en la CPU.");
            Console.ResetColor();   
            Console.WriteLine("------------------------------------------------------");
            }
            catch
        {
            Console.WriteLine("[INFO] El proceso activo est protegido por windows. Prioridad estandar mantenida.\n");
        }
        

        Process[] procesos = Process.GetProcesses();
        List<Process> listaNegra = new List<Process>(); // Aquí guardaremos a los prisioneros

        foreach (Process proceso in procesos)
        {
            try
            {
                long memoriaMB = proceso.WorkingSet64 / (1024 * 1024);

                if (memoriaMB > 300 && proceso.Id != idProcesoActivo)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[OBJETIVO FIJADO] {proceso.ProcessName} | RAM: {memoriaMB} MB");
                    Console.ResetColor();
                    listaNegra.Add(proceso); // Añadimos el proceso a nuestra lista negra
                }
            }
            catch { continue; }
        }

        // Si la lista está vacía, no hay nada que hacer
        if (listaNegra.Count == 0)
        {
            Console.WriteLine("El sistema está limpio. No hay objetivos para congelar.");
            Console.WriteLine("Presiona Enter para salir.");
            Console.ReadLine();
            return; 
        }

        // --- EL PANEL DE CONTROL MANUAL ---
        Console.WriteLine("\n¿Qué orden deseas ejecutar, Arquitecto?");
        Console.WriteLine("[ 1 ] Congelar procesos (Activar Boost)");
        Console.WriteLine("[ 2 ] Cancelar y salir");
        
        Console.Write("\nIngresa tu orden: ");
        string opcion = Console.ReadLine();

        if (opcion == "1")
        {
            Console.WriteLine("\n>>> INICIANDO SECUENCIA DE SUSPENSIÓN <<<");
            foreach (Process objetivo in listaNegra)
            {
                try
                {
                    // ¡AQUÍ ESTÁ LA MAGIA! Disparamos el rayo congelante
                    NtSuspendProcess(objetivo.Handle);
                    
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"[CONGELADO] {objetivo.ProcessName} ha sido suspendido exitosamente.");
                    Console.ResetColor();
                }
                catch
                {
                    Console.WriteLine($"[DENEGADO] Windows protegió a {objetivo.ProcessName}. Requiere más privilegios.");
                }
            }
            
            Console.WriteLine("\n==================================================");
            Console.WriteLine("  ¡PXI BOOST ACTIVADO! LOS PROCESOS ESTÁN EN COMA  ");
            Console.WriteLine("==================================================");
            Console.WriteLine("Toda tu RAM y CPU están ahora libres para la tarea principal.");
            Console.WriteLine("Cuando quieras restaurar el sistema, escribe 2 y presiona Enter.");
            
            // Atrapamos al programa aquí hasta que escribas "2"
            while(Console.ReadLine() != "2") 
            {
                 Console.WriteLine("Comando no reconocido. Escribe 2 para restaurar y salir.");
            }

            // --- SECUENCIA DE DESCONGELACIÓN ---
            Console.WriteLine("\n>>> INICIANDO SECUENCIA DE DESCONGELACIÓN <<<");
            foreach (Process objetivo in listaNegra)
            {
                try
                {
                    NtResumeProcess(objetivo.Handle); // Despertamos al proceso
                    Console.WriteLine($"[RESTAURADO] {objetivo.ProcessName} ha vuelto a la vida.");
                }
                catch { }
            }
            Console.WriteLine("\nSistema restaurado a la normalidad. Presiona Enter para salir.");
            Console.ReadLine();
        }
        else
        {
            Console.WriteLine("Operación cancelada. Sistema intacto.");
        }
    }
}
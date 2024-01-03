using NanoKernel.CLI;
using NanoKernel.Comunicacion;
using NanoKernel.Modulos;
using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace NanoKernel
{
    public static class App
    {
        public static void Start(Assembly baseAssembly)
        {
            Debug.WriteLine("\r\n\r\n  ______  _____  _                        _ \r\n |  ____|/ ____|| |                      | |\r\n | |__  | (___  | | _____ _ __ _ __   ___| |\r\n |  __|  \\___ \\ | |/ / _ \\ '__| '_ \\ / _ \\ |\r\n | |____ ____) ||   <  __/ |  | | | |  __/ |\r\n |______|_____(_)_|\\_\\___|_|  |_| |_|\\___|_|\r\n                                            \r\n                                            \r\n\r\n");
            Debug.WriteLine("Starting...");

            ConstruirModulos(baseAssembly);

            Debug.WriteLine("Started OK");

            Thread.Sleep(Timeout.Infinite);
        }

        static Hashtable Modulos = new Hashtable();
        private static void ConstruirModulos(Assembly baseAssembly)
        {
            // Esta sintaxis fea va a quedar linda con Dependency injection
            var com = new ComunicadorSerie();
            RegistrarModulo("comunicador", com, typeof(ComunicadorSerie));

            RegistrarModulo("consola", new Consola(com, Modulos), typeof(Consola));

            RegistrarModulosGenericos(baseAssembly);
        }

        private static void RegistrarModulosGenericos(Assembly assembly)
        {
            Type[] classesWithModuloAttribute = assembly.GetTypes();

            foreach (Type classType in classesWithModuloAttribute)
            {
                object[] attributes = classType.GetCustomAttributes(true);
                foreach (var item in attributes)
                {
                    if (item is ModuloAttribute)
                    {
                        try
                        {
                            RegistrarModulo(((ModuloAttribute)item).Nombre, classType);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Error: no se pudo registrar: " + ((ModuloAttribute)item).Nombre);
                        }

                        break;
                    }
                }
            }
        }

        private static object RegistrarModulo(string id, Type classType)
        {
            return RegistrarModulo(id, Activator.CreateInstance(classType), classType);
        }

        /// <summary>
        /// Aca se agregan los modulos a la aplicacion
        /// </summary>
        /// <param name="id"></param>
        /// <param name="instancia"></param>
        /// <returns>El modulo</returns>
        private static Modulo RegistrarModulo(string id, object instancia, Type classType)
        {
            id = id.ToLower();

            if (Modulos.Contains(id))
                throw new Exception("Error: ya se encuentra registrado el modulo" + id);

            var modulo = new Modulo(id, instancia, classType);
            Modulos.Add(id, modulo);

            Debug.WriteLine("Modulo degistrado: " + id);

            return modulo;
        }
    }
}

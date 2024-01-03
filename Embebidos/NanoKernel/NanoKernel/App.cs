using NanoKernel.CLI;
using NanoKernel.Comunicacion;
using NanoKernel.Loggin;
using NanoKernel.Modulos;
using System;
using System.Collections;
using System.Reflection;
using System.Threading;

namespace NanoKernel
{
    public static class App
    {
        public static void Start(Assembly baseAssembly)
        {
            Logger.Log("\r\n\r\n  ______  _____  _                        _ \r\n |  ____|/ ____|| |                      | |\r\n | |__  | (___  | | _____ _ __ _ __   ___| |\r\n |  __|  \\___ \\ | |/ / _ \\ '__| '_ \\ / _ \\ |\r\n | |____ ____) ||   <  __/ |  | | | |  __/ |\r\n |______|_____(_)_|\\_\\___|_|  |_| |_|\\___|_|\r\n                                            \r\n                                            \r\n\r\n");
            Logger.Log("Starting...");

            ConstruirModulos(baseAssembly);

            Logger.Log("Started OK");

            Thread.Sleep(Timeout.Infinite);
        }

        static Hashtable Modulos = new Hashtable();
        private static void ConstruirModulos(Assembly baseAssembly)
        {
            // Esta sintaxis fea va a quedar linda con Dependency Injection

            var com2 = new ComunicadorSerie("COM2", 32, 33);
            var com3 = new ComunicadorSerie("COM3", 16, 17);

            RegistrarModulo("sim", new ComunicadorSIM800L(com2), typeof(ComunicadorSIM800L));

            RegistrarModulo("consola", new Consola(com3, Modulos), typeof(Consola));

            RegistrarModulos(Assembly.GetExecutingAssembly());
            RegistrarModulos(baseAssembly);
        }

        private static void RegistrarModulos(Assembly assembly)
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
                            Logger.Log("Error: no se pudo registrar: " + ((ModuloAttribute)item).Nombre);
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

            Logger.Log("Modulo registrado: " + id);

            return modulo;
        }
    }
}

using System;
using System.Collections;
using System.Reflection;

namespace NanoKernel.Modulos
{
    public class Modulo
    {
        public string Id => id;
        public Type Tipo => tipoInstancia;

        public Hashtable Servicios = new Hashtable();

        private readonly string id;
        private readonly object instancia;
        private readonly Type tipoInstancia;

        public Modulo(string id, object instancia, Type tipoInstancia)
        {
            this.id = id;
            this.instancia = instancia;
            this.tipoInstancia = tipoInstancia;

            // Extraemos los servicios
            foreach (MethodInfo metodo in Tipo.GetMethods())
            {
                foreach (object attr in metodo.GetCustomAttributes(false))
                {
                    if (attr is ServicioAttribute)
                    {
                        Servicios.Add(((ServicioAttribute)attr).Nombre, metodo);
                        break;
                    }
                }
            }
        }

        public object InvocarServicio(string idServicio, params object[] parametros)
        {
            if (Servicios.Contains(idServicio) == false)
                throw new Exception($"No existe el servicio {idServicio}");

            var metodo = Servicios[idServicio] as MethodInfo;

            return metodo.Invoke(this.instancia, parametros);
        }

    }
}

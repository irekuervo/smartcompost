using System;

namespace NanoKernel.Modulos
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ServicioAttribute : Attribute
    {
        public ServicioAttribute(string nombre, string descripcion = "-")
        {
            this.nombre = nombre;
            this.descripcion = descripcion;
        }

        private string nombre;
        private string descripcion;

        public string Nombre => nombre;
        public string Descripcion => descripcion;
    }
}

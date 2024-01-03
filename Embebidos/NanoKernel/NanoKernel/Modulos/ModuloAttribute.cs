using System;

namespace NanoKernel.Modulos
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ModuloAttribute : Attribute
    {
        public ModuloAttribute(string nombre, string descripcion = "-")
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

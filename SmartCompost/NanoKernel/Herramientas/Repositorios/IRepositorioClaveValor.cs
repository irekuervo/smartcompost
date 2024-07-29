using System;

namespace NanoKernel.Herramientas.Repositorios
{
    public interface IRepositorioClaveValor : IDisposable
    {
        public string[] GetAll();

        public string Get(string id);

        public void Delete(string id);

        public void Update(string id, string value);
    }
}

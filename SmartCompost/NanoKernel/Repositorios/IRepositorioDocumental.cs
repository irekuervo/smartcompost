namespace NanoKernel.Repositorios
{
    public interface IRepositorioDocumental
    {
        public string Get(string id);

        public void Delete(string id);

        public void Update(string id, string value);
    }
}

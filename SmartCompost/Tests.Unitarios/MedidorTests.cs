using nanoFramework.TestFramework;
using NanoKernel.Herramientas.Medidores;
using System;

namespace Tests.Unitarios
{
    [TestClass]
    public class MedidorTests
    {
        [TestMethod]
        public void Contar_SingleThread_CorrectCount()
        {
            // Arrange
            Medidor medidor = new Medidor(1000);

            // Act
            medidor.Contar("evento1", 5);
            medidor.Contar("evento1", 3);

            // Assert
            Assert.AreEqual(8, medidor.ContadoTotal("evento1"));
            Assert.AreEqual(8, medidor.ContadoEnPeriodo("evento1"));
        }
    }
}

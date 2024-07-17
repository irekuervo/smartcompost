using nanoFramework.Json;
using NanoKernel.Ayudantes;
using NanoKernel.Comunicacion.Old;
using NanoKernel.Logging;
using NanoKernel.Modulos;
using System;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Threading;

namespace NanoKernel.Herramientas.CLI
{
    public class Consola : IDisposable
    {
        private Comunicador comunicador;
        private Hashtable modulos;
        public Consola(Comunicador comunicador, Hashtable modulos)
        {
            this.comunicador = comunicador;
            this.modulos = modulos;

            this.comunicador.DataRecieved += Comunicador_DataRecieved;
        }

        private void Comunicador_DataRecieved(byte[] data, int offset, int count)
        {
            string comando = Encoding.UTF8.GetString(data, offset, count).TrimEnd('\r', '\n');

            if (comando.Length == 0)
            {
                ResponderComando("El comando no puede venir vacio");
                return;
            }

            string[] partes = comando.Split(' ');
            string idModulo = partes[0].ToLower();

            if (idModulo == "?")
            {
                AyudaGeneral();
                return;
            }

            if (modulos.Contains(idModulo) == false)
            {
                ResponderComando("No se reconoce el modulo: " + idModulo);
                return;
            }

            if (partes.Length < 2)
            {
                ResponderComando("Debe especificar el servicio");
                return;
            }

            Modulo modulo = modulos[idModulo] as Modulo;
            var idServicio = partes[1].ToLower();

            if (idServicio == "?")
            {
                AyudaModulo(modulo);
                return;
            }

            if (modulo.Servicios.Contains(idServicio) == false)
            {
                ResponderComando("No se reconoce el servicio: " + idServicio);
                return;
            }

            MethodInfo metodo = modulo.Servicios[idServicio] as MethodInfo;
            try
            {
                var res = modulo.InvocarServicio(idServicio, ObtenerParametros(partes, metodo.GetParameters()));
                ImprimirRespuestaServicio(metodo.ReturnType, res);
            }
            catch (Exception ex)
            {
                ResponderComando(ex.Message);
            }
        }

        private void AyudaModulo(Modulo modulo)
        {
            string res = "Servicios disponibles: \r\n";
            foreach (var idServicio in modulo.Servicios.Keys)
            {
                res += $"-{idServicio}:";
                MethodInfo info = (MethodInfo)modulo.Servicios[idServicio];
                foreach (ParameterInfo param in info.GetParameters())
                {
                    res = res + $" <{param.ParameterType.Name}>";
                }
                res += "\r\n";
            }

            ResponderComando(res);
        }

        private void AyudaGeneral()
        {
            string res = "Modulos disponibles: \r\n";
            foreach (var item in modulos.Keys)
            {
                res += $"[{item}]" + "\r\n";
            }

            ResponderComando(res);
        }

        private void ImprimirRespuestaServicio(Type returnType, object methodResponse)
        {
            /// Respuesta OK void
            if (returnType == typeof(void))
            {
                ResponderComando("OK");
                return;
            }

            /// Respuesta vacia invalida
            if (methodResponse == null && returnType != typeof(string))
            {
                ResponderComando("Error: respuesta vacia invalida");
                return;
            }

            /// Respuesta OK vacia valida
            if (methodResponse == null && returnType == typeof(string))
            {
                ResponderComando("");
                return;
            }

            /// Respuesta no vacia
            if (returnType == typeof(string))
            {
                ResponderComando(methodResponse.ToString());
                return;
            }

            if (returnType == typeof(DateTime))
            {
                ResponderComando(((DateTime)methodResponse).ToUnixTimeSeconds().ToString());
                return;
            }

            if (returnType.IsValueType)
            {
                ResponderComando(methodResponse.ToString());
                return;
            }
            else
            {
                ResponderComando(methodResponse.ToJson());
                return;
            }
        }

        private byte[] bufferResponse = new byte[1024];
        private string comando;
        private void ResponderComando(string mensaje)
        {
            comando = mensaje + "\r\n";

            if (comando.Length > bufferResponse.Length)
            {
                comando = "ERROR: respuesta demasiada larga. \r\n";
                Encoding.UTF8.GetBytes(comando, 0, comando.Length, bufferResponse, 0);
                comunicador.SendAsync(bufferResponse, 0, comando.Length);
                return;
            }

            Encoding.UTF8.GetBytes(comando, 0, comando.Length, bufferResponse, 0);
            comunicador.SendAsync(bufferResponse, 0, comando.Length);
            return;
        }

        const int IndiceHastaParametro = 2;
        private object[] ObtenerParametros(string[] partes, ParameterInfo[] parametersInfo)
        {
            if (partes.Length - IndiceHastaParametro != parametersInfo.Length)
            {
                throw new Exception("Faltan parametros");
            }

            // Recorro en orden los parametros y los casteo segun lo que pida la firma
            object[] parametrosMetodo = new object[parametersInfo.Length];
            int indice = 0;
            foreach (var parametro in parametersInfo)
            {
                Type tipoParametro = parametro.ParameterType;
                string parametroDelComando = partes[indice + IndiceHastaParametro];

                try
                {
                    object obj = null;

                    if (tipoParametro == typeof(string))
                        obj = parametroDelComando;
                    else if (tipoParametro.IsValueType)
                        obj = aySerializacion.FromString(tipoParametro, parametroDelComando);
                    else
                        obj = aySerializacion.FromJson(parametroDelComando, tipoParametro);

                    if (obj == null)
                        throw new Exception("No se pudo deserealizar el comando");

                    parametrosMetodo[indice] = obj;
                }
                catch (Exception ex)
                {
                    throw new Exception($"El parámetro '{parametroDelComando}' debe ser '{tipoParametro.Name}'");
                }

                indice++;
            }

            return parametrosMetodo;
        }

        public void Dispose()
        {
        }
    }
}

using NanoKernel.Comunicacion;
using NanoKernel.Modulos;
using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace NanoKernel.CLI
{
    public class Consola
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
            if (modulo.Servicios.Contains(idServicio) == false)
            {
                ResponderComando("No se reconoce el servicio: " + idServicio);
                return;
            }

            MethodInfo metodo = modulo.Servicios[idServicio] as MethodInfo;
            try
            {
                var res = modulo.InvocarServicio(idServicio, ObtenerParametros(partes, metodo.GetParameters()));
                ImprimirRespuestaServicio(res.GetType(), res);
            }
            catch (Exception ex)
            {
                ResponderComando(ex.Message);
            }
        }

        private void ImprimirRespuestaServicio(Type returnType, object methodResponse)
        {
            string res = "OK";

            /// Respuesta OK void
            if (returnType == typeof(void))
                res = null;
            /// Respuesta vacia invalida
            else if (methodResponse == null && returnType != typeof(string))
                res = "Error: respuesta vacia invalida";
            /// Respuesta OK vacia valida
            else if (methodResponse == null && returnType == typeof(string))
                res = null;
            /// Respuesta no vacia
            else if (returnType == typeof(string))
                res = methodResponse.ToString();
            else if (returnType == typeof(DateTime))
                res = ((DateTime)methodResponse).ToUnixTimeSeconds().ToString();
            if (returnType.IsValueType)
                res = methodResponse.ToString();
            //else
            //    res = methodResponse.ToJson();

            if (res != null)
                ResponderComando(res);
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

        private object[] ObtenerParametros(string[] partes, ParameterInfo[] parametersInfo)
        {
            if (partes.Length - 2 != parametersInfo.Length)
            {
                throw new Exception("Faltan parametros");
            }

            // Recorro en orden los parametros y los casteo segun lo que pida la firma
            object[] parametrosMetodo = new object[parametersInfo.Length];
            int indice = 2;
            foreach (var parametro in parametersInfo)
            {
                Type tipoParametro = parametro.ParameterType;
                string parametroDelComando = partes[indice];

                try
                {
                    object obj = null;

                    if (tipoParametro == typeof(string))
                        obj = parametroDelComando;
                    else if (tipoParametro.IsValueType)
                        obj = ParsearParametroComandoValueType(tipoParametro, parametroDelComando);
                    //else
                    //    obj = AyudanteDeSerializacion.FromJson(parametroDelComando, tipoParametro);

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

        private object ParsearParametroComandoValueType(Type tipoParametro, string parametroComando)
        {
            object objetoParametro = null;

            if (tipoParametro == typeof(byte))
            {
                objetoParametro = Convert.ToByte(parametroComando);
            }
            else if (tipoParametro == typeof(short))
            {
                objetoParametro = Convert.ToInt16(parametroComando);
            }
            else if (tipoParametro == typeof(ushort))
            {
                objetoParametro = Convert.ToUInt16(parametroComando);
            }
            else if (tipoParametro == typeof(int))
            {
                objetoParametro = Convert.ToInt32(parametroComando);
            }
            else if (tipoParametro == typeof(uint))
            {
                objetoParametro = Convert.ToUInt32(parametroComando);
            }
            else if (tipoParametro == typeof(long))
            {
                objetoParametro = Convert.ToInt64(parametroComando);
            }
            else if (tipoParametro == typeof(ulong))
            {
                objetoParametro = Convert.ToUInt64(parametroComando);
            }
            else if (tipoParametro == typeof(float))
            {
                objetoParametro = Convert.ToSingle(parametroComando);
            }
            else if (tipoParametro == typeof(double))
            {
                objetoParametro = Convert.ToDouble(parametroComando);
            }

            if (objetoParametro == null)
                throw new Exception($"No se puede parsear el parametro '{parametroComando}' del tipo '{tipoParametro.Name}'");

            return objetoParametro;
        }
    }
}

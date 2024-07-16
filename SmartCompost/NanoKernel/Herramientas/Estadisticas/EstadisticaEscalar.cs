using NanoKernel.Ayudantes;
using System;
using System.Collections;

namespace NanoKernel.Herramientas.Estadisticas
{
    public class EstadisticaEscalar
    {
        public EstadisticaEscalar()
        {

        }

        #region Propiedades
        public bool Habilitado { get; set; }
        public float SumaCuadratica { get; set; } = 0;
        public float Suma { get; set; } = 0;
        public uint CantidadMuestras { get; set; } = 0;
        public float Minimo { get; set; } = float.MaxValue;
        public float Maximo { get; set; } = float.MinValue;
        public float RangoMaximo { get; set; } = float.MaxValue;
        public float RangoMinimo { get; set; } = float.MinValue;
        public uint Infracciones { get; set; } = 0;
        public float UltimaMuestra { get; set; } = float.NaN;
        public DateTime FechaUltimaMuestra { get; set; } = DateTime.MinValue;
        public DateTime FechaPrimeraMuestra { get; set; } = DateTime.MaxValue;

        /// Histograma

        public uint[] Histograma { get; set; } = new uint[0]; /// Si no se inicializa se va a crear con un tamaño default
        public float HistogramaDesde { get; set; } = 0; /// Para setearlo a mano debe estar inicializado el vector Histograma, sino se pisa
        public float HistogramaHasta { get; set; } = 0; /// Para setearlo a mano debe estar inicializado el vector Histograma, sino se pisa
        public uint CantidadMuestrasHistograma { get; set; } = 0;
        public uint CantidadMuestrasFueraHistogramaIzquierda { get; set; } = 0;
        public uint CantidadMuestrasFueraHistogramaDerecha { get; set; } = 0;


        #endregion

        private object lockAgregarMuestra = new object();

        #region Metodos Publicos
        public TimeSpan Duracion()
        {
            return FechaUltimaMuestra - FechaPrimeraMuestra;
        }

        public double Desvio()
        {
            if (CantidadMuestras == 0) return 0;

            /// σ = Raiz { 1/N * ∑( (xi-µ)^2 ) }
            /// =   Raiz {  1/N * ( SumaCuadratica - 2 * Promedio * Suma + Promedio^2 * CantidadRecibida ) }
            float Promedio = Suma / CantidadMuestras;
            return ayEstadisticas.ObtenerDesvioEstandar(CantidadMuestras, SumaCuadratica, Suma, Promedio);
        }

        public float Promedio()
        {
            if (CantidadMuestras == 0) return 0;
            return Suma / CantidadMuestras;
        }

        public bool AgregarMuestra(float muestra, bool calcularHistograma = false /*ES MUY CARO POR DEFAULT*/)
        {
            lock (lockAgregarMuestra)
            {
                CantidadMuestras++;
                UltimaMuestra = muestra;

                //// FechaPrimeraMuestra y FechaUltimaMuestra
                var ahora = DateTime.UtcNow;
                FechaUltimaMuestra = ahora;
                if (FechaPrimeraMuestra == DateTime.MaxValue) FechaPrimeraMuestra = ahora;

                /// CantidadMuestrasFueraDeRango
                bool huboInfraccion = false;
                if (RangoMaximo != float.MaxValue && muestra > RangoMaximo || RangoMinimo != float.MinValue && muestra < RangoMinimo)
                {
                    huboInfraccion = true;
                    Infracciones++;
                }

                /// Luego asigno las nuevas cotas
                Maximo = muestra > Maximo ? muestra : Maximo;
                Minimo = muestra < Minimo ? muestra : Minimo;

                /// Suma y suma cuadratica
                Suma += muestra;
                SumaCuadratica += (float)Math.Pow(muestra, 2);

                /// Agregamos la muestra al histograma
                if (calcularHistograma)
                    AgregarMuestraHistograma(muestra);

                return huboInfraccion;
            }
        }


        /// acumularUltimaMuestraHistograma: si es true, agregamos la ultima muestra de la estadistica anterior al histograma,
        /// sino intentamos acumular el vector entero. Está pensado para estadisticas que se acumulan de a una muestra (misiones)
        public void AcumularMuestras(EstadisticaEscalar estadistica, bool acumularUltimaMuestraHistograma = false)
        {
            SumaCuadratica += estadistica.SumaCuadratica;
            Suma += estadistica.Suma;
            CantidadMuestras += estadistica.CantidadMuestras;
            Infracciones += estadistica.Infracciones;

            Minimo = estadistica.Minimo < Minimo ? estadistica.Minimo : Minimo;
            Maximo = estadistica.Maximo > Maximo ? estadistica.Maximo : Maximo;

            FechaPrimeraMuestra = new DateTime((long)Math.Min(estadistica.FechaPrimeraMuestra.Ticks, FechaPrimeraMuestra.Ticks));
            FechaUltimaMuestra = new DateTime((long)Math.Max(estadistica.FechaUltimaMuestra.Ticks, FechaUltimaMuestra.Ticks));

            if (acumularUltimaMuestraHistograma)
            {
                AgregarMuestraHistograma(estadistica.UltimaMuestra);
            }
            else
            {
                /// Acumulacion Histograma
                if (Histograma.Length == 0 && estadistica.Histograma.Length > 0)
                {
                    Histograma = new uint[estadistica.Histograma.Length];
                }

                /// Si no tengo nada me adapto al que viene
                if (Histograma.Any(c => c > 0) == false)
                {
                    HistogramaDesde = estadistica.HistogramaDesde;
                    HistogramaHasta = estadistica.HistogramaHasta;
                }

                /// Si no coinciden exactamente no acumulo
                if (Histograma.Length != estadistica.Histograma.Length
                    || HistogramaDesde != estadistica.HistogramaDesde
                    || HistogramaHasta != estadistica.HistogramaHasta)
                    return;

                /// Si puedo acumular los bines, acumulo los datos
                CantidadMuestrasHistograma += estadistica.CantidadMuestrasHistograma;
                CantidadMuestrasFueraHistogramaIzquierda += estadistica.CantidadMuestrasFueraHistogramaIzquierda;
                CantidadMuestrasFueraHistogramaDerecha += estadistica.CantidadMuestrasFueraHistogramaDerecha;

                /// Acumulo los bines
                for (int i = 0; i < Histograma.Length; i++)
                {
                    Histograma[i] += estadistica.Histograma[i];
                }
            }

        }

        public void Limpiar()
        {
            SumaCuadratica         /**/ = 0;
            Suma                   /**/ = 0;
            CantidadMuestras       /**/ = 0;
            Minimo                 /**/ = float.MaxValue;
            Maximo                 /**/ = float.MinValue;
            Infracciones           /**/ = 0;
            UltimaMuestra          /**/ = float.NaN;
            FechaUltimaMuestra     /**/ = DateTime.MinValue;
            FechaPrimeraMuestra    /**/ = DateTime.MaxValue;

            LimpiarHistograma();
        }

        public EstadisticaEscalar Clonar()
        {
            EstadisticaEscalar ret = new EstadisticaEscalar();

            ret.Habilitado =                                /**/ Habilitado;
            ret.SumaCuadratica =                            /**/ SumaCuadratica;
            ret.Suma =                                      /**/ Suma;
            ret.CantidadMuestras =                          /**/ CantidadMuestras;
            ret.Minimo =                                    /**/ Minimo;
            ret.Maximo =                                    /**/ Maximo;
            ret.RangoMaximo =                               /**/ RangoMaximo;
            ret.RangoMinimo =                               /**/ RangoMinimo;
            ret.Infracciones =                              /**/ Infracciones;
            ret.UltimaMuestra =                             /**/ UltimaMuestra;
            ret.FechaUltimaMuestra =                        /**/ FechaUltimaMuestra;
            ret.FechaPrimeraMuestra =                       /**/ FechaPrimeraMuestra;
            ret.Histograma =                                /**/ Histograma;
            ret.HistogramaDesde =                           /**/ HistogramaDesde;
            ret.HistogramaHasta =                           /**/ HistogramaHasta;
            ret.CantidadMuestrasHistograma =                /**/ CantidadMuestrasHistograma;
            ret.CantidadMuestrasFueraHistogramaDerecha =    /**/ CantidadMuestrasFueraHistogramaDerecha;
            ret.CantidadMuestrasFueraHistogramaIzquierda =  /**/ CantidadMuestrasFueraHistogramaIzquierda;
            return ret;
        }

        public void SetRangosAutomaticos()
        {
            RangoMaximo = Maximo;
            RangoMinimo = Minimo;
        }

        public void SetHistogramaAutomatico()
        {
            LimpiarHistograma();

            HistogramaDesde = Minimo;
            HistogramaHasta = Maximo;
        }

        #endregion

        #region  Histograma

        private int histogramaCantidadBins = 21;
        private float histogramaConfianza = .01f;

        /// <summary>
        /// La idea es que con un buffer mantengo las ultimas N muestras para el histograma. Si el histograma se resetea siempre cuento con esas muestras.
        /// El histograma se resetea cuando no cumple con el intevalo de confianza
        /// No se cumple el intervalo de confianza cuando las muestras empiezan a caer fuera de los limites de confianza
        /// </summary>
        const int maxBuffer = 500;
        Queue bufferCircular = new Queue();

        public void AgregarMuestraHistograma(float muestra)
        {
            if (Histograma.Length == 0)
                LimpiarHistograma();

            bufferCircular.Enqueue(muestra);
            if (bufferCircular.Count > maxBuffer)
                bufferCircular.Dequeue();

            IngresarValorABin(muestra);

            float cocienteIzq = (float)CantidadMuestrasFueraHistogramaIzquierda / CantidadMuestrasHistograma;
            float cocienteDer = (float)CantidadMuestrasFueraHistogramaDerecha / CantidadMuestrasHistograma;

            bool fueraIntervaloConfianza = cocienteIzq > histogramaConfianza || cocienteDer > histogramaConfianza;

            if (fueraIntervaloConfianza)
            {
                LimpiarHistograma();
                HistogramaDesde = Minimo;
                HistogramaHasta = Maximo;
                foreach (var item in bufferCircular)
                {
                    IngresarValorABin((float)item);
                }
            }
        }

        private float rango => HistogramaHasta - HistogramaDesde;
        private float tamanioBin;
        private int indiceBin = 0;
        private void IngresarValorABin(float muestra)
        {
            if (float.IsNaN(muestra))
                return;

            CantidadMuestrasHistograma++;

            if (muestra < HistogramaDesde)
            {
                CantidadMuestrasFueraHistogramaIzquierda++;
                return;
            }

            if (muestra > HistogramaHasta)
            {
                CantidadMuestrasFueraHistogramaDerecha++;
                return;
            }

            if (rango != 0)
            {
                tamanioBin = rango / Histograma.Length;
                indiceBin = (int)Math.Floor((muestra - HistogramaDesde) / tamanioBin);
                indiceBin = indiceBin == Histograma.Length ? Histograma.Length - 1 : indiceBin;
            }
            else
            {
                indiceBin = Histograma.Length / 2;
            }

            Histograma[indiceBin]++;
        }

        public void LimpiarRangos()
        {
            RangoMinimo = float.MinValue;
            RangoMaximo = float.MaxValue;
        }

        public void LimpiarHistograma()
        {
            Histograma = new uint[histogramaCantidadBins];
            CantidadMuestrasHistograma = 0;
            CantidadMuestrasFueraHistogramaIzquierda = 0;
            CantidadMuestrasFueraHistogramaDerecha = 0;
            HistogramaDesde = 0;
            HistogramaHasta = 0;
        }

        #endregion

    }
}

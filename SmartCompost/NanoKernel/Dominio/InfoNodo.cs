﻿using NanoKernel.Nodos;
using System;

namespace NanoKernel.Dominio
{
    public class InfoNodo
    {
        public DateTime FechaCompilacion { get; set; }
        public string HostCompilacion { get; set; }
        public string NumeroSerie { get; set; }
        public string TipoNodo { get; set; }
    }
}

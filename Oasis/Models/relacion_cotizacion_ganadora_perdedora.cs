
//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------


namespace Oasis.Models
{

using System;
    using System.Collections.Generic;
    
public partial class relacion_cotizacion_ganadora_perdedora
{

    public int id_cotizacion_ganadora { get; set; }

    public int id_cotizacion_perdedora { get; set; }

    public string memo { get; set; }

    public int estado { get; set; }

    public int id { get; set; }



    public virtual cotizacion_cabecera cotizacion_cabecera { get; set; }

    public virtual cotizacion_cabecera cotizacion_cabecera1 { get; set; }

}

}

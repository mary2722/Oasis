
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
    
public partial class Transito_PT_orden
{

    public int id_producto { get; set; }

    public string codigo { get; set; }

    public string nombre { get; set; }

    public Nullable<decimal> cantidad_a_fabricar { get; set; }

    public Nullable<decimal> cantidad_fabricada { get; set; }

    public Nullable<decimal> cantidad_pendiente { get; set; }

    public Nullable<decimal> rendimiento { get; set; }

}

}

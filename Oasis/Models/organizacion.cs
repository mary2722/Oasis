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
    
    public partial class organizacion
    {
        public int id_organizacion { get; set; }
        public Nullable<System.DateTime> fecha_creacion { get; set; }
        public Nullable<System.DateTime> fecha_modificacion { get; set; }
        public Nullable<int> id_entidad_origen { get; set; }
        public string usuario_creacion { get; set; }
        public string usuario_modificacion { get; set; }
        public string descripcion_notificacion { get; set; }
        public string identificacion { get; set; }
        public string identificacion_contador { get; set; }
        public string identificacion_representante { get; set; }
        public string imagen { get; set; }
        public bool indicador_notificacion { get; set; }
        public string nombre_comercial { get; set; }
        public string pagina_web { get; set; }
        public string razon_social { get; set; }
        public string representante_legal { get; set; }
        public Nullable<int> id_tipo_identificacion { get; set; }
        public Nullable<int> id_tipo_identificacion_contador { get; set; }
        public Nullable<int> id_tipo_identificacion_representante { get; set; }
    }
}

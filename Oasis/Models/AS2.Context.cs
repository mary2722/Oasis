﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class AS2Context : DbContext
    {
        public AS2Context()
            : base("name=AS2Context")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<factura_cliente> factura_cliente { get; set; }
        public virtual DbSet<detalle_forma_cobro> detalle_forma_cobro { get; set; }
        public virtual DbSet<empresa> empresa { get; set; }
        public virtual DbSet<factura_proveedorSRI> factura_proveedorSRI { get; set; }
        public virtual DbSet<organizacion> organizacion { get; set; }
        public virtual DbSet<usuario> usuario { get; set; }
        public virtual DbSet<direccion_empresa> direccion_empresa { get; set; }
        public virtual DbSet<ubicacion> ubicacion { get; set; }
        public virtual DbSet<categoria_empresa> categoria_empresa { get; set; }
    }
}

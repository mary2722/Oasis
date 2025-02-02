﻿using iTextSharp.text;
using iTextSharp.text.pdf;
using Oasis.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using static Oasis.Reporte;

namespace Oasis.Controllers
{
    public class CostosController : Controller
    {
        // GET: Costos
        //PRUEBA2
        public ActionResult Ordenes()
        {
            return View();
        }

        public ActionResult AnalisisCostos()
        {
            return View();
        }

        public ActionResult ManoObra()
        {
            return View();
        }
        public ActionResult ImprimirHojaCostos(int id_orden_fabricacion)
        {
            using(var db = new as2oasis())
            {
                var cabecera = db.Orden_Produccion
                    .Where(x => x.id_orden_fabricacion == id_orden_fabricacion)
                    .FirstOrDefault();

                var costos_mp_me = db.Costos_MP_ME
                    .Where(x => x.id_orden_fabricacion == id_orden_fabricacion);

                var costos_mod= db.Costos_MOD
                    .Where(x => x.id_orden_fabricacion == id_orden_fabricacion);

                var costos_indirectos= db.Costos_Indirectos
                    .Where(x => x.id_orden_fabricacion == id_orden_fabricacion);
                                
                using (MemoryStream myMemoryStream = new MemoryStream())
                {
                    Reporte R = new Reporte();
                    R.MemoryStream = myMemoryStream;
                    R.Empresa = "LABOVIDA S.A.";
                    R.tipo_reporte = Reporte.TipoReporte.Reporte;
                    float[] margenes = new float[] { 0f, 0f, 20f, 10f}; 
                    var doc = R.CrearDocA4(margenes);
                    var pdf = R.CrearPDF();
                    var hoy = DateTime.Now;
                    var suma_mod = 0.0;
                    var suma_horas_mod = 0.0;
                    var _costos_mod = 0.0;

                    var fuente_cabecera = R.CrearFuente("georgia", 10, 1, BaseColor.BLACK);
                    var fuente_cabecera_regular = R.CrearFuente("georgia", 10, 0, BaseColor.BLACK);
                    var fuente_tabla_detalle = R.CrearFuente("georgia", 8, 0, BaseColor.BLACK);
                    var fuente_tabla_cabecera = R.CrearFuente("georgia", 8, 1, BaseColor.BLACK);
                    
                    var _standardFont = FontFactory.GetFont("SEGOE UI", 15, Font.BOLD, BaseColor.BLACK);
                    var subtitulo = FontFactory.GetFont("SEGOE UI", 7, Font.BOLD, BaseColor.BLACK);
                    var encabezado_tabla = FontFactory.GetFont("SEGOE UI", 8,Font.BOLD, BaseColor.BLACK);
                    var detalle = FontFactory.GetFont("SEGOE UI", 7, Font.NORMAL, BaseColor.BLACK);

                    var MP = costos_mp_me.Where(x => x.codigo_categoria_producto.Replace("","") == "GRP01" ||
                            x.codigo_categoria_producto.Replace("", "") == "GRP05" || x.codigo_categoria_producto.Replace("", "") == "GRP04"
                            || x.codigo_categoria_producto.Replace("", "") == "GRP07");
                    //var MP = costos_mp_me.Where(x => x.codigo_categoria_producto == "GRP01" || x.codigo_categoria_producto == "GRP05"
                    //|| x.codigo_categoria_producto == "GRP07");
                    //|| x.codigo_categoria_producto == "GRP07");
                    //var ME = costos_mp_me.Where(x => x.codigo_categoria_producto == "GRP02" || x.codigo_categoria_producto == "GRP03");
                    var ME = costos_mp_me.Where(x => x.codigo_categoria_producto.Replace("", "") == "GRP02" ||
                            x.codigo_categoria_producto.Replace("", "") == "GRP03");

                    var suma_mp = MP.Sum(x => x.costo_total==null?0: x.costo_total)??0;
                    //var suma_mp = MP.Sum(x => x.costo_total) ?? 0;
                    var suma_me = ME.Sum(x => x.costo_total==null?0: x.costo_total) ??0;

                    if (costos_mod.Count() > 0)
                    {
                        _costos_mod = Convert.ToDouble(costos_mod.Select(x => x.costo_total).First() ?? 0);
                    }

                    suma_mod = _costos_mod;
                    //var suma_maq = costos_mod.Sum(x => x.costo_total)??0;
                    var suma_otros_costos = costos_indirectos.Sum(x => x.costo_total==null?0: x.costo_total) ??0;
                            
                    var suma_costos_total =  
                        suma_mp + suma_me + Convert.ToDecimal(suma_mod)  + suma_otros_costos;
                    var pnd_mp = string.Format("{0:0.00%}", ((suma_mp) / suma_costos_total));
                    var pnd_me = string.Format("{0:0.00%}", ((suma_me) / suma_costos_total));
                    var pnd_mod = string.Format("{0:0.00%}", ((Convert.ToDecimal(suma_mod)) / suma_costos_total));
                    //var pnd_maq = string.Format("{0:0.00%}", ((suma_maq) / suma_costos_total));
                    var pnd_otros_costos = string.Format("{0:0.00%}", ((suma_otros_costos) / suma_costos_total));
                    var costo_unitario = string.Format("{0:0.00000}",
                        suma_costos_total / cabecera.cantidad_fabricada);
                    //25-02-2022 presentacion producto I JD
                    var costo_unitario2 = string.Format("{0:0.00000}",
                        suma_costos_total / (cabecera.cantidad_fabricada * cabecera.presentacion_producto));
                    //25-02-2022 presentacion producto F JD

                    if (costos_mod.Count() > 0)
                    {
                        suma_horas_mod = Convert.ToDouble(costos_mod.Sum(x => x.horas_hombre));
                    }                 
                   

                    doc.AddTitle($"Hoja de costos #{cabecera.OP}");
                    doc.Open();

                    //doc.Add(R.ImagenFondo(empresa));
                    //500 totw
                    var encabezado = new PdfPTable(6)
                    {
                        LockedWidth = true,                        
                        TotalWidth = 500f,  
                        SpacingBefore = 5f,
                        SpacingAfter = 20f
                    };

                    encabezado.SetWidths(new float[] { 66f, 100f, 66f, 150f, 66f, 52f });
                    //encabezado.SetWidths(new float[] { 66f, 70f, 66f, 100f, 66f, 52f, 66f, 50f });

                    var table1 = new PdfPTable(3)
                    {
                        LockedWidth = true,
                        TotalWidth = 500f,
                        SpacingBefore = 5f
                    };

                    var _Titulo = new Chunk($"ANÁLISIS DE COSTO \n LOTE: {cabecera.lote} \n ",
                        _standardFont);
                    Paragraph __Titulo = new Paragraph(_Titulo);
                    __Titulo.Alignment = Element.ALIGN_CENTER;

                    table1.SetWidths(new float[] { 100f, 200f, 100f });

                    PdfPCell cell1 = new PdfPCell();
                    cell1 = new PdfPCell();

                    cell1.Padding = 0;
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    cell1.Border = PdfPCell.NO_BORDER;
                    table1.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase(_Titulo));
                    cell1.Padding = 0;
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                    cell1.Border = PdfPCell.NO_BORDER;

                    table1.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase("USUARIO: \n" + System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString() +
                        " \n "));
                    cell1.Padding = 0;
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
                    cell1.Border = PdfPCell.NO_BORDER;

                    table1.AddCell(cell1);

                    doc.Add(table1);

                    doc.Add(Chunk.NEWLINE);

                    cell1 = new PdfPCell(new Phrase("Cod. PT.:", subtitulo));
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    cell1.Border = PdfPCell.NO_BORDER;
                    encabezado.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase(cabecera.codigo_producto, detalle));
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    cell1.Border = PdfPCell.NO_BORDER;
                    encabezado.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase("PT.:", subtitulo));
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    cell1.Border = PdfPCell.NO_BORDER;
                    encabezado.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase(cabecera.descripcion_producto, detalle));
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    cell1.Border = PdfPCell.NO_BORDER;
                    encabezado.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase("O.P.:", subtitulo));
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    cell1.Border = PdfPCell.NO_BORDER;
                    encabezado.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase(cabecera.OP, detalle));
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    cell1.Border = PdfPCell.NO_BORDER;
                    encabezado.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase("Und. Elaboradas.:", subtitulo));
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    cell1.Border = PdfPCell.NO_BORDER;
                    encabezado.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase(String.Format("{0:n0}", cabecera.cantidad_fabricada), detalle));
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    cell1.Border = PdfPCell.NO_BORDER;
                    encabezado.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase("Costo unit.:", subtitulo));
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    cell1.Border = PdfPCell.NO_BORDER;
                    encabezado.AddCell(cell1);

                    //03-03-2022 JD I
                    cell1 = new PdfPCell(new Phrase(Convert.ToString(costo_unitario) + "   Costo unit2.: " + Convert.ToString(costo_unitario2), detalle));
                    //03-03-2022 JD F
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    cell1.Border = PdfPCell.NO_BORDER;
                    encabezado.AddCell(cell1);

                    //cell1 = new PdfPCell(new Phrase("Costo unit2.:", subtitulo));
                    //cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    //cell1.Border = PdfPCell.NO_BORDER;
                    //encabezado.AddCell(cell1);

                    //cell1 = new PdfPCell(new Phrase(costo_unitario2.ToString(), detalle));
                    //cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    //cell1.Border = PdfPCell.NO_BORDER;
                    //encabezado.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase("Costo total:", subtitulo));
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    cell1.Border = PdfPCell.NO_BORDER;
                    encabezado.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase(String.Format("{0:0.000000}", suma_costos_total), detalle));
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    cell1.Border = PdfPCell.NO_BORDER;
                    encabezado.AddCell(cell1);                    

                    cell1 = new PdfPCell(new Phrase("Und. a elaborar:", subtitulo));
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    cell1.Border = PdfPCell.NO_BORDER;
                    encabezado.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase(String.Format("{0:n0}", cabecera.cantidad), detalle));
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    cell1.Border = PdfPCell.NO_BORDER;
                    encabezado.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase("Rendimiento:", subtitulo));
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    cell1.Border = PdfPCell.NO_BORDER;
                    encabezado.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase((cabecera.cantidad_fabricada / cabecera.cantidad)*100 + "%", detalle));
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    cell1.Border = PdfPCell.NO_BORDER;
                    encabezado.AddCell(cell1);
                    
                    cell1 = new PdfPCell(new Phrase("U.M.:", subtitulo));
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    cell1.Border = PdfPCell.NO_BORDER;
                    encabezado.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase(cabecera.unidad_medida_op , detalle));
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    cell1.Border = PdfPCell.NO_BORDER;
                    encabezado.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase("Fecha inicio O.P.:", subtitulo));
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    cell1.Border = PdfPCell.NO_BORDER;
                    encabezado.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase(cabecera.Fecha_creacion_OP.ToString() , detalle));
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    cell1.Border = PdfPCell.NO_BORDER;
                    encabezado.AddCell(cell1);


                    cell1 = new PdfPCell(new Phrase("Fecha cierre O.P.:", subtitulo));
                    cell1.Border = PdfPCell.NO_BORDER;
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    encabezado.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase(cabecera.fecha_cierre.ToString(), detalle));
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    cell1.Border = PdfPCell.NO_BORDER;
                    encabezado.AddCell(cell1);


                    cell1 = new PdfPCell(new Phrase("Sucursal:", subtitulo));
                    cell1.Border = PdfPCell.NO_BORDER;
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    encabezado.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase(cabecera.planta.ToString(), detalle));
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    cell1.Border = PdfPCell.NO_BORDER;
                    encabezado.AddCell(cell1);

                    
                    //cell1 = new PdfPCell(new Phrase("", detalle));
                    //cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    //cell1.Border = PdfPCell.NO_BORDER;
                    //encabezado.AddCell(cell1);

                    //cell1 = new PdfPCell(new Phrase("", detalle));
                    //cell1.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    //cell1.Border = PdfPCell.NO_BORDER;
                    //encabezado.AddCell(cell1);

                    doc.Add(encabezado);

                    #region detalle
                    table1.FlushContent();
                    table1.SetWidths(new float[] { 150f, 150f, 100f });

                    var detalle_mp = new PdfPTable(8);
                    detalle_mp.LockedWidth = true;
                    detalle_mp.TotalWidth = 500f;
                    detalle_mp.SpacingBefore = 5f;
                    detalle_mp.SetWidths(new float[] { 50f, 205f, 20f,
                    45f,45f,45f,45f,45f});

                    PdfPCell cell_detalle = new PdfPCell();
                    cell_detalle.Padding = 0;
                    cell_detalle.Border = PdfPCell.NO_BORDER;


                    cell1 = new PdfPCell(new Phrase("MATERIA PRIMA"));
                    cell1.Padding = 0;
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                    cell1.PaddingBottom = 4f;
                    cell1.CellEvent = new RoundedBorder();
                    cell1.Border = PdfPCell.NO_BORDER;
                    //cell1.BackgroundColor = new BaseColor(53, 101, 255);

                    table1.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase(""));
                    cell1.Padding = 0;
                    cell1.Border = PdfPCell.NO_BORDER;

                    table1.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase(""));
                    cell1.Padding = 0;
                    cell1.Border = PdfPCell.NO_BORDER;

                    table1.AddCell(cell1);
                    
                    //if(MP.Count()>0)
                    if(suma_mp > 0)
                        doc.Add(table1);


                    detalle_mp.AddCell(new Phrase("Cod.", subtitulo));
                    detalle_mp.AddCell(new Phrase("Materia prima", subtitulo));
                    detalle_mp.AddCell(new Phrase("UM", subtitulo));
                    detalle_mp.AddCell(new Phrase("Receta", subtitulo));
                    detalle_mp.AddCell(new Phrase("Consumo", subtitulo));
                    detalle_mp.AddCell(new Phrase("Desperdicio", subtitulo));
                    detalle_mp.AddCell(new Phrase("Costo unit.", subtitulo));
                    detalle_mp.AddCell(new Phrase("Costo total", subtitulo));

                    foreach (var item in MP)
                    {
                        detalle_mp.AddCell(new Phrase( 
                            item.codigo_producto.ToString() , detalle));
                        detalle_mp.AddCell(new Phrase(item.producto, detalle));
                        detalle_mp.AddCell(new Phrase(item.unidad_medida, detalle));
                        detalle_mp.AddCell(new Phrase(string.Format("{0:n4}", item.receta), detalle));
                        detalle_mp.AddCell(new Phrase(string.Format("{0:n4}", item.consumo), detalle));
                        detalle_mp.AddCell(new Phrase(string.Format("{0:0.00%}", item.desperdicio ), detalle));
                        detalle_mp.AddCell(new Phrase(string.Format("{0:n6}", item.costo_unitario), detalle));
                        detalle_mp.AddCell(new Phrase(string.Format("{0:n4}", item.costo_total), detalle));
                    }

                    //if (MP.Count() > 0)
                    if (suma_mp > 0)
                        doc.Add(detalle_mp);
                    detalle_mp.FlushContent();
                    table1.FlushContent();

                    cell1 = new PdfPCell(new Phrase(""));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase(""));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase(""));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase(""));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase("Pnd.:", subtitulo));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase(pnd_mp, detalle));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase("Total M.P.:", subtitulo));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase(string.Format("{0:n4}", suma_mp), detalle));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);

                    //if (MP.Count() > 0)
                    if (suma_mp > 0)
                        doc.Add(detalle_mp);
                    detalle_mp.FlushContent();


                    cell1 = new PdfPCell(new Phrase("MATERIAL EMPAQUE"));
                    cell1.Padding = 0;
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                    cell1.PaddingBottom = 4f;
                    cell1.CellEvent = new RoundedBorder();
                    cell1.Border = PdfPCell.NO_BORDER;

                    table1.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase(""));
                    cell1.Padding = 0;
                    cell1.Border = PdfPCell.NO_BORDER;

                    table1.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase(""));
                    cell1.Padding = 0;
                    cell1.Border = PdfPCell.NO_BORDER;

                    table1.AddCell(cell1);

                    //if (ME.Count() > 0)
                    if (suma_me > 0)
                        doc.Add(table1); 

                    detalle_mp.AddCell(new Phrase("Cod.", subtitulo));
                    detalle_mp.AddCell(new Phrase("Materia empaque", subtitulo));
                    detalle_mp.AddCell(new Phrase("UM", subtitulo));
                    detalle_mp.AddCell(new Phrase("Receta", subtitulo));
                    detalle_mp.AddCell(new Phrase("Consumo", subtitulo));
                    detalle_mp.AddCell(new Phrase("Desperdicio", subtitulo));
                    detalle_mp.AddCell(new Phrase("Costo unit.", subtitulo));
                    detalle_mp.AddCell(new Phrase("Costo total", subtitulo));

                    foreach (var item in ME)
                    {
                        detalle_mp.AddCell(new Phrase(
                            item.codigo_producto.ToString(), detalle));
                        detalle_mp.AddCell(new Phrase(item.producto, detalle));
                        detalle_mp.AddCell(new Phrase(item.unidad_medida, detalle));
                        detalle_mp.AddCell(new Phrase(string.Format("{0:n4}", item.receta), detalle));
                        detalle_mp.AddCell(new Phrase(string.Format("{0:n4}", item.consumo), detalle));
                        detalle_mp.AddCell(new Phrase(string.Format("{0:0.00%}", item.desperdicio), detalle));
                        detalle_mp.AddCell(new Phrase(string.Format("{0:n6}", item.costo_unitario), detalle));
                        detalle_mp.AddCell(new Phrase(string.Format("{0:n4}", item.costo_total), detalle));
                    }

                    //if (ME.Count() > 0)
                    if (suma_me > 0)
                        doc.Add(detalle_mp);
                    detalle_mp.FlushContent();
                    table1.FlushContent();

                    cell1 = new PdfPCell(new Phrase(""));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase(""));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase(""));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase(""));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase("Pnd.:", subtitulo));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase(pnd_me, detalle));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase("Total M.e.:", subtitulo));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase(string.Format("{0:n4}", suma_me), detalle));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);

                    //if (ME.Count() > 0)
                    if (suma_me > 0)
                        doc.Add(detalle_mp);
                    detalle_mp.FlushContent();

                    var detalle_datos = new PdfPTable(7);
                    detalle_datos.LockedWidth = true;
                    detalle_datos.TotalWidth = 500f;
                    detalle_datos.SpacingBefore = 5f;
                    detalle_datos.SetWidths(new float[] { 90f, 210f, 20f,
                    45f,45f,45f, 45f});

                    cell1 = new PdfPCell(new Phrase("MANO DE OBRA DIRECTA"));
                    cell1.Padding = 0;
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                    cell1.PaddingBottom = 4f;
                    cell1.CellEvent = new RoundedBorder();
                    cell1.Border = PdfPCell.NO_BORDER;

                    table1.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase(""));
                    cell1.Padding = 0;
                    cell1.Border = PdfPCell.NO_BORDER;

                    table1.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase(""));
                    cell1.Padding = 0;
                    cell1.Border = PdfPCell.NO_BORDER;

                    table1.AddCell(cell1);

                    if (costos_mod.Count() > 0)
                        doc.Add(table1);

                    detalle_datos.AddCell(new Phrase("Cod. ", subtitulo));
                    detalle_datos.AddCell(new Phrase("Descripción", subtitulo));
                    detalle_datos.AddCell(new Phrase("UM", subtitulo));
                    detalle_datos.AddCell(new Phrase("Tiempo Est.", subtitulo));
                    detalle_datos.AddCell(new Phrase("Consumo", subtitulo));
                    detalle_datos.AddCell(new Phrase("Costo unit.", subtitulo));
                    detalle_datos.AddCell(new Phrase("Costo total", subtitulo));

                    foreach (var item in costos_mod)
                    {
                        var costo_total_mod = (item.costo_total / Convert.ToDecimal(suma_horas_mod))* item.horas_hombre;
                        var costo_unitario_mod = (item.costo_total / Convert.ToDecimal(suma_horas_mod));
                        detalle_datos.AddCell(new Phrase(item.operacion, detalle));
                        detalle_datos.AddCell(new Phrase(item.nombre_maquina, detalle));
                        detalle_datos.AddCell(new Phrase("HH", detalle));
                        detalle_datos.AddCell(new Phrase(string.Format("{0:n4}", item.horas_standard), detalle));
                        detalle_datos.AddCell(new Phrase(string.Format("{0:n4}", item.horas_hombre), detalle));
                        detalle_datos.AddCell(new Phrase(string.Format("{0:n6}", costo_unitario_mod), detalle));
                        detalle_datos.AddCell(new Phrase(string.Format("{0:n4}", costo_total_mod), detalle));
                    }

                    if (costos_mod.Count() > 0)
                        doc.Add(detalle_datos);

                    detalle_datos.FlushContent(); 
                    table1.FlushContent();

                    cell1 = new PdfPCell(new Phrase(""));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase(""));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase(""));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase(""));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase("Pnd.:", subtitulo));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase(pnd_mod, detalle));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase("Total MOD:", subtitulo));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase(string.Format("{0:n4}", _costos_mod), detalle));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);


                    if (costos_mod.Count() > 0)
                        doc.Add(detalle_mp);
                    detalle_mp.FlushContent();

                    var detalle_otros = new PdfPTable(6);
                    detalle_otros.LockedWidth = true;
                    detalle_otros.TotalWidth = 500f;
                    detalle_otros.SpacingBefore = 5f;
                    detalle_otros.SetWidths(new float[] { 90f, 210f, 20f,
                    45f,45f,45f});

                    cell1 = new PdfPCell(new Phrase("OTROS COSTOS"));
                    cell1.Padding = 0;
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                    cell1.PaddingBottom = 4f;
                    cell1.CellEvent = new RoundedBorder();
                    cell1.Border = PdfPCell.NO_BORDER;

                    table1.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase(""));
                    cell1.Padding = 0;
                    cell1.Border = PdfPCell.NO_BORDER;

                    table1.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase(""));
                    cell1.Padding = 0;
                    cell1.Border = PdfPCell.NO_BORDER;

                    table1.AddCell(cell1);

                    doc.Add(table1);

                    detalle_otros.AddCell(new Phrase("Cod. ", subtitulo));
                    detalle_otros.AddCell(new Phrase("Descripción", subtitulo));
                    detalle_otros.AddCell(new Phrase("UM", subtitulo));
                    detalle_otros.AddCell(new Phrase("Consumo", subtitulo));
                    detalle_otros.AddCell(new Phrase("Costo unit.", subtitulo));
                    detalle_otros.AddCell(new Phrase("Costo total", subtitulo));

                    foreach (var item in costos_indirectos)
                    {
                        detalle_otros.AddCell(new Phrase("CIF", detalle));
                        detalle_otros.AddCell(new Phrase("COSTOS INDIRECTOS", detalle));
                        detalle_otros.AddCell(new Phrase("UP", detalle));
                        detalle_otros.AddCell(new Phrase(string.Format("{0:n2}", item.cantidad), detalle));
                        detalle_otros.AddCell(new Phrase(string.Format("{0:n6}", item.costo_unitario), detalle));
                        detalle_otros.AddCell(new Phrase(string.Format("{0:n4}", item.costo_total), detalle));
                    }

                    doc.Add(detalle_otros);
                    detalle_otros.FlushContent();
                    table1.FlushContent();

                    cell1 = new PdfPCell(new Phrase(""));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase(""));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase(""));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase(""));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase("Pnd.:", subtitulo));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase(pnd_otros_costos, detalle));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase("Total Otros:", subtitulo));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);
                    cell1 = new PdfPCell(new Phrase(string.Format("{0:n4}", suma_otros_costos), detalle));
                    cell1.Border = PdfPCell.NO_BORDER;
                    detalle_mp.AddCell(cell1);

                    doc.Add(detalle_mp);
                    detalle_mp.FlushContent();

                    #region Comentario
                    //cell1 = new PdfPCell(new Phrase("PROCESOS Y MAQUINARIAS"));
                    //cell1.Padding = 0;
                    //cell1.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                    //cell1.PaddingBottom = 4f;
                    //cell1.CellEvent = new RoundedBorder();
                    //cell1.Border = PdfPCell.NO_BORDER;

                    //table1.AddCell(cell1);

                    //cell1 = new PdfPCell(new Phrase(""));
                    //cell1.Padding = 0;
                    //cell1.Border = PdfPCell.NO_BORDER;

                    //table1.AddCell(cell1);

                    //cell1 = new PdfPCell(new Phrase(""));
                    //cell1.Padding = 0;
                    //cell1.Border = PdfPCell.NO_BORDER;

                    //table1.AddCell(cell1);


                    //doc.Add(table1);

                    //detalle_datos.AddCell(new Phrase("Cod. ", subtitulo));
                    //detalle_datos.AddCell(new Phrase("Descripción", subtitulo));
                    //detalle_datos.AddCell(new Phrase("UM", subtitulo));
                    //detalle_datos.AddCell(new Phrase("Receta", subtitulo));
                    //detalle_datos.AddCell(new Phrase("Consumo", subtitulo));
                    //detalle_datos.AddCell(new Phrase("Desperdicio", subtitulo));

                    ////foreach (var item in MAQ)
                    ////{
                    ////    detalle_datos.AddCell(new Phrase(item.CODIGO_PRODUCTO.ToString(), detalle));
                    ////    detalle_datos.AddCell(new Phrase(item.DESCRIPCION_PRODUCTO, detalle));
                    ////    detalle_datos.AddCell(new Phrase(item.UM, detalle));
                    ////    detalle_datos.AddCell(new Phrase(string.Format("{0:n4}", item.RECETA), detalle));
                    ////    detalle_datos.AddCell(new Phrase(string.Format("{0:n4}", item.CONSUMO), detalle));
                    ////    detalle_datos.AddCell(new Phrase(string.Format("{0:0.00%}", item.DESPERDICIO), detalle));
                    ////}

                    //doc.Add(detalle_datos);
                    //detalle_datos.FlushContent();
                    #endregion
                    #endregion

                    #region NOTA     

                    var tabla_nota = new PdfPTable(1);
                    tabla_nota.LockedWidth = true;
                    tabla_nota.TotalWidth = 500f;
                    tabla_nota.SpacingBefore = 25f;
                    tabla_nota.SetWidths(new float[] { 250f});

                    cell1 = new PdfPCell(new Phrase("NOTA:"));
                    cell1.Padding = 0;
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                    cell1.PaddingBottom = 4f;
                    cell1.CellEvent = new RoundedBorder();
                    cell1.Border = PdfPCell.NO_BORDER;

                    table1.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase(""));
                    cell1.Padding = 0;
                    cell1.Border = PdfPCell.NO_BORDER;

                    table1.AddCell(cell1);

                    doc.Add(table1);

                    tabla_nota.AddCell(new Phrase("Nota ", subtitulo));
                    tabla_nota.AddCell(new Phrase(cabecera.descripcion_op, detalle));
                    doc.Add(tabla_nota);
                    tabla_nota.FlushContent();
                    table1.FlushContent();
                    #endregion

                    #region FIRMAS 
                    var tabla_firma = new PdfPTable(2);
                    tabla_firma.LockedWidth = true;
                    tabla_firma.TotalWidth = 500f;
                    tabla_firma.SpacingBefore = 25f;
                    tabla_firma.SetWidths(new float[] { 250f, 250f });

                    cell1 = new PdfPCell(new Phrase("________________"));
                    cell1.Padding = 0;
                    cell1.Border = PdfPCell.NO_BORDER;
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                    tabla_firma.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase("________________"));
                    cell1.Padding = 0;
                    cell1.Border = PdfPCell.NO_BORDER;
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                    tabla_firma.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase("REVISADO \n Departamento de costos"));
                    cell1.Padding = 5f;
                    cell1.Border = PdfPCell.NO_BORDER;
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                    tabla_firma.AddCell(cell1);

                    cell1 = new PdfPCell(new Phrase("APROBADO"));
                    cell1.Padding = 5f;
                    cell1.Border = PdfPCell.NO_BORDER;
                    cell1.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                    tabla_firma.AddCell(cell1);

                    doc.Add(tabla_firma);
                    #endregion
                    doc.Close();

                    var pdf_generado = R.GenerarPDF();

                    byte[] file = pdf_generado;
                    MemoryStream output = new MemoryStream();
                    output.Write(file, 0, file.Length);
                    output.Position = 0;

                    return new FileStreamResult(output, "application/pdf");
                }
            }
            
        }

        public ActionResult ObtenerOrdenes(string fecha_inicio, string fecha_fin)
        {
            var _fecha_inicio = Convert.ToDateTime(fecha_inicio);
            var _fecha_fin = Convert.ToDateTime(fecha_fin);
            using (var db = new as2oasis())
            {

                return Json(db.Orden_Produccion
                    .Where(x => x.fecha_cierre >= _fecha_inicio &&
                    x.fecha_cierre <= _fecha_fin &&
                    x.fecha_cierre != null)
                    .GroupBy(x => new
                    {
                        x.lote,
                        x.fecha_cierre,
                        x.codigo_producto,
                        x.descripcion_producto,
                        x.planta,
                        x.cantidad,
                        x.cantidad_fabricada,
                        x.id_orden_fabricacion,
                        x.OP
                    })
                    .ToList()
                    .Select(x => new
                    {
                        x.Key.OP,
                        x.Key.lote,
                        fecha = x.Key.fecha_cierre.Value.ToShortDateString(),
                        codigo = x.Key.codigo_producto,
                        nombre = x.Key.descripcion_producto,
                        sucursal = x.Key.planta,
                        x.Key.cantidad,
                        x.Key.cantidad_fabricada,
                        rendimiento = (x.Key.cantidad_fabricada / x.Key.cantidad) * 100,
                        x.Key.id_orden_fabricacion
                    })
                    .OrderBy(x => x.fecha),
                    JsonRequestBehavior.AllowGet);

                //IQueryable<Orden_Produccion> data = db.Orden_Produccion.Where(
                //   x => x.fecha_cierre >= _fecha_inicio &&
                //   x.fecha_cierre <= _fecha_fin
                //   );

                //var data_json = Json(
                //   data
                //   .ToList()
                //   .Select(x => new
                //   {
                //       x.OP,
                //       x.lote,
                //       fecha = x.fecha_cierre.Value.ToShortDateString(),
                //       codigo = x.codigo_producto,
                //       nombre = x.descripcion_producto,
                //       sucursal = x.planta,
                //       x.cantidad,
                //       x.cantidad_fabricada,
                //       rendimiento = (x.cantidad_fabricada / x.cantidad) * 100,
                //       x.id_orden_fabricacion

                //   }), JsonRequestBehavior.AllowGet
                //   );

                //data_json.MaxJsonLength = 500000000;

                //return data_json;

            }
        }

        [HttpPost]
        public JsonResult ObtenerCostos(string fecha_desde, string fecha_hasta, string empresa)
        {

            using (var db = new as2oasis())
            {
                var _fecha_inicio = Convert.ToDateTime(fecha_desde);
                var _fecha_fin = Convert.ToDateTime(fecha_hasta);

                IQueryable<Reporte_Analisis_Costos> data = db.Reporte_Analisis_Costos.Where(
                    x => x.fecha_cierre >= _fecha_inicio &&
                    x.fecha_cierre <= _fecha_fin
                    );


                var data_json = Json(
                    data
                    .ToList()
                    .Select(x => new
                    {
                        x.empresa,
                        x.mes_cierre,
                        x.orden_pro,
                        x.lote,
                        x.planta,
                        x.tipo_orden,
                        x.codigo_pro,
                        x.producto,
                        x.cant_elaborar,
                        x.cantidad_fabricada,
                        x.cajas_elaboradas,
                        costo_un = ((x.costo_mp==null?0: x.costo_mp) + (x.costo_me==null?0: x.costo_me) + 
                                   (x.costo_mod==null?0: x.costo_mod) + (x.costo_otros==null?0: x.costo_otros)) /x.cantidad_fabricada,
                        costo_caja = ((x.costo_mp == null ? 0 : x.costo_mp) + (x.costo_me == null ? 0 : x.costo_me) +
                                   (x.costo_mod == null ? 0 : x.costo_mod) + (x.costo_otros == null ? 0 : x.costo_otros)) / x.cantidad_fabricada,
                        costo_total = (x.costo_mp == null ? 0 : x.costo_mp) + (x.costo_me == null ? 0 : x.costo_me) +
                                   (x.costo_mod == null ? 0 : x.costo_mod) + (x.costo_otros == null ? 0 : x.costo_otros),
                        costo_mp = x.costo_mp==null?0: x.costo_mp,
                        costo_me= x.costo_me==null?0: x.costo_me,
                        costo_mod = x.costo_mod==null?0: x.costo_mod,
                        costo_otros = x.costo_otros==null?0: x.costo_otros,
                        x.rendimiento
                    }), JsonRequestBehavior.AllowGet
                    );

                data_json.MaxJsonLength = 500000000;

                return data_json;
            }
        }

        [HttpPost]
        public JsonResult ObtenerManoObra(string fecha_desde, string fecha_hasta, string empresa)
        {
            DateTime fecha_desde_ = DateTime.Parse(fecha_desde);
            DateTime fecha_hasta_ = DateTime.Parse(fecha_hasta);

            if (empresa == "0")
            {
                empresa = "";
            }

            using (var context = new as2oasis())
            {

                //var datos = context.SP_Mano_Obra(empresa, fecha_desde_, fecha_hasta_);

                //var datos_json = JsonConvert.SerializeObject(datos, Formatting.Indented);
                //var json_data = Json(datos_json, JsonRequestBehavior.AllowGet);
                //json_data.MaxJsonLength = 50000000;
                //return json_data;

                IQueryable<Mano_Obra> data = context.Mano_Obra.Where(
                   x => x.fecha >= fecha_desde_ &&
                   x.fecha <= fecha_hasta_
                   );

                if (empresa != "0")
                {
                    data = data.Where(x => x.empresa == empresa);
                }

                var data_json = Json(
                    data
                    .ToList()
                    .Select(x => new
                    {
                        x.empresa,
                        x.planta,
                        x.orden_fa,
                        //fecha = x.fecha.ToString("yyyy-MM-dd"),
                        fecha = (x.fecha==null?null:x.fecha.ToString("yyyy-MM-dd")),
                        x.codigo_pro,
                        x.producto,
                        x.lote,
                        x.cantidad_fabricada,
                        x.cod_tarea,
                        x.tarea,
                        x.maquina,
                        x.numero_maquinas,
                        x.numero_persona,
                        x.horas_hombre_es,
                        x.horas_maquina,
                        x.horas_hombre,
                        x.responsable
                    }), JsonRequestBehavior.AllowGet
                    ); 

                data_json.MaxJsonLength = 50000000;

                return data_json;

            }

        }

    }
}

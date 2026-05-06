using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _02_MVC.Models;

namespace _02_MVC.Controllers
{
    [Authorize(Roles = "SuperAdmin, Administrador, Paciente")]
    public class pacienteController : Controller
    {
        private ProyectoVeris_MVC_BDEntities db = new ProyectoVeris_MVC_BDEntities();

        // GET: paciente
        public ActionResult Index()
        {
            var pacientes = db.pacientes.Include(p => p.AspNetUsers);
            return View(pacientes.ToList());
        }

        // GET: paciente/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            pacientes pacientes = db.pacientes.Find(id);
            if (pacientes == null)
            {
                return HttpNotFound();
            }
            return View(pacientes);
        }

        // GET: paciente/Create
        public ActionResult Create()
        {
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers, "Id", "Email");
            CargarFotosPaciente();
            return View();
        }

        // POST: paciente/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdPaciente,IdUsuario,Nombre,Cedula,Edad,Genero,Estatura,Peso,Foto")] pacientes pacientes)
        {
            if (ModelState.IsValid)
            {
                db.pacientes.Add(pacientes);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IdUsuario = new SelectList(db.AspNetUsers, "Id", "Email", pacientes.IdUsuario);
            return View(pacientes);
        }

        // GET: paciente/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            pacientes pacientes = db.pacientes.Find(id);
            if (pacientes == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers, "Id", "Email", pacientes.IdUsuario);
            CargarFotosPaciente(pacientes.Foto);
            return View(pacientes);
        }

        // POST: paciente/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdPaciente,IdUsuario,Nombre,Cedula,Edad,Genero,Estatura,Peso,Foto")] pacientes pacientes)
        {
            if (ModelState.IsValid)
            {
                db.Entry(pacientes).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers, "Id", "Email", pacientes.IdUsuario);
            return View(pacientes);
        }

        // GET: paciente/Delete/5
        [Authorize(Roles = "SuperAdmin, Administrador")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            pacientes pacientes = db.pacientes.Find(id);
            if (pacientes == null)
            {
                return HttpNotFound();
            }
            return View(pacientes);
        }

        // POST: paciente/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin, Administrador")]
        public ActionResult DeleteConfirmed(int id)
        {
            pacientes pacientes = db.pacientes.Find(id);
            db.pacientes.Remove(pacientes);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private void CargarFotosPaciente(string seleccionada = null)
        {
            string carpeta = Server.MapPath("~/imágenes/usuarios/");
            var imagenes = System.IO.Directory.Exists(carpeta)
                ? System.IO.Directory.GetFiles(carpeta)
                    .Select(f => System.IO.Path.GetFileName(f))
                    .ToList()
                : new List<string>();
            ViewBag.FotosPaciente = new SelectList(imagenes, seleccionada);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

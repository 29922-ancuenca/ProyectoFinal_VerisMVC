using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _02_MVC.Models;
using _02_MVC.Helpers;

namespace _02_MVC.Controllers
{
    [Authorize(Roles = "SuperAdmin, Administrador, Medico")]
    public class medicoController : Controller
    {
        private ProyectoVeris_MVC_BDEntities db = new ProyectoVeris_MVC_BDEntities();

        // GET: medico
        public ActionResult Index()
        {
            var usuario = SessionHelper.CurrentUser;
            if (usuario == null) return RedirectToAction("Login", "Account");

            IQueryable<medicos> medicos = db.medicos.Include(m => m.AspNetUsers);

            if (!User.IsInRole("SuperAdmin") && User.IsInRole("Medico"))
            {
                medicos = medicos.Where(m => m.IdUsuario == usuario.Id);
            }

            return View(medicos.ToList());
        }

        // GET: medico/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            medicos medicos = db.medicos.Find(id);
            if (medicos == null)
            {
                return HttpNotFound();
            }
            return View(medicos);
        }

        // GET: medico/Create
        [Authorize(Roles = "SuperAdmin, Administrador")]
        public ActionResult Create()
        {
            ViewBag.IdEspecialidad = new SelectList(db.especialidades, "IdEspecialidad", "Descripcion");
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers, "Id", "Email");
            CargarFotosMedico();
            return View();
        }

        // POST: medico/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin, Administrador")]
        public ActionResult Create([Bind(Include = "IdMedico,IdUsuario,IdEspecialidad,Nombre,Foto")] medicos medicos)
        {
            if (ModelState.IsValid)
            {
                db.medicos.Add(medicos);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IdEspecialidad = new SelectList(db.especialidades, "IdEspecialidad", "Descripcion", medicos.IdEspecialidad);
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers, "Id", "Email", medicos.IdUsuario);
            return View(medicos);
        }

        // GET: medico/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            medicos medicos = db.medicos.Find(id);
            if (medicos == null)
                return HttpNotFound();

            // Médico solo puede editar su propio perfil
            if (User.IsInRole("Medico") && !User.IsInRole("SuperAdmin") && !User.IsInRole("Administrador"))
            {
                var usuario = SessionHelper.CurrentUser;
                if (usuario == null || medicos.IdUsuario != usuario.Id)
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            ViewBag.IdEspecialidad = new SelectList(db.especialidades, "IdEspecialidad", "Descripcion", medicos.IdEspecialidad);
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers, "Id", "Email", medicos.IdUsuario);
            CargarFotosMedico(medicos.Foto);
            return View(medicos);
        }

        // POST: medico/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdMedico,IdUsuario,IdEspecialidad,Nombre,Foto")] medicos medicos)
        {
            // Médico solo puede editar su propio perfil
            if (User.IsInRole("Medico") && !User.IsInRole("SuperAdmin") && !User.IsInRole("Administrador"))
            {
                var usuario = SessionHelper.CurrentUser;
                if (usuario == null || medicos.IdUsuario != usuario.Id)
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            if (ModelState.IsValid)
            {
                db.Entry(medicos).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdEspecialidad = new SelectList(db.especialidades, "IdEspecialidad", "Descripcion", medicos.IdEspecialidad);
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers, "Id", "Email", medicos.IdUsuario);
            return View(medicos);
        }

        // GET: medico/Delete/5
        [Authorize(Roles = "SuperAdmin, Administrador")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            medicos medicos = db.medicos.Find(id);
            if (medicos == null)
            {
                return HttpNotFound();
            }
            return View(medicos);
        }

        // POST: medico/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin, Administrador")]
        public ActionResult DeleteConfirmed(int id)
        {
            medicos medicos = db.medicos.Find(id);
            db.medicos.Remove(medicos);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private void CargarFotosMedico(string seleccionada = null)
        {
            string carpeta = Server.MapPath("~/imágenes/médicos/");
            var imagenes = System.IO.Directory.Exists(carpeta)
                ? System.IO.Directory.GetFiles(carpeta)
                    .Select(f => System.IO.Path.GetFileName(f))
                    .ToList()
                : new List<string>();
            ViewBag.FotosMedico = new SelectList(imagenes, seleccionada);
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

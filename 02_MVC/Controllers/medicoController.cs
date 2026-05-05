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
    public class medicoController : Controller
    {
        private ProyectoVeris_MVC_BDEntities db = new ProyectoVeris_MVC_BDEntities();

        // GET: medico
        public ActionResult Index()
        {
            var medicos = db.medicos.Include(m => m.AspNetUsers).Include(m => m.especialidades);
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
        public ActionResult Create()
        {
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers, "Id", "Email");
            ViewBag.IdEspecialidad = new SelectList(db.especialidades, "IdEspecialidad", "Descripcion");
            return View();
        }

        // POST: medico/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdMedico,Nombre,IdEspecialidad,IdUsuario,Foto")] medicos medicos)
        {
            if (ModelState.IsValid)
            {
                db.medicos.Add(medicos);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IdUsuario = new SelectList(db.AspNetUsers, "Id", "Email", medicos.IdUsuario);
            ViewBag.IdEspecialidad = new SelectList(db.especialidades, "IdEspecialidad", "Descripcion", medicos.IdEspecialidad);
            return View(medicos);
        }

        // GET: medico/Edit/5
        public ActionResult Edit(int? id)
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
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers, "Id", "Email", medicos.IdUsuario);
            ViewBag.IdEspecialidad = new SelectList(db.especialidades, "IdEspecialidad", "Descripcion", medicos.IdEspecialidad);
            return View(medicos);
        }

        // POST: medico/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdMedico,Nombre,IdEspecialidad,IdUsuario,Foto")] medicos medicos)
        {
            if (ModelState.IsValid)
            {
                db.Entry(medicos).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers, "Id", "Email", medicos.IdUsuario);
            ViewBag.IdEspecialidad = new SelectList(db.especialidades, "IdEspecialidad", "Descripcion", medicos.IdEspecialidad);
            return View(medicos);
        }

        // GET: medico/Delete/5
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
        public ActionResult DeleteConfirmed(int id)
        {
            medicos medicos = db.medicos.Find(id);
            db.medicos.Remove(medicos);
            db.SaveChanges();
            return RedirectToAction("Index");
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

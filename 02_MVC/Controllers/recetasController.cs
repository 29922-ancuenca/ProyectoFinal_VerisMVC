using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _02_MVC.Models;
using Microsoft.AspNet.Identity;

namespace _02_MVC.Controllers
{
    [Authorize(Roles = "SuperAdmin, Medico, Paciente")]
    public class recetasController : Controller
    {
        private ProyectoVeris_MVC_BDEntities db = new ProyectoVeris_MVC_BDEntities();

        // GET: recetas
        public ActionResult Index()
        {
            IQueryable<recetas> recetas = db.recetas.Include(r => r.consultas).Include(r => r.medicamentos);

            if (User.IsInRole("Paciente"))
            {
                string userId = User.Identity.GetUserId();
                var paciente = db.pacientes.FirstOrDefault(p => p.IdUsuario == userId);
                if (paciente == null) return View(new List<recetas>());
                recetas = recetas.Where(r => r.consultas.IdPaciente == paciente.IdPaciente);
            }
            else if (User.IsInRole("Medico"))
            {
                string userId = User.Identity.GetUserId();
                var medico = db.medicos.FirstOrDefault(m => m.IdUsuario == userId);
                if (medico == null) return View(new List<recetas>());
                recetas = recetas.Where(r => r.consultas.IdMedico == medico.IdMedico);
            }

            return View(recetas.ToList());
        }

        // GET: recetas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            recetas recetas = db.recetas.Find(id);
            if (recetas == null)
            {
                return HttpNotFound();
            }
            return View(recetas);
        }

        // GET: recetas/Create
        [Authorize(Roles = "SuperAdmin, Medico")]
        public ActionResult Create()
        {
            ViewBag.IdConsulta = new SelectList(db.consultas, "IdConsulta", "Diagnostico");
            ViewBag.IdMedicamento = new SelectList(db.medicamentos, "IdMedicamento", "Nombre");
            return View();
        }

        // POST: recetas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin, Medico")]
        public ActionResult Create([Bind(Include = "IdReceta,IdConsulta,IdMedicamento,Cantidad")] recetas recetas)
        {
            if (ModelState.IsValid)
            {
                db.recetas.Add(recetas);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IdConsulta = new SelectList(db.consultas, "IdConsulta", "Diagnostico", recetas.IdConsulta);
            ViewBag.IdMedicamento = new SelectList(db.medicamentos, "IdMedicamento", "Nombre", recetas.IdMedicamento);
            return View(recetas);
        }

        // GET: recetas/Edit/5
        [Authorize(Roles = "SuperAdmin, Medico")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            recetas recetas = db.recetas.Find(id);
            if (recetas == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdConsulta = new SelectList(db.consultas, "IdConsulta", "Diagnostico", recetas.IdConsulta);
            ViewBag.IdMedicamento = new SelectList(db.medicamentos, "IdMedicamento", "Nombre", recetas.IdMedicamento);
            return View(recetas);
        }

        // POST: recetas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin, Medico")]
        public ActionResult Edit([Bind(Include = "IdReceta,IdConsulta,IdMedicamento,Cantidad")] recetas recetas)
        {
            if (ModelState.IsValid)
            {
                db.Entry(recetas).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdConsulta = new SelectList(db.consultas, "IdConsulta", "Diagnostico", recetas.IdConsulta);
            ViewBag.IdMedicamento = new SelectList(db.medicamentos, "IdMedicamento", "Nombre", recetas.IdMedicamento);
            return View(recetas);
        }

        // GET: recetas/Delete/5
        [Authorize(Roles = "SuperAdmin, Medico")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            recetas recetas = db.recetas.Find(id);
            if (recetas == null)
            {
                return HttpNotFound();
            }
            return View(recetas);
        }

        // POST: recetas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin, Medico")]
        public ActionResult DeleteConfirmed(int id)
        {
            recetas recetas = db.recetas.Find(id);
            db.recetas.Remove(recetas);
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

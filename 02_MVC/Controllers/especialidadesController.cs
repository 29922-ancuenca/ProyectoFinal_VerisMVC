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
    [Authorize(Roles = "SuperAdmin, Administrador")]
    public class especialidadesController : Controller
    {
        private ProyectoVeris_MVC_BDEntities db = new ProyectoVeris_MVC_BDEntities();

        // GET: especialidades
        public ActionResult Index()
        {
            return View(db.especialidades.ToList());
        }

        // GET: especialidades/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            especialidades especialidades = db.especialidades.Find(id);
            if (especialidades == null)
            {
                return HttpNotFound();
            }
            return View(especialidades);
        }

        // GET: especialidades/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: especialidades/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdEspecialidad,Descripcion,Franja_HI,Franja_HF")] especialidades especialidades)
        {
            var diasSeleccionados = Request.Form.GetValues("Dias");
            especialidades.Dias = diasSeleccionados != null ? string.Join("", diasSeleccionados) : "";

            ValidarFranjaHoraria(especialidades.Franja_HI, especialidades.Franja_HF);

            if (ModelState.IsValid)
            {
                db.especialidades.Add(especialidades);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(especialidades);
        }

        // GET: especialidades/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            especialidades especialidades = db.especialidades.Find(id);
            if (especialidades == null)
            {
                return HttpNotFound();
            }
            return View(especialidades);
        }

        // POST: especialidades/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdEspecialidad,Descripcion,Franja_HI,Franja_HF")] especialidades especialidades)
        {
            var diasSeleccionados = Request.Form.GetValues("Dias");
            especialidades.Dias = diasSeleccionados != null ? string.Join("", diasSeleccionados) : "";

            ValidarFranjaHoraria(especialidades.Franja_HI, especialidades.Franja_HF);

            if (ModelState.IsValid)
            {
                db.Entry(especialidades).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(especialidades);
        }

        private void ValidarFranjaHoraria(TimeSpan? hi, TimeSpan? hf)
        {
            var minimo = new TimeSpan(8, 0, 0);
            var maximo = new TimeSpan(18, 0, 0);

            if (hi == null || hf == null)
            {
                ModelState.AddModelError("", "Debe ingresar la hora de inicio y hora de fin.");
                return;
            }
            if (hi < minimo || hi > maximo)
                ModelState.AddModelError("Franja_HI", "La hora de inicio debe estar entre 08:00 y 18:00 (Matutina: 08:00 a 12:00 / Vespertina: 12:00 a 18:00).");
            if (hf < minimo || hf > maximo)
                ModelState.AddModelError("Franja_HF", "La hora de fin debe estar entre 08:00 y 18:00 (Matutina: 08:00 a 12:00 / Vespertina: 12:00 a 18:00).");
            if (hi >= hf)
                ModelState.AddModelError("", "La hora de inicio debe ser menor a la hora de fin.");
        }

        // GET: especialidades/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            especialidades especialidades = db.especialidades.Find(id);
            if (especialidades == null)
            {
                return HttpNotFound();
            }
            return View(especialidades);
        }

        // POST: especialidades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            especialidades especialidades = db.especialidades.Find(id);
            db.especialidades.Remove(especialidades);
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

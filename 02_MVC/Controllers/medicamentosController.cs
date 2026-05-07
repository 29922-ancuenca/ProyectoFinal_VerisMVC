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
    public class medicamentosController : Controller
    {
        private ProyectoVeris_MVC_BDEntities db = new ProyectoVeris_MVC_BDEntities();

        // GET: medicamentos
        public ActionResult Index()
        {
            return View(db.medicamentos.ToList());
        }

        // GET: medicamentos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            medicamentos medicamentos = db.medicamentos.Find(id);
            if (medicamentos == null)
            {
                return HttpNotFound();
            }
            return View(medicamentos);
        }

        // GET: medicamentos/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: medicamentos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdMedicamento,Nombre,Tipo")] medicamentos medicamentos)
        {
            if (ModelState.IsValid)
            {
                db.medicamentos.Add(medicamentos);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(medicamentos);
        }

        // GET: medicamentos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            medicamentos medicamentos = db.medicamentos.Find(id);
            if (medicamentos == null)
            {
                return HttpNotFound();
            }
            return View(medicamentos);
        }

        // POST: medicamentos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdMedicamento,Nombre,Tipo")] medicamentos medicamentos)
        {
            if (ModelState.IsValid)
            {
                db.Entry(medicamentos).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(medicamentos);
        }

        // GET: medicamentos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            medicamentos medicamentos = db.medicamentos.Find(id);
            if (medicamentos == null)
            {
                return HttpNotFound();
            }
            return View(medicamentos);
        }

        // POST: medicamentos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            medicamentos medicamentos = db.medicamentos.Find(id);
            db.medicamentos.Remove(medicamentos);
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

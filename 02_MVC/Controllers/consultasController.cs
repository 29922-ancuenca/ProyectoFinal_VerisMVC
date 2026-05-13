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
using Microsoft.AspNet.Identity;

namespace _02_MVC.Controllers
{
    [Authorize(Roles = "SuperAdmin, Medico, Paciente")]
    public class consultasController : Controller
    {
        private ProyectoVeris_MVC_BDEntities db = new ProyectoVeris_MVC_BDEntities();

        // GET: consultas
        public ActionResult Index()
        {
            var usuario = SessionHelper.CurrentUser;
            if (usuario == null) return RedirectToAction("Login", "Account");

            List<consultas> lista;

            if (User.IsInRole("SuperAdmin"))
            {
                lista = db.consultas
                    .Include(c => c.pacientes)
                    .Include(c => c.medicos)
                    .ToList();
            }
            else if (User.IsInRole("Medico"))
            {
                var medico = db.medicos.FirstOrDefault(m => m.IdUsuario == usuario.Id);
                if (medico == null) return View(new List<consultas>());
                lista = db.consultas
                    .Include(c => c.pacientes)
                    .Include(c => c.medicos)
                    .Where(c => c.IdMedico == medico.IdMedico)
                    .ToList();
            }
            else if (User.IsInRole("Paciente"))
            {
                var paciente = db.pacientes.FirstOrDefault(p => p.IdUsuario == usuario.Id);
                if (paciente == null) return View(new List<consultas>());
                lista = db.consultas
                    .Include(c => c.pacientes)
                    .Include(c => c.medicos)
                    .Where(c => c.IdPaciente == paciente.IdPaciente)
                    .ToList();
            }
            else
            {
                lista = new List<consultas>();
            }

            ViewBag.TotalEnBD = db.consultas.Count();
            return View(lista);
        }

        // GET: consultas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            consultas consultas = db.consultas.Find(id);
            if (consultas == null)
            {
                return HttpNotFound();
            }
            return View(consultas);
        }

        // GET: consultas/Create
        public ActionResult Create()
        {
            return RedirectToAction("Index", "AgendarCita");
        }

        // POST: consultas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdConsulta,IdMedico,IdPaciente,FechaConsulta,HI,HF,Diagnostico")] consultas consultas)
        {
            if (ModelState.IsValid)
            {
                db.consultas.Add(consultas);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IdPaciente = new SelectList(db.pacientes, "IdPaciente", "Nombre", consultas.IdPaciente);
            ViewBag.IdMedico = new SelectList(db.medicos, "IdMedico", "Nombre", consultas.IdMedico);
            return View(consultas);
        }

        // GET: consultas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            consultas consultas = db.consultas.Find(id);
            if (consultas == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdPaciente = new SelectList(db.pacientes, "IdPaciente", "Nombre", consultas.IdPaciente);
            ViewBag.IdMedico = new SelectList(db.medicos, "IdMedico", "Nombre", consultas.IdMedico);
            return View(consultas);
        }

        // POST: consultas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdConsulta,IdMedico,IdPaciente,FechaConsulta,HI,HF,Diagnostico")] consultas consultas)
        {
            if (ModelState.IsValid)
            {
                db.Entry(consultas).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdPaciente = new SelectList(db.pacientes, "IdPaciente", "Nombre", consultas.IdPaciente);
            ViewBag.IdMedico = new SelectList(db.medicos, "IdMedico", "Nombre", consultas.IdMedico);
            return View(consultas);
        }

        // GET: consultas/Atender/5
        [Authorize(Roles = "Medico, SuperAdmin")]
        public ActionResult Atender(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            consultas consulta = db.consultas
                .Include(c => c.pacientes)
                .Include(c => c.medicos)
                .FirstOrDefault(c => c.IdConsulta == id);

            if (consulta == null)
                return HttpNotFound();

            return View(consulta);
        }

        // POST: consultas/Atender/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Medico, SuperAdmin")]
        public ActionResult Atender(int id, string Diagnostico)
        {
            var consulta = db.consultas.Find(id);
            if (consulta == null)
                return HttpNotFound();

            consulta.Diagnostico = Diagnostico?.Trim();
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: consultas/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            consultas consultas = db.consultas.Find(id);
            if (consultas == null)
            {
                return HttpNotFound();
            }
            return View(consultas);
        }

        // POST: consultas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult DeleteConfirmed(int id)
        {
            consultas consultas = db.consultas.Find(id);
            db.consultas.Remove(consultas);
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

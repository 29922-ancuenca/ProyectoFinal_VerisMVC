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
    public class recetasController : Controller
    {
        private ProyectoVeris_MVC_BDEntities db = new ProyectoVeris_MVC_BDEntities();

        // GET: recetas
        public ActionResult Index()
        {
            var usuario = SessionHelper.CurrentUser;
            if (usuario == null) return RedirectToAction("Login", "Account");

            List<recetas> lista;

            if (User.IsInRole("SuperAdmin"))
            {
                lista = db.recetas
                    .Include(r => r.consultas)
                    .Include("consultas.pacientes")
                    .Include("consultas.medicos")
                    .Include(r => r.medicamentos)
                    .ToList();
            }
            else if (User.IsInRole("Medico"))
            {
                var medico = db.medicos.FirstOrDefault(m => m.IdUsuario == usuario.Id);
                if (medico == null) return View(new List<recetas>());
                lista = db.recetas
                    .Include(r => r.consultas)
                    .Include("consultas.pacientes")
                    .Include("consultas.medicos")
                    .Include(r => r.medicamentos)
                    .Where(r => r.consultas.IdMedico == medico.IdMedico)
                    .ToList();
            }
            else if (User.IsInRole("Paciente"))
            {
                var paciente = db.pacientes.FirstOrDefault(p => p.IdUsuario == usuario.Id);
                if (paciente == null) return View(new List<recetas>());
                lista = db.recetas
                    .Include(r => r.consultas)
                    .Include("consultas.pacientes")
                    .Include("consultas.medicos")
                    .Include(r => r.medicamentos)
                    .Where(r => r.consultas.IdPaciente == paciente.IdPaciente)
                    .ToList();
            }
            else
            {
                lista = new List<recetas>();
            }

            return View(lista);
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
            var usuario = SessionHelper.CurrentUser;
            if (usuario == null) return RedirectToAction("Login", "Account");

            List<pacientes> listaPacientes;

            if (User.IsInRole("SuperAdmin"))
            {
                listaPacientes = db.pacientes.ToList();
            }
            else if (User.IsInRole("Medico"))
            {
                var medico = db.medicos.FirstOrDefault(m => m.IdUsuario == usuario.Id);
                if (medico != null)
                {
                    var idsPacientes = db.consultas
                        .Where(c => c.IdMedico == medico.IdMedico)
                        .Select(c => c.IdPaciente)
                        .Distinct()
                        .ToList();
                    listaPacientes = db.pacientes
                        .Where(p => idsPacientes.Contains(p.IdPaciente))
                        .ToList();
                }
                else
                {
                    listaPacientes = new List<pacientes>();
                }
            }
            else
            {
                listaPacientes = db.pacientes.ToList();
            }

            ViewBag.Pacientes = new SelectList(listaPacientes, "IdPaciente", "Nombre");
            ViewBag.IdMedicamento = new SelectList(db.medicamentos, "IdMedicamento", "Nombre");
            return View();
        }

        // AJAX: consultas por paciente (filtradas por médico si aplica)
        [Authorize(Roles = "SuperAdmin, Medico")]
        public JsonResult GetConsultas(int idPaciente)
        {
            var usuario = SessionHelper.CurrentUser;
            IQueryable<consultas> query = db.consultas.Where(c => c.IdPaciente == idPaciente);

            if (User.IsInRole("Medico") && usuario != null)
            {
                var medico = db.medicos.FirstOrDefault(m => m.IdUsuario == usuario.Id);
                if (medico != null)
                    query = query.Where(c => c.IdMedico == medico.IdMedico);
            }

            var resultado = query.ToList().Select(c => new {
                id = c.IdConsulta,
                texto = c.FechaConsulta.ToString("dd/MM/yyyy") + "  " +
                        c.HI.ToString(@"hh\:mm") + " - " +
                        c.HF.ToString(@"hh\:mm")
            });

            return Json(resultado, JsonRequestBehavior.AllowGet);
        }

        // AJAX: diagnóstico de una consulta
        [Authorize(Roles = "SuperAdmin, Medico")]
        public JsonResult GetDiagnostico(int idConsulta)
        {
            var consulta = db.consultas.Find(idConsulta);
            if (consulta == null) return Json(new { diagnostico = "" }, JsonRequestBehavior.AllowGet);
            return Json(new { diagnostico = consulta.Diagnostico }, JsonRequestBehavior.AllowGet);
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
                var consulta = db.consultas.Find(recetas.IdConsulta);
                if (consulta == null || string.IsNullOrWhiteSpace(consulta.Diagnostico) || consulta.Diagnostico == "Pendiente")
                {
                    ModelState.AddModelError("", "La consulta seleccionada aún no ha sido atendida. Registre un diagnóstico antes de crear la receta.");
                }
                else
                {
                    // Médico solo puede crear recetas de sus propias consultas
                    if (User.IsInRole("Medico") && !User.IsInRole("SuperAdmin"))
                    {
                        var usuarioSesion = SessionHelper.CurrentUser;
                        var medico = db.medicos.FirstOrDefault(m => m.IdUsuario == usuarioSesion.Id);
                        if (medico == null || consulta.IdMedico != medico.IdMedico)
                            return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                    }

                    db.recetas.Add(recetas);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            var usuario = SessionHelper.CurrentUser;
            List<pacientes> listaPacientesError;
            if (User.IsInRole("SuperAdmin"))
            {
                listaPacientesError = db.pacientes.ToList();
            }
            else if (User.IsInRole("Medico") && usuario != null)
            {
                var medico = db.medicos.FirstOrDefault(m => m.IdUsuario == usuario.Id);
                if (medico != null)
                {
                    var ids = db.consultas.Where(c => c.IdMedico == medico.IdMedico).Select(c => c.IdPaciente).Distinct().ToList();
                    listaPacientesError = db.pacientes.Where(p => ids.Contains(p.IdPaciente)).ToList();
                }
                else listaPacientesError = new List<pacientes>();
            }
            else listaPacientesError = new List<pacientes>();

            ViewBag.Pacientes = new SelectList(listaPacientesError, "IdPaciente", "Nombre");
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
            ViewBag.IdConsulta = new SelectList(
                db.consultas.Include(c => c.pacientes).ToList()
                    .Select(c => new {
                        c.IdConsulta,
                        Descripcion = (c.pacientes != null ? c.pacientes.Nombre : "Sin paciente") + " — " + (c.Diagnostico ?? "")
                    }),
                "IdConsulta", "Descripcion", recetas.IdConsulta);
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
            ViewBag.IdConsulta = new SelectList(
                db.consultas.Include(c => c.pacientes).ToList()
                    .Select(c => new {
                        c.IdConsulta,
                        Descripcion = (c.pacientes != null ? c.pacientes.Nombre : "Sin paciente") + " — " + (c.Diagnostico ?? "")
                    }),
                "IdConsulta", "Descripcion", recetas.IdConsulta);
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

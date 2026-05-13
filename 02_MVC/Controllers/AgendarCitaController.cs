using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using _02_MVC.Models;
using _02_MVC.Helpers;

namespace _02_MVC.Controllers
{
    [Authorize(Roles = "SuperAdmin, Administrador, Paciente")]
    public class AgendarCitaController : Controller
    {
        private const int DURACION_CITA = 60; // minutos por cita

        private ProyectoVeris_MVC_BDEntities db = new ProyectoVeris_MVC_BDEntities();

        public ActionResult Index()
        {
            var usuario = SessionHelper.CurrentUser;
            if (usuario == null) return RedirectToAction("Login", "Account");

            ViewBag.Especialidades = new SelectList(db.especialidades.ToList(), "IdEspecialidad", "Descripcion");

            var appDb = new ApplicationDbContext();
            var rolesUsuario = appDb.Users
                .Where(u => u.Id == usuario.Id)
                .SelectMany(u => u.Roles)
                .Select(r => r.RoleId)
                .ToList();
            var nombresRoles = appDb.Roles
                .Where(r => rolesUsuario.Contains(r.Id))
                .Select(r => r.Name)
                .ToList();
            bool esAdmin = nombresRoles.Contains("SuperAdmin") || nombresRoles.Contains("Administrador");
            ViewBag.EsAdmin = esAdmin;

            if (esAdmin)
                ViewBag.Pacientes = new SelectList(db.pacientes.ToList(), "IdPaciente", "Nombre");

            return View();
        }

        // AJAX: médicos de una especialidad
        public JsonResult GetMedicos(int idEspecialidad)
        {
            var medicos = db.medicos
                .Where(m => m.IdEspecialidad == idEspecialidad)
                .Select(m => new { id = m.IdMedico, nombre = m.Nombre })
                .ToList();
            return Json(medicos, JsonRequestBehavior.AllowGet);
        }

        // AJAX: días disponibles de la semana + conteo de slots libres por día
        public JsonResult GetCalendario(int idMedico, int anio, int semana)
        {
            var medico = db.medicos.Include(m => m.especialidades)
                           .FirstOrDefault(m => m.IdMedico == idMedico);
            if (medico == null || medico.especialidades == null)
                return Json(new { diasDisponibles = new List<string>(), slots = new { } }, JsonRequestBehavior.AllowGet);

            string diasStr = medico.especialidades.Dias ?? "";
            var diasSemana = new List<int>();
            if (diasStr.Contains("L")) diasSemana.Add(1);
            if (diasStr.Contains("M")) diasSemana.Add(2);
            if (diasStr.Contains("X")) diasSemana.Add(3);
            if (diasStr.Contains("J")) diasSemana.Add(4);
            if (diasStr.Contains("V")) diasSemana.Add(5);

            var inicio = medico.especialidades.Franja_HI;
            var fin    = medico.especialidades.Franja_HF;
            int totalSlots = (int)((fin - inicio).TotalMinutes / DURACION_CITA);

            // Calcular el lunes de la semana ISO 8601 (calendario gregoriano)
            // La semana 1 es la que contiene el primer jueves del año (enero 4 siempre cae en semana 1)
            var enero4 = new DateTime(anio, 1, 4);
            int dow = (int)enero4.DayOfWeek;
            if (dow == 0) dow = 7; // domingo = 7 en ISO
            var lunes = enero4.AddDays(1 - dow).AddDays((semana - 1) * 7);
            var viernes = lunes.AddDays(4);

            var reservasPorDia = db.consultas
                .Where(c => c.IdMedico == idMedico
                         && c.FechaConsulta >= lunes
                         && c.FechaConsulta <= viernes)
                .GroupBy(c => DbFunctions.TruncateTime(c.FechaConsulta))
                .Select(g => new { fecha = g.Key, ocupados = g.Count() })
                .ToList();

            var diasDisponibles = new List<string>();
            var slotsLibres = new Dictionary<string, int>();

            for (int i = 0; i <= 4; i++) // lunes a viernes
            {
                var fecha = lunes.AddDays(i);
                if (!diasSemana.Contains((int)fecha.DayOfWeek)) continue;

                string key = fecha.ToString("yyyy-MM-dd");
                diasDisponibles.Add(key);

                var reserva = reservasPorDia.FirstOrDefault(r => r.fecha.HasValue && r.fecha.Value.Date == fecha.Date);
                slotsLibres[key] = totalSlots - (reserva != null ? reserva.ocupados : 0);
            }

            return Json(new { diasDisponibles, slotsLibres, totalSlots, lunesSemana = lunes.ToString("yyyy-MM-dd"), viernesSemana = viernes.ToString("yyyy-MM-dd") }, JsonRequestBehavior.AllowGet);
        }

        // AJAX: horas libres para un médico en una fecha
        public JsonResult GetHoras(int idMedico, string fecha)
        {
            var medico = db.medicos.Include(m => m.especialidades)
                           .FirstOrDefault(m => m.IdMedico == idMedico);
            if (medico == null || medico.especialidades == null)
                return Json(new List<string>(), JsonRequestBehavior.AllowGet);

            DateTime fechaDt;
            if (!DateTime.TryParse(fecha, out fechaDt))
                return Json(new List<string>(), JsonRequestBehavior.AllowGet);

            var horasOcupadas = db.consultas
                .Where(c => c.IdMedico == idMedico
                         && DbFunctions.TruncateTime(c.FechaConsulta) == fechaDt.Date)
                .Select(c => c.HI)
                .ToList();

            var slots = new List<string>();
            var slotActual = medico.especialidades.Franja_HI;
            var slotFin    = medico.especialidades.Franja_HF;

            while (slotActual < slotFin)
            {
                if (!horasOcupadas.Contains(slotActual))
                    slots.Add(slotActual.ToString(@"hh\:mm"));
                slotActual = slotActual.Add(TimeSpan.FromMinutes(DURACION_CITA));
            }

            return Json(slots, JsonRequestBehavior.AllowGet);
        }

        // POST: confirmar cita
        [HttpPost]
        public ActionResult Confirmar(int idMedico, string fecha, string hora, int? idPaciente)
        {
            var usuario = SessionHelper.CurrentUser;
            if (usuario == null)
                return Json(new { success = false, message = "Sesión expirada. Vuelve a iniciar sesión." });

            pacientes paciente;
            if (User.IsInRole("SuperAdmin") || User.IsInRole("Administrador"))
            {
                if (idPaciente == null)
                    return Json(new { success = false, message = "Debe seleccionar un paciente." });
                paciente = db.pacientes.Find(idPaciente.Value);
            }
            else
            {
                paciente = db.pacientes.FirstOrDefault(p => p.IdUsuario == usuario.Id);
            }

            if (paciente == null)
                return Json(new { success = false, message = "No se encontró el perfil de paciente." });

            DateTime fechaDt;
            if (!DateTime.TryParse(fecha, out fechaDt))
                return Json(new { success = false, message = "Fecha inválida." });

            // Validación 0: no se puede agendar en fechas pasadas
            if (fechaDt.Date < DateTime.Today)
                return Json(new { success = false, message = "No puedes agendar una cita en una fecha pasada." });

            TimeSpan horaTs  = TimeSpan.Parse(hora);
            TimeSpan horaFin = horaTs.Add(TimeSpan.FromMinutes(DURACION_CITA));

            // Validación 1: el paciente ya tiene cita a esa hora ese día
            bool conflictoPaciente = db.consultas.Any(c =>
                c.IdPaciente == paciente.IdPaciente &&
                DbFunctions.TruncateTime(c.FechaConsulta) == fechaDt.Date &&
                c.HI == horaTs);

            if (conflictoPaciente)
                return Json(new { success = false, message = "Ya tienes una cita agendada a esa hora ese día." });

            // Validación 2: el slot ya fue tomado por otro paciente
            bool ocupado = db.consultas.Any(c =>
                c.IdMedico == idMedico &&
                DbFunctions.TruncateTime(c.FechaConsulta) == fechaDt.Date &&
                c.HI == horaTs);

            if (ocupado)
                return Json(new { success = false, message = "Ese horario ya fue reservado. Por favor elige otra hora." });

            db.consultas.Add(new consultas
            {
                IdMedico      = idMedico,
                IdPaciente    = paciente.IdPaciente,
                FechaConsulta = fechaDt,
                HI            = horaTs,
                HF            = horaFin,
                Diagnostico   = "Pendiente"
            });
            db.SaveChanges();

            return Json(new { success = true, message = "¡Cita agendada exitosamente!" });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}

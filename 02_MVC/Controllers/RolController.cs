using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using _02_MVC.Models;

namespace _02_MVC.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class RolController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            var users = db.Users.ToList().Select(u => new UserRoleViewModel
            {
                Id = u.Id,
                Email = u.Email,
                Roles = string.Join(", ", userManager.GetRoles(u.Id))
            }).ToList();

            return View(users);
        }

        public ActionResult AsignarRol(string id)
        {
            if (id == null)
                return HttpNotFound();

            var user = db.Users.Find(id);
            if (user == null)
                return HttpNotFound();

            ViewBag.Roles = new SelectList(db.Roles, "Name", "Name");
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AsignarRol(string id, string rolSeleccionado)
        {
            using (var ctx = new ApplicationDbContext())
            {
                var user = ctx.Users.Include("Roles").FirstOrDefault(u => u.Id == id);
                if (user == null) return HttpNotFound();

                // Proteger al SuperAdmin — sus roles no se pueden modificar
                bool esSuperAdmin = user.Roles.Any(r => ctx.Roles.Any(ro => ro.Id == r.RoleId && ro.Name == "SuperAdmin"));
                if (esSuperAdmin)
                {
                    TempData["Error"] = "No se pueden modificar los roles del usuario SuperAdmin.";
                    return RedirectToAction("Index");
                }

                var role = ctx.Roles.FirstOrDefault(r => r.Name == rolSeleccionado);
                if (role == null) return HttpNotFound();

                user.Roles.Clear();
                user.Roles.Add(new IdentityUserRole { UserId = id, RoleId = role.Id });
                ctx.SaveChanges();

                TempData["Mensaje"] = $"Rol '{rolSeleccionado}' asignado a {user.Email}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QuitarRol(string id)
        {
            using (var ctx = new ApplicationDbContext())
            {
                var user = ctx.Users.Include("Roles").FirstOrDefault(u => u.Id == id);
                if (user == null) return HttpNotFound();

                // Proteger al SuperAdmin — sus roles no se pueden modificar
                bool esSuperAdmin = user.Roles.Any(r => ctx.Roles.Any(ro => ro.Id == r.RoleId && ro.Name == "SuperAdmin"));
                if (esSuperAdmin)
                {
                    TempData["Error"] = "No se pueden modificar los roles del usuario SuperAdmin.";
                    return RedirectToAction("Index");
                }

                string email = user.Email;
                user.Roles.Clear();
                ctx.SaveChanges();

                TempData["Mensaje"] = $"Se eliminaron todos los roles del usuario {email}.";
            }
            return RedirectToAction("Index");
        }
    }
}

using ArticlesApp.Data;
using ArticlesApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArticlesApp.Controllers
{
    [Authorize]
    public class BasketsController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public BasketsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }


        // toti utilizatorii pot vedea Basket-urile existente in platforma
        // fiecare utilizator vede basket-urile pe care le-a creat
        // HttpGet - implicit
        [Authorize(Roles = "User,Collaborator,Admin")]
        public IActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            SetAccessRights();

            if (User.IsInRole("User") || User.IsInRole("Collaborator"))
            {
                var baskets = from basket in db.Baskets.Include("User")
                               .Where(b => b.UserId == _userManager.GetUserId(User))
                                select basket;

                ViewBag.Baskets = baskets;

                return View();
            }
            else
            if (User.IsInRole("Admin"))
            {
                var baskets = from basket in db.Baskets.Include("User")
                                select basket;

                ViewBag.Baskets = baskets;

                return View();
            }

            else
            {
                TempData["message"] = "Nu aveti drepturi asupra cosului de cumparaturi";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Articles");
            }

        }

        // Afisarea tuturor produselor pe care utilizatorul le-a salvat in 
        // basket-ul sau 
        [Authorize(Roles = "User,Collaborator,Admin")]
        public IActionResult Show(int id)
        {
            SetAccessRights();

            if (User.IsInRole("User") || User.IsInRole("Collaborator"))
            {
                var baskets = db.Baskets
                                  .Include("ProductBaskets.Product.Category")
                                  .Include("ProductBaskets.Product.User")
                                  .Include("User")
                                  .Where(b => b.Id == id)
                                  .Where(b => b.UserId == _userManager.GetUserId(User))
                                  .FirstOrDefault();

                if (baskets == null)
                {
                    TempData["message"] = "Nu aveti drepturi";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index", "Products");
                }

                return View(baskets);
            }

            else
            if (User.IsInRole("Admin"))
            {
                var baskets = db.Baskets
                                  .Include("ProductBaskets.Product.Category")
                                  .Include("ProductBaskets.Product.User")
                                  .Include("User")
                                  .Where(b => b.Id == id)
                                  .FirstOrDefault();


                if (baskets == null)
                {
                    TempData["message"] = "Resursa cautata nu poate fi gasita";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index", "Products");
                }


                return View(baskets);
            }

            else
            {
                TempData["message"] = "Nu aveti drepturi";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }
        }


        [Authorize(Roles = "User,Collaborator,Admin")]
        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "User,Collaborator,Admin")]
        public ActionResult New(Basket bs)
        {
            bs.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Baskets.Add(bs);
                db.SaveChanges();
                TempData["message"] = "Cosul a fost adaugat";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }

            else
            {
                return View(bs);
            }
        }


        // Conditiile de afisare a butoanelor de editare si stergere
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("Collaborator") || User.IsInRole("User"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.EsteAdmin = User.IsInRole("Admin");

            ViewBag.UserCurent = _userManager.GetUserId(User);
        }
    }
}

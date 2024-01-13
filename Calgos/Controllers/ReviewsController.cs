using Calgos.Data;
using Calgos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Calgos.Controllers
{
    public class ReviewsController : Controller
    {

        // PASUL 10 - useri si roluri


        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public ReviewsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }


        public IActionResult Show(int id)
        {
            return View();
        }

        
        
        // Adaugarea unui review asociat unui produs in baza de date
        [HttpPost]
        public IActionResult New(Review rev)
        {
            rev.Date = DateTime.Now;

            if(ModelState.IsValid)
            {
                db.Reviews.Add(rev);
                db.SaveChanges();
                var reviews = db.Reviews.Where(r => r.ProductId == rev.ProductId).ToList();
                double averageRating = reviews.Average(r => r.Points);

                var product = db.Products.Find(rev.ProductId);
                product.Rating = averageRating;

                db.SaveChanges();
                return Redirect("/Products/Show/" + rev.ProductId);
            }

            else
            {
                return Redirect("/Products/Show/" + rev.ProductId);
            }

        }

        
        


        // Stergerea unui review asociat unui produs din baza de date
        // Se poate sterge reviewul doar de catre userii cu rolul Admin
        // sau de catre userii cu rolul User sau Collaborator doar daca reviewul
        // a fost lasat de acestia
        [HttpPost]
        [Authorize(Roles = "User,Collaborator,Admin")]
        public IActionResult Delete(int id)
        {
            Review rev = db.Reviews.Find(id);

            if (rev.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Reviews.Remove(rev);
                db.SaveChanges();
                var reviews = db.Reviews.Where(r => r.ProductId == rev.ProductId).ToList();
                if (reviews.Count != 0)
                {
                    double averageRating = reviews.Average(r => r.Points);
                    var product = db.Products.Find(rev.ProductId);
                    product.Rating = averageRating;
                }
                else
                {
                    var product = db.Products.Find(rev.ProductId);
                    product.Rating = 0;
                }
                db.SaveChanges();
                return Redirect("/Products/Show/" + rev.ProductId);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti review-ul";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }
        }

        
        [Authorize(Roles = "User,Collaborator,Admin")]
        public IActionResult Edit(int id)
        {
            Review rev = db.Reviews.Find(id);

            if (rev.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(rev);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati review-ul";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Collaborator,Admin")]
        public IActionResult Edit(int id, Review requestReview)
        {
            Review rev = db.Reviews.Find(id);

            if (rev.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                if (ModelState.IsValid)
                {
                    rev.Content = requestReview.Content;

                    rev.Points = requestReview.Points;

                    db.SaveChanges();

                    var reviews = db.Reviews.Where(r => r.ProductId == rev.ProductId).ToList();

                    if (reviews.Count != 0)
                    {
                        double averageRating = reviews.Average(r => r.Points);
                        var product = db.Products.Find(rev.ProductId);
                        product.Rating = averageRating;
                    }
                    else
                    {
                        var product = db.Products.Find(rev.ProductId);
                        product.Rating = 0;
                    }
                    db.SaveChanges();
                    return Redirect("/Products/Show/" + rev.ProductId);
                }
                else
                {
                    return View(requestReview);
                }
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }
        }
    }
}
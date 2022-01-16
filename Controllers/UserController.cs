﻿using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModernyzeWebsite.Data;
using ModernyzeWebsite.Models;

namespace ModernyzeWebsite.Controllers; 

public class UserController : Controller {
    private readonly ModernyzeWebsiteContext db;

    public UserController(ModernyzeWebsiteContext context) {
        this.db = context;
    }

    // GET: User
    public ActionResult Index() {
        return View();
    }
    
    // GET: Register
    public ActionResult Register() {
        return View();
    }

    // POST: Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(UserAccount user) {
        if (!this.ModelState.IsValid) {
            return View();
        }

        UserAccount existing = this.db.UserAccount.FirstOrDefault(s => s.Email == user.Email);
        if (existing != null) {
            this.ViewBag.error = "Email is already in use.";
            return View();
        }

        user.Password = GetMD5(user.Password);
        this.db.UserAccount.Add(user);
        await this.db.SaveChangesAsync();
        this.db.UserPermission.Add(new UserPermission {
            PermissionId = 2,
            UserId = user.Id
        });
        await this.db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    // GET: Login
    public ActionResult Login() {
        return View();
    }

    // POST: Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Login(string email, string password) {
        if (!this.ModelState.IsValid) {
            return View();
        }

        string passwordAttempt = GetMD5(password);
        List<UserAccount> accounts =
            this.db.UserAccount
                .Where(user => user.Email.Equals(email) && 
                               user.Password == passwordAttempt)
                .ToList();
        if (accounts.Count == 0) {
            this.ViewBag.error = "Login failed";
            return RedirectToAction("Login");
        }

        UserAccount currentUser = accounts.FirstOrDefault();
        this.HttpContext.Session.SetString("FullName", currentUser.FirstName + " " + currentUser.LastName);
        this.HttpContext.Session.SetString("UserId", currentUser.Id.ToString());
        return RedirectToAction("Index");
    }

    // GET: Logout
    public ActionResult Logout() {
        this.HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }

    /// <summary>
    /// Hash a string using the MD5 algorithm.
    /// </summary>
    /// <param name="str">The string to hash</param>
    /// <returns>The hashed string</returns>
    private static string GetMD5(string str) {
        MD5 md5 = MD5.Create();
        byte[] fromData = Encoding.UTF8.GetBytes(str);
        byte[] targetData = md5.ComputeHash(fromData);
        string byte2string = null;

        for (int i = 0; i < targetData.Length; i++) {
            byte2string += targetData[i].ToString("x2");
        }

        return byte2string;
    }
}
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModernyzeWebsite.Data;
using ModernyzeWebsite.Models;

namespace ModernyzeWebsite.Controllers;

public class UserController : Controller {
    public UserController(ModernyzeWebsiteContext context) {
        this.db = context;
    }

    #region Member Variables

    private const string ADMIN = "Administrator";
    private const string REGISTERED = "Registered User";
    private const string UNVERIFIED = "Unverified";

    private readonly ModernyzeWebsiteContext db;

    #endregion

    #region Endpoint Methods

    #region GET Methods

    // GET: User
    public ActionResult Index() {
        return View();
    }

    // GET: Register
    public ActionResult Register() {
        return View();
    }

    // GET: Admin User Panel
    public async Task<IActionResult> Admin() {
        return View(await this.db.UserAccount.ToListAsync());
    }

    // GET: Login
    public ActionResult Login() {
        return View();
    }

    // GET: Logout
    public ActionResult Logout() {
        this.HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }

    #endregion

    #region POST Methods

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
        int unverified = GetPermissionsID(UNVERIFIED);
        this.db.UserPermission.Add(new UserPermission {
            PermissionId = unverified,
            UserId = user.Id
        });
        await this.db.SaveChangesAsync();
        return RedirectToAction("Index");
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

        UserAccount currentUser = accounts.First();
        string role = (from up in this.db.UserPermission
                       join p in this.db.Permissions on up.PermissionId equals p.Id
                       orderby p.Id
                       where up.UserId == currentUser.Id
                       select p.Name).First();
        this.HttpContext.Session.SetString("FullName", currentUser.FullName);
        this.HttpContext.Session.SetString("HighestPermission", role);
        this.HttpContext.Session.SetString("UserId", currentUser.Id.ToString());
        return RedirectToAction("Index");
    }

    // POST: Verify
    [HttpPost]
    public ActionResult Verify(int userID) {
        // try to get unverified record from database for this user
        UserPermission unverified = GetUnverifiedRecord(userID);
        if (unverified == null) {
            // this user isn't unverified.
            return RedirectToAction("Admin");
        }

        // If we get here, we found an unverified record for this user.
        unverified.PermissionId = GetPermissionsID(REGISTERED);
        // Update the record with the new permission ID
        this.db.UserPermission.Update(unverified);
        this.db.SaveChanges();
        return RedirectToAction("Admin");
    }

    // POST: MakeAdmin
    [HttpPost]
    public ActionResult MakeAdmin(int userID) {
        // try to get the registered record from database for this user
        UserPermission registered = GetRegisteredRecord(userID);
        if (registered == null) {
            // this user doesn't have a registered record.
            return RedirectToAction("Admin");
        }

        // If we get here, we found that this user has the registered role.
        registered.PermissionId = GetPermissionsID(ADMIN);
        // Update the record with the new permission ID
        this.db.UserPermission.Update(registered);
        this.db.SaveChanges();
        return RedirectToAction("Admin");
    }

    #endregion

    #endregion

    #region Private Helper Functions

    /// <summary>
    ///     Hash a string using the MD5 algorithm.
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

    /// <summary>
    ///     Get the Permissions ID given a permission Name. This method does not
    ///     check if the permission name exists before trying to retrieve it.
    /// </summary>
    /// <param name="permissionName">The name of the permission</param>
    /// <returns>The primary key for the permission searched for.</returns>
    private int GetPermissionsID(string permissionName) {
        return this.db.Permissions.Where(p => p.Name == permissionName).First().Id;
    }

    /// <summary>
    /// Try to get the "unverified" user permission record for a given user.
    /// </summary>
    /// <param name="userID">The UserId for the given user.</param>
    /// <returns>A UserPermission object if found, otherwise this method returns null.</returns>
    private UserPermission? GetUnverifiedRecord(int userID) {
        int unverifiedID = GetPermissionsID(UNVERIFIED);
        return this.db.UserPermission
                   .Where(up => up.PermissionId == unverifiedID && up.UserId == userID)
                   .FirstOrDefault();
    }

    /// <summary>
    /// Try to get the "registered" user permission record for a given user.
    /// </summary>
    /// <param name="userID">The UserId for the given user.</param>
    /// <returns>A UserPermission object if found, otherwise this method returns null.</returns>
    private UserPermission? GetRegisteredRecord(int userID) {
        int registeredID = GetPermissionsID(REGISTERED);
        return this.db.UserPermission
                   .Where(up => up.PermissionId == registeredID && up.UserId == userID)
                   .FirstOrDefault();
    }

    #endregion
}

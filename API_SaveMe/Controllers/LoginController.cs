using API_SaveMe.HelperModels;
using API_SaveMe.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace API_SaveMe.Controllers
{
    public class LoginController : ApiController
    {
        private db_SaveMeTrackerEntities db = new db_SaveMeTrackerEntities();

        /// <summary>
        /// Verifies User Authentication
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        [Route("Login/Authenticate")]
        [HttpPost]
        public IHttpActionResult AuthenticateUser(Login loginModel)
        {
            AuthenticationResponse authenticationResponse = new AuthenticationResponse();
            LoginDetail loginDetails = db.LoginDetails.Where(e => e.UserName.Equals(loginModel.UserName) && e.UserPassword.Equals(loginModel.Password)).FirstOrDefault();
            if (loginDetails != null)
            {
                UserData userData = db.UserDatas.Where(u => u.UserDataID == loginDetails.UserDataID).FirstOrDefault();
                if (userData != null)
                {
                    authenticationResponse.IsAuthenticated = true;
                    User user = new User();
                    user.FirstName = userData.FirstName;
                    user.LastName = userData.LastName;
                    user.UserName = loginDetails.UserName;
                    user.Id = Convert.ToInt32(userData.UserDataID);
                    authenticationResponse.User = user;
                }
                return Ok(authenticationResponse);
            }
            else
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Update user password
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        [Route("Login/ChangePassword")]
        [HttpPost]
        public IHttpActionResult ChangePassword(Login loginModel)
        {
            LoginDetail loginDetails = db.LoginDetails.Where(e => e.UserDataID == loginModel.UserDataID).FirstOrDefault();
            if (loginDetails != null)
            {
                loginDetails.UserPassword = loginModel.Password;
                db.Entry(loginDetails).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    return Ok(true);
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
            }
            return BadRequest();
        }
    }
}

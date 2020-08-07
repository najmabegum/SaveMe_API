using API_SaveMe.HelperModels;
using API_SaveMe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace API_SaveMe.Controllers
{
    public class UserRegistrationController : ApiController
    {
        private db_SaveMeTrackerEntities db = new db_SaveMeTrackerEntities();

        /// <summary>
        /// Saves User registration and login data
        /// </summary>
        /// <param name="userRegistrationModel"></param>
        /// <returns></returns>
        [Route("UserRegistration/SaveUserRegistration")]
        [HttpPost]
        public IHttpActionResult SaveUserRegistration(UserRegistration userRegistrationModel)
        {
            AuthenticationResponse authenticationResponse = new AuthenticationResponse();
            LoginDetail loginDetail = new LoginDetail();
            int? userDataID = this.SaveUserData(userRegistrationModel);
            if (userDataID != null)
            {
                if (this.SaveUserLoginDetails(userRegistrationModel, Convert.ToInt32(userDataID)))
                {
                    authenticationResponse.IsAuthenticated = true;
                    User user = new User();
                    user.FirstName = userRegistrationModel.FirstName;
                    user.LastName = userRegistrationModel.LastName;
                    user.UserName = userRegistrationModel.UserName;
                    user.Id = Convert.ToInt32(userDataID);
                    authenticationResponse.User = user;
                    return Ok(authenticationResponse);
                }
            }
            return BadRequest();
        }

        /// <summary>
        /// Registers a user into the application
        /// </summary>
        /// <param name="userRegistrationModel"></param>
        /// <returns></returns>
        private int? SaveUserData(UserRegistration userRegistrationModel)
        {
            UserData userData = new UserData();

            userData.FirstName = !string.IsNullOrEmpty(userRegistrationModel.FirstName) ? userRegistrationModel.FirstName : null;
            userData.MiddleName = !string.IsNullOrEmpty(userRegistrationModel.MiddleName) ? userRegistrationModel.MiddleName : null;
            userData.LastName = !string.IsNullOrEmpty(userRegistrationModel.LastName) ? userRegistrationModel.LastName : null;
            userData.Gender = !string.IsNullOrEmpty(userRegistrationModel.Gender) ? userRegistrationModel.Gender : null;
            userData.Age = userRegistrationModel.Age > 0 ? userRegistrationModel.Age : 0;
            userData.MaritalStatus = !string.IsNullOrEmpty(userRegistrationModel.MaritalStatus) ? userRegistrationModel.MaritalStatus : null;
            userData.NumberMembersInFamily = userRegistrationModel.NumberMembersInFamily;
            userData.NumberKidsInFamily = userRegistrationModel.NumberKidsInFamily;
            userData.DateCreated = DateTime.UtcNow;
            userData.DateModiﬁed = DateTime.UtcNow;

            if (ModelState.IsValid || userData.Age != 0)
            {
                db.UserDatas.Add(userData);
                db.SaveChanges();
                return userData.UserDataID;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Saves the Login credentials of a user
        /// </summary>
        /// <param name="userRegistrationModel"></param>
        /// <param name="userDataID"></param>
        /// <returns></returns>
        private bool SaveUserLoginDetails(UserRegistration userRegistrationModel, int userDataID = 0)
        {
            bool isLoginDataSaved = false;
            LoginDetail loginDetail = new LoginDetail();

            loginDetail.UserName = !string.IsNullOrEmpty(userRegistrationModel.UserName) ? userRegistrationModel.UserName : null;
            loginDetail.UserPassword = !string.IsNullOrEmpty(userRegistrationModel.Password) ? userRegistrationModel.Password : null;
            loginDetail.UserDataID = userDataID;
            loginDetail.DateCreated = DateTime.UtcNow;
            loginDetail.DateModiﬁed = DateTime.UtcNow;

            if (!ModelState.IsValid || userDataID == 0)
            {
                isLoginDataSaved = false;
            }
            else
            {
                db.LoginDetails.Add(loginDetail);
                db.SaveChanges();
                isLoginDataSaved = true;
            }
            return isLoginDataSaved;
        }
    }
}

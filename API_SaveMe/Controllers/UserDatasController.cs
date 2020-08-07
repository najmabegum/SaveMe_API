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
using System.Web.Http.Description;

namespace API_SaveMe.Controllers
{
    public class UserDatasController : ApiController
    {
        private db_SaveMeTrackerEntities db = new db_SaveMeTrackerEntities();
        
        [Route("GetUserData/{userDataID}")]
        [HttpGet]
        public IHttpActionResult FetchData(int userDataID)
        {
            if(this.UserDataExists(userDataID))
            {
                UserData user = db.UserDatas.Where(e => e.UserDataID == userDataID).FirstOrDefault();
                User userToReturn = new User();
                userToReturn.FirstName = user.FirstName;
                userToReturn.LastName = user.LastName;
                userToReturn.MiddleName = user.MiddleName;
                userToReturn.Age = user.Age;
                userToReturn.Gender = user.Gender;
                userToReturn.MaritalStatus = user.MaritalStatus;
                userToReturn.NumberMembersInFamily = user.NumberMembersInFamily;
                userToReturn.NumberKidsInFamily = user.NumberKidsInFamily;
                return Ok(userToReturn);
            }
            else
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Updates User's personal information
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        [ResponseType(typeof(void))]
        [Route("UpdateUserData/{id}")]
        [HttpPut]
        public IHttpActionResult PutUserData(int id, UserData userData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != userData.UserDataID)
            {
                return BadRequest();
            }

            db.Entry(userData).State = EntityState.Modified;

            try
            {
                userData.DateModiﬁed = DateTime.UtcNow;
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserDataExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(true);
        }


        private bool UserDataExists(int id)
        {
            return db.UserDatas.Count(e => e.UserDataID == id) > 0;
        }
    }
}

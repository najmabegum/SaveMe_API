using API_SaveMe.HelperModels;
using API_SaveMe.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace API_SaveMe.Controllers
{
    public class DailyExpenseController : ApiController
    {
        private db_SaveMeTrackerEntities db = new db_SaveMeTrackerEntities();

        /// <summary>
        /// Records daily expenses of a user
        /// </summary>
        /// <param name="addExpenseModel"></param>
        /// <returns></returns>
        [Route("Expenses/DailyExpense")]
        [HttpPost]
        public IHttpActionResult AddUserDailyExpense(AddExpense addExpenseModel)
       {
            DailyExpense dailyExpense = new DailyExpense();

            // Fetch category id from category name
            int categoryID = db.Categories.Where(e => e.CategoryName.Equals(addExpenseModel.Category)).FirstOrDefault().CategoryID;

            int week = HelperFunctions.HelperFunctions.CurrentWeekNumber;
            int month = DateTime.UtcNow.Month;
            int year = DateTime.UtcNow.Year;

            // Create a new entry for DailyExpense
            dailyExpense.UserDataID = addExpenseModel.UserDataID;
            dailyExpense.CategoryID = categoryID;
            dailyExpense.Amount = addExpenseModel.Amount;
            dailyExpense.CurrentWeek = week;
            dailyExpense.CurrentMonth = month;
            dailyExpense.CurrentYear = year;
            dailyExpense.DateCreated = DateTime.UtcNow;
            dailyExpense.DateModiﬁed = DateTime.UtcNow;

            // Update WeeklySaving - Subtract the daily expense amount from the total amount for that category

            WeeklySaving weeklySaving = db.WeeklySavings.Where(e =>
                                        e.UserDataID == addExpenseModel.UserDataID &&
                                        e.CategoryID == categoryID &&
                                        e.CurrentMonth == month &&
                                        e.CurrentYear == year &&
                                        e.CurrentWeek == week).FirstOrDefault();

            if (weeklySaving != null && weeklySaving.Amount != 0.00m && weeklySaving.Amount >= addExpenseModel.Amount)
            {                
                weeklySaving.Amount -= addExpenseModel.Amount;
            }
            else
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                DailyExpenseReturnModel dailyExpenseReturnModel = new DailyExpenseReturnModel();

                // Saves data in DB
                db.Entry(weeklySaving).State = EntityState.Modified;
                _ = db.DailyExpenses.Add(dailyExpense);
                db.SaveChanges();
                dailyExpenseReturnModel.isDatabaseUpdated = true;


                // Verifies if weekly limit is reached                

                WeeklyExpenseDetail weeklyExpenseDetail = db.WeeklyExpenseDetails.Where(e =>
                                                             e.UserDataID == addExpenseModel.UserDataID &&
                                                             e.CategoryID == categoryID).FirstOrDefault();
                if (this.VerifyThresholdNotReached(weeklyExpenseDetail.Amount, weeklySaving.Amount))
                {
                    dailyExpenseReturnModel.hasThresholdReached = false;
                }
                else
                {
                    dailyExpenseReturnModel.hasThresholdReached = true;
                }

                return Ok(dailyExpenseReturnModel);
            }
            else
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Return the amount that can be spent without crossing threshold
        /// </summary>
        /// <param name="thresholdRequest"></param>
        /// <returns></returns>
        [Route("Expenses/GetAllowedAmount")]
        [HttpPost]
        public IHttpActionResult GetAmountThresholdValue(AddExpense thresholdRequest)
        {
            int week = HelperFunctions.HelperFunctions.CurrentWeekNumber;
            int month = DateTime.UtcNow.Month;
            int year = DateTime.UtcNow.Year;
            
            int categoryID = db.Categories.Where(e => e.CategoryName.Equals(thresholdRequest.Category)).FirstOrDefault().CategoryID;
            
            WeeklyExpenseDetail weeklyExpenseDetail = db.WeeklyExpenseDetails.Where(e =>
                                                             e.UserDataID == thresholdRequest.UserDataID &&
                                                             e.CategoryID == categoryID).FirstOrDefault();
            if (weeklyExpenseDetail != null)
            {
                decimal amountAllowed = this.GetAmountAvailableToSpend(weeklyExpenseDetail.Amount);
                return Ok(amountAllowed);
            }
            else
            {
                return BadRequest();
            }

        }

        /// <summary>
        /// Verifies if 80% of amount consumption has been reached for the week
        /// </summary>
        /// <param name="totalAmount"></param>
        /// <param name="currentAmount"></param>
        /// <returns></returns>
        private bool VerifyThresholdNotReached(decimal totalAmount, decimal currentAmount)
        {
            decimal percent = 20.0m / 100.0m;
            decimal thresholdValue = percent * totalAmount;
            decimal roundedOffThreshold = Math.Round(thresholdValue, 2);
            if (currentAmount > roundedOffThreshold)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Return the amount that can be spent without crossing threshold
        /// </summary>
        /// <param name="totalAmount"></param>
        /// <returns></returns>
        private decimal GetAmountAvailableToSpend(decimal totalAmount)
        {
            decimal percent = 20.0m / 100.0m;
            decimal thresholdValue = percent * totalAmount;            
            decimal amountReturn = totalAmount - thresholdValue;
            decimal roundedOffReturn = Math.Round(amountReturn, 2);
            return roundedOffReturn;
        }
    }
}

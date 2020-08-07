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
    public class WeeklyLimitController : ApiController
    {
        private db_SaveMeTrackerEntities db = new db_SaveMeTrackerEntities();

        /// <summary>
        /// Updates the montly limit set for each category
        /// </summary>
        /// <returns></returns>
        [Route("WeeklyLimit/ResetLimit")]
        [HttpPost]
        public IHttpActionResult UpdateWeeklyLimit(List<WeeklyLimit> weeklyLimits)
        {
            try
            {
                // Initializations
                List<WeeklyExpenseDetail> weeklyExpenseDetails = new List<WeeklyExpenseDetail>();
                int currentWeek = HelperFunctions.HelperFunctions.CurrentWeekNumber;
                int currentMonth = DateTime.UtcNow.Month;
                int currentYear = DateTime.UtcNow.Year;

                decimal zeroAmount = 0.00m;
                decimal totalSavingsTracker = 0.00m;
                int userDataId = 0;

                // Loop to repeat process for each category
                foreach (var item in weeklyLimits)
                {
                    userDataId = item.UserDataID;
                    // Fetch category id from category name
                    int categoryID = db.Categories.Where(e => e.CategoryName.Equals(item.CategoryName)).FirstOrDefault().CategoryID;

                    // Check if WeeklyExpenseDetails already exist
                    if (this.WeeklyExpenseForUserExists(item.UserDataID, categoryID))
                    {
                        WeeklyExpenseDetail weeklyExpenseDetail = db.WeeklyExpenseDetails.Where(e =>
                                                                e.UserDataID == item.UserDataID &&
                                                                e.CategoryID == categoryID).FirstOrDefault();

                        if (weeklyExpenseDetail != null)
                        {
                            // Update WeeklyExpenseDetails with new reset values
                            if (UpdateWeeklyExpenseInstance(currentWeek, currentMonth, currentYear, item, weeklyExpenseDetail))
                            {
                                weeklyExpenseDetails.Add(weeklyExpenseDetail);

                                // Push old WeeklyExpenseDetails to RemoteWeeklyExpenseDetails
                                CreateRemoteWeeklyExpense(currentWeek, currentMonth, currentYear, weeklyExpenseDetail);

                                // Check WeeklySavngs exits
                                WeeklySaving weeklySavingsToUpdate = db.WeeklySavings.Where(e =>
                                                                               e.UserDataID == item.UserDataID &&
                                                                               e.CategoryID == categoryID).FirstOrDefault();
                                if (weeklySavingsToUpdate != null)
                                {
                                    // Check monthlyExpenseDetailsExist and update
                                    if (this.MonthlySavingsExistForUser(item.UserDataID, categoryID, currentMonth, currentYear))
                                    {
                                        MonthlySaving monthlySavingToUpdate = db.MonthlySavings.Where(e =>
                                                                                                      e.UserDataID == item.UserDataID &&
                                                                                                      e.CategoryID == categoryID &&
                                                                                                      e.CurrentMonth == currentMonth &&
                                                                                                      e.CurrentYear == currentYear).FirstOrDefault();

                                        monthlySavingToUpdate.Amount = monthlySavingToUpdate.Amount + weeklySavingsToUpdate.Amount;
                                        monthlySavingToUpdate.DateModiﬁed = DateTime.UtcNow;
                                        db.Entry(monthlySavingToUpdate).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        // Create MonthlySavingsDetails for new month
                                        CreateMonthlySavingsInsatnace(currentMonth, currentYear, zeroAmount, item, categoryID);

                                        //Update previous months weekly savings
                                        int prevMonth = currentMonth - 1;
                                        MonthlySaving previousMonthlySavingToUpdate = db.MonthlySavings.Where(e =>
                                                                                                      e.UserDataID == item.UserDataID &&
                                                                                                      e.CategoryID == categoryID &&
                                                                                                      e.CurrentMonth == prevMonth &&
                                                                                                      e.CurrentYear == currentYear).FirstOrDefault();

                                        previousMonthlySavingToUpdate.Amount = previousMonthlySavingToUpdate.Amount + weeklySavingsToUpdate.Amount;
                                        previousMonthlySavingToUpdate.DateModiﬁed = DateTime.UtcNow;
                                        db.Entry(previousMonthlySavingToUpdate).State = EntityState.Modified;
                                    }

                                    // Track total savings for each category week wise
                                    totalSavingsTracker += weeklySavingsToUpdate.Amount;

                                    // Rest weekly savings to new values
                                    weeklySavingsToUpdate.Amount = item.Amount;
                                    weeklySavingsToUpdate.CurrentWeek = currentWeek;
                                    weeklySavingsToUpdate.CurrentMonth = currentMonth;
                                    weeklySavingsToUpdate.CurrentYear = currentYear;
                                    weeklySavingsToUpdate.DateModiﬁed = DateTime.UtcNow;
                                    db.Entry(weeklySavingsToUpdate).State = EntityState.Modified;
                                }

                            }
                            else
                            {
                                return BadRequest();
                            }


                        }
                        else
                        {
                            return BadRequest();
                        }
                    }
                    else
                    {
                        // Creating for first time set up
                        CreateWeeklyExpenseInstance(currentWeek, currentMonth, currentYear, item, categoryID);
                        CreateWeeklySavingsInstance(currentWeek, currentMonth, currentYear, item, categoryID);
                        CreateMonthlySavingsInsatnace(currentMonth, currentYear, zeroAmount, item, categoryID);
                    }

                }

                // Create or Update TotalSavings to total savings of all categories for the week
                if (this.TotalSavingsExistForUser(userDataId))
                {
                    TotalSaving totalSaving = db.TotalSavings.Where(e => e.UserDataID == userDataId).FirstOrDefault();
                    totalSaving.Amount += totalSavingsTracker;
                    totalSaving.DateModiﬁed = DateTime.UtcNow;
                    db.Entry(totalSaving).State = EntityState.Modified;
                }
                else
                {
                    this.CreateTotalSavingsInstance(userDataId, zeroAmount);
                }

                // Save changes to DB
                if (ModelState.IsValid)
                {
                    if (weeklyExpenseDetails.Any())
                    {
                        weeklyExpenseDetails.ForEach(a =>
                        {
                            db.Entry(a).State = EntityState.Modified;

                        });
                    }

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
            catch
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Updates the weekly limit set for a category mid week
        /// </summary>
        /// <returns></returns>
        [Route("AddAdditionalAmount/ResetLimit")]
        [HttpPost]
        public IHttpActionResult AddAdditionAmount(AdditionalExpenseRequest additionalExpenseRequest)
        {
            if (ModelState.IsValid)
            {
                int currentWeek = HelperFunctions.HelperFunctions.CurrentWeekNumber;
                int currentMonth = DateTime.UtcNow.Month;
                int currentYear = DateTime.UtcNow.Year;
                int userDataid = additionalExpenseRequest.UserDataID;
                int toCategoryID = db.Categories.Where(e => e.CategoryName.Equals(additionalExpenseRequest.ToCategory)).FirstOrDefault().CategoryID;
                if (additionalExpenseRequest.IsFromCategory)
                {                    
                    int fromCategoryID = db.Categories.Where(e => e.CategoryName.Equals(additionalExpenseRequest.FromCategory)).FirstOrDefault().CategoryID;                   
                    
                    #region subtract_amount_from selected category

                    // Create a new entry for DailyExpense
                    DailyExpense dailyExpense = new DailyExpense();
                    dailyExpense.UserDataID = userDataid;
                    dailyExpense.CategoryID = fromCategoryID;
                    dailyExpense.Amount = additionalExpenseRequest.AmountToAdd;
                    dailyExpense.CurrentWeek = currentWeek;
                    dailyExpense.CurrentMonth = currentMonth;
                    dailyExpense.CurrentYear = currentYear;
                    dailyExpense.DateCreated = DateTime.UtcNow;
                    dailyExpense.DateModiﬁed = DateTime.UtcNow;

                    // Update WeeklySaving - Subtract the daily expense amount from the total amount for that category

                    WeeklySaving weeklySavingfromCategory = db.WeeklySavings.Where(e =>
                                                 e.UserDataID == userDataid &&
                                                 e.CategoryID == fromCategoryID &&
                                                 e.CurrentMonth == currentMonth &&
                                                 e.CurrentYear == currentYear &&
                                                 e.CurrentWeek == currentWeek).FirstOrDefault();

                    if (weeklySavingfromCategory != null)
                    {
                        weeklySavingfromCategory.Amount -= additionalExpenseRequest.AmountToAdd;
                        if (this.WeeklyExpenseForUserExists(userDataid, toCategoryID))
                        {
                            WeeklyExpenseDetail weeklyExpenseDetail = db.WeeklyExpenseDetails.Where(e =>
                                                                    e.UserDataID == userDataid &&
                                                                    e.CategoryID == fromCategoryID).FirstOrDefault();

                            if (weeklyExpenseDetail != null)
                            {
                                // Update WeeklyExpenseDetails with new reset values
                                weeklyExpenseDetail.Amount -= additionalExpenseRequest.AmountToAdd;
                                weeklyExpenseDetail.DateModiﬁed = DateTime.UtcNow;
                                db.Entry(weeklyExpenseDetail).State = EntityState.Modified;
                            }
                            else
                            {
                                return BadRequest();
                            }
                        }
                    }
                    else
                    {
                        return BadRequest();
                    }

                    #endregion
                    
                }

                #region add amount to requested category

                if (this.WeeklyExpenseForUserExists(userDataid, toCategoryID))
                {
                    WeeklyExpenseDetail weeklyExpenseDetail = db.WeeklyExpenseDetails.Where(e =>
                                                            e.UserDataID == userDataid &&
                                                            e.CategoryID == toCategoryID).FirstOrDefault();

                    if (weeklyExpenseDetail != null)
                    {
                        // Update WeeklyExpenseDetails with new reset values
                        weeklyExpenseDetail.Amount += additionalExpenseRequest.AmountToAdd;
                        weeklyExpenseDetail.DateModiﬁed = DateTime.UtcNow;
                        db.Entry(weeklyExpenseDetail).State = EntityState.Modified;

                        // Check WeeklySavngs exits
                        WeeklySaving weeklySavingsToUpdate = db.WeeklySavings.Where(e =>
                                                                       e.UserDataID == userDataid &&
                                                                       e.CategoryID == toCategoryID).FirstOrDefault();
                        if (weeklySavingsToUpdate != null)
                        {
                            weeklySavingsToUpdate.Amount += additionalExpenseRequest.AmountToAdd;
                            weeklyExpenseDetail.DateModiﬁed = DateTime.UtcNow;
                            db.Entry(weeklySavingsToUpdate).State = EntityState.Modified;
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
                        else
                        {
                            return BadRequest();
                        }
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    return BadRequest();
                }

                #endregion
            }
            else
            {
                return BadRequest();
            }
        }

        [Route("GetUserWeekStatistics/{userID}")]
        [HttpGet]
        public IHttpActionResult GetWeeklyExpenseAndSaving(int userID)
        {
            int currentWeek = HelperFunctions.HelperFunctions.CurrentWeekNumber;
            int currentMonth = DateTime.UtcNow.Month;
            int currentYear = DateTime.UtcNow.Year;
            List<WeeklyExpenseDetail> weeklyExpenseDetailList = new List<WeeklyExpenseDetail>();
            weeklyExpenseDetailList = db.WeeklyExpenseDetails.Where(e => e.UserDataID == userID
                                                                && e.CurrentWeek == currentWeek
                                                                && e.CurrentMonth == currentMonth
                                                                && e.CurrentYear == currentYear).ToList();
            DailyStatistics dailyStatistics = new DailyStatistics();
            if (weeklyExpenseDetailList.Any())
            {
                decimal totalExpenseSet = weeklyExpenseDetailList.Sum(e => e.Amount);
                List<WeeklySaving> weeklySavingList = new List<WeeklySaving>();
                weeklySavingList = db.WeeklySavings.Where(e => e.UserDataID == userID
                                                                    && e.CurrentWeek == currentWeek
                                                                    && e.CurrentMonth == currentMonth
                                                                    && e.CurrentYear == currentYear).ToList();
                decimal currentWeekSaving = weeklySavingList.Sum(e => e.Amount);
                decimal currentExpenditure = totalExpenseSet - currentWeekSaving; ;                
                dailyStatistics.CurrentWeekTotalExpenditure = Math.Round(totalExpenseSet,2);
                dailyStatistics.CurrentWeekSavings = Math.Round(currentWeekSaving,2);
                dailyStatistics.CurrentWeekExpenses = Math.Round(currentExpenditure,2);
                dailyStatistics.IsWeekPlanSet = true;
                return Ok(dailyStatistics);
            }
            else
            {
                dailyStatistics.IsWeekPlanSet = false;
                dailyStatistics.CurrentWeekTotalExpenditure = 0.00m;
                dailyStatistics.CurrentWeekSavings = 0.00m;
                dailyStatistics.CurrentWeekExpenses = 0.00m;
                return Ok(dailyStatistics);
            }                        
        }

        /// <summary>
        /// Create New instance of MonthlySavings
        /// </summary>
        /// <param name="currentMonth"></param>
        /// <param name="currentYear"></param>
        /// <param name="zeroAmount"></param>
        /// <param name="item"></param>
        /// <param name="categoryID"></param>
        private void CreateMonthlySavingsInsatnace(int currentMonth, int currentYear, decimal zeroAmount, WeeklyLimit item, int categoryID)
        {
            MonthlySaving monthlySaving = new MonthlySaving();
            monthlySaving.UserDataID = item.UserDataID;
            monthlySaving.CategoryID = categoryID;
            monthlySaving.Amount = zeroAmount;
            monthlySaving.CurrentMonth = currentMonth;
            monthlySaving.CurrentYear = currentYear;
            monthlySaving.DateCreated = DateTime.UtcNow;
            monthlySaving.DateModiﬁed = DateTime.UtcNow;
            db.MonthlySavings.Add(monthlySaving);
        }

        /// <summary>
        /// Create new instance of TotalSavings
        /// </summary>
        /// <param name="item"></param>
        private void CreateTotalSavingsInstance(int userDataID, decimal amount)
        {
            TotalSaving totalSaving = new TotalSaving();
            totalSaving.UserDataID = userDataID;
            totalSaving.Amount = amount;
            totalSaving.DateCreated = DateTime.UtcNow;
            totalSaving.DateModiﬁed = DateTime.UtcNow;
            db.TotalSavings.Add(totalSaving);
        }

        /// <summary>
        /// Create new instance of WeeklySavings
        /// </summary>
        /// <param name="currentWeek"></param>
        /// <param name="currentMonth"></param>
        /// <param name="currentYear"></param>
        /// <param name="item"></param>
        /// <param name="categoryID"></param>
        private void CreateWeeklySavingsInstance(int currentWeek, int currentMonth, int currentYear, WeeklyLimit item, int categoryID)
        {
            WeeklySaving weeklySaving = new WeeklySaving();
            weeklySaving.UserDataID = item.UserDataID;
            weeklySaving.CategoryID = categoryID;
            weeklySaving.CurrentWeek = currentWeek;
            weeklySaving.CurrentMonth = currentMonth;
            weeklySaving.CurrentYear = currentYear;
            weeklySaving.Amount = item.Amount;
            weeklySaving.DateCreated = DateTime.UtcNow;
            weeklySaving.DateModiﬁed = DateTime.UtcNow;
            db.WeeklySavings.Add(weeklySaving);
        }

        /// <summary>
        /// Create new instance of WeeklyExpenseDetails 
        /// </summary>
        /// <param name="currentWeek"></param>
        /// <param name="currentMonth"></param>
        /// <param name="currentYear"></param>
        /// <param name="item"></param>
        /// <param name="categoryID"></param>
        private void CreateWeeklyExpenseInstance(int currentWeek, int currentMonth, int currentYear, WeeklyLimit item, int categoryID)
        {
            WeeklyExpenseDetail weeklyExpenseDetail = new WeeklyExpenseDetail();
            weeklyExpenseDetail.UserDataID = item.UserDataID;
            weeklyExpenseDetail.CategoryID = categoryID;
            weeklyExpenseDetail.Amount = item.Amount;
            weeklyExpenseDetail.CurrentWeek = currentWeek;
            weeklyExpenseDetail.CurrentMonth = currentMonth;
            weeklyExpenseDetail.CurrentYear = currentYear;
            weeklyExpenseDetail.DateCreated = DateTime.UtcNow;
            weeklyExpenseDetail.DateModiﬁed = DateTime.UtcNow;
            db.WeeklyExpenseDetails.Add(weeklyExpenseDetail);
        }

        /// <summary>
        /// Update WeeklyExpenseDetails for each category and return list
        /// </summary>
        /// <param name="weeklyExpenseDetails"></param>
        /// <param name="currentWeek"></param>
        /// <param name="currentMonth"></param>
        /// <param name="currentYear"></param>
        /// <param name="item"></param>
        /// <param name="weeklyExpenseDetail"></param>
        private static bool UpdateWeeklyExpenseInstance(int currentWeek, int currentMonth, int currentYear, WeeklyLimit item, WeeklyExpenseDetail weeklyExpenseDetail)
        {
            try
            {
                weeklyExpenseDetail.Amount = item.Amount;
                weeklyExpenseDetail.DateModiﬁed = DateTime.UtcNow;
                weeklyExpenseDetail.CurrentWeek = currentWeek;
                weeklyExpenseDetail.CurrentMonth = currentMonth;
                weeklyExpenseDetail.CurrentYear = currentYear;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Create new nstance of RemoteWeeklyExpenseDetails 
        /// </summary>
        /// <param name="currentWeek"></param>
        /// <param name="currentMonth"></param>
        /// <param name="currentYear"></param>
        /// <param name="weeklyExpenseDetail"></param>
        private void CreateRemoteWeeklyExpense(int currentWeek, int currentMonth, int currentYear, WeeklyExpenseDetail weeklyExpenseDetail)
        {
            RemoteWeeklyExpenseDetail remoteWeeklyExpenseDetail = new RemoteWeeklyExpenseDetail();
            remoteWeeklyExpenseDetail.UserDataID = weeklyExpenseDetail.UserDataID;
            remoteWeeklyExpenseDetail.CategoryID = weeklyExpenseDetail.CategoryID;
            remoteWeeklyExpenseDetail.WeeklyExpenseDetailsID = weeklyExpenseDetail.WeeklyExpenseDetailsID;
            remoteWeeklyExpenseDetail.Amount = weeklyExpenseDetail.Amount;
            remoteWeeklyExpenseDetail.UpdatedWeek = currentWeek;
            remoteWeeklyExpenseDetail.UpdatedMonth = currentMonth;
            remoteWeeklyExpenseDetail.UpdatedYear = currentYear;
            remoteWeeklyExpenseDetail.DateCreated = DateTime.UtcNow;
            remoteWeeklyExpenseDetail.DateModiﬁed = DateTime.UtcNow;
            db.RemoteWeeklyExpenseDetails.Add(remoteWeeklyExpenseDetail);
        }

        /// <summary>
        /// Check if WeeklyExpenseDetails  exists for a user and category
        /// </summary>
        /// <param name="userDataID"></param>
        /// <param name="categoryID"></param>
        /// <returns></returns>
        private bool WeeklyExpenseForUserExists(int userDataID, int categoryID)
        {
            return db.WeeklyExpenseDetails.Count(e => e.UserDataID == userDataID && e.CategoryID == categoryID) > 0;
        }

        /// <summary>
        /// Check if WeeklySavings exists for a user and category
        /// </summary>
        /// <param name="userDataID"></param>
        /// <param name="categoryID"></param>
        /// <returns></returns>
        private bool WeeklySavingsExistForUser(int userDataID, int categoryID)
        {
            return db.WeeklySavings.Count(e => e.UserDataID == userDataID && e.CategoryID == categoryID) > 0;
        }

        /// <summary>
        /// Check if MonthlySavings exists for a user and category
        /// </summary>
        /// <param name="userDataID"></param>
        /// <param name="categoryID"></param>
        /// <param name="month"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        private bool MonthlySavingsExistForUser(int userDataID, int categoryID, int month, int year)
        {
            return db.MonthlySavings.Count(e => e.UserDataID == userDataID &&
                                          e.CategoryID == categoryID &&
                                          e.CurrentMonth == month &&
                                          e.CurrentYear == year) > 0;
        }

        /// <summary>
        /// Check if TotalSavings exists for a user and category
        /// </summary>
        /// <param name="userDataID"></param>
        /// <returns></returns>
        private bool TotalSavingsExistForUser(int userDataID)
        {
            return db.TotalSavings.Count(e => e.UserDataID == userDataID) > 0;
        }
    }
}

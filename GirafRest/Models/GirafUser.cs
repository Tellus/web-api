using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using GirafRest.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace GirafRest.Models
{
    /// <summary>
    /// GirafUser defines all relavant data for the user's of Giraf.
    /// </summary>
    [Table("User")]
    public class GirafUser : IdentityUser<string>
    {       
        /// <summary>
        /// Whether or not the current user is a DepartmentUser
        /// </summary>
        public bool IsDepartment { get; set; }

        /// <summary>
        /// List of guardians in a relationship with the user. Is empty if the user is a guardian.
        /// </summary>
        public virtual ICollection<GuardianRelation> Guardians { get; set; }

        /// <summary>
        /// List of citizens in a relationship with the user. Is empty if the user is a citizen.
        /// </summary>
        public virtual ICollection<GuardianRelation> Citizens { get; set; }

        public string DisplayName { get; set; }

        public virtual byte[] UserIcon { get; set; }

        [ForeignKey("Department")]
        public long? DepartmentKey { get; set; }

        public virtual Department Department { get; set; }

        public virtual ICollection<Week> WeekSchedule { get; set; }

        public virtual ICollection<UserResource> Resources { get; set; }

        public virtual Setting Settings { get; set; }

        private void IntialiseData()
        {
            this.Settings = new Setting();
            this.Resources = new List<UserResource>();
            this.WeekSchedule = new List<Week>();
            this.Citizens = new List<GuardianRelation>();
            this.Guardians = new List<GuardianRelation>();
        }

        public void AddCitizens(List<GirafUser> citizens){
            foreach (var citizen in citizens)
            {
                AddCitizen(citizen);
            }
        }

        public void AddCitizen(GirafUser citizen)
        {
            this.Citizens.Add(new GuardianRelation(this, citizen));
        }

        public void AddGuardians(List<GirafUser> guardians)
        {
            foreach (var guardian in guardians)
            {
                AddGuardian(guardian);
            }
        }

        public void AddGuardian(GirafUser guardian)
        {
            this.Guardians.Add(new GuardianRelation(guardian, this));
        }

        public GirafUser(string userName, Department department) : base(userName)
        {
            IntialiseData();
            Settings.InitialiseWeekDayColors();
            DepartmentKey = department?.Key ?? -1;
        }

        // DO NOT DELETE
        public GirafUser()
        {
            IntialiseData();
        }

       
    }
}
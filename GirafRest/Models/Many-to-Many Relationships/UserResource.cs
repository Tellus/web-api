using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GirafRest.Models
{
    /// <summary>
    /// Defines a many-to-many relationship between users and resources.
    /// </summary>
    public class UserResource
    {
        /// <summary>
        /// The key of the relationship entity.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Key {get; set;}

        /// <summary>
        /// The key of the user who is involved in the relationship.
        /// </summary>
        [Required]
        public string OtherKey { get; set; }
        /// <summary>
        /// A reference to the actual user.
        /// </summary>
        [ForeignKey("OtherKey")]
        public virtual GirafUser Other {get; set;}

        /// <summary>
        /// The key of the involved resource.
        /// </summary>
        [Required]
        public long PictogramKey { get; set; }
        //A reference to the actual resource.
        [ForeignKey("ResourceKey")]
        public virtual Pictogram Pictogram { get; set; }

        /// <summary>
        /// Creates a new many-to-many relationship between a user and a resource.
        /// </summary>
        /// <param name="user">The involved user.</param>
        /// <param name="resource">The involved resource.</param>
        public UserResource(GirafUser user, Pictogram pictogram)
        {
            this.OtherKey = user.Id;
            this.Other = user;
            this.PictogramKey = pictogram.Id;
            this.Pictogram = pictogram;

            pictogram.LastEdit = DateTime.Now;
            Other.Resources.Add(this);
            pictogram.Users.Add(this);
        }

        /// <summary>
        /// DO NOT DELETE THIS.
        /// </summary>
        public UserResource(){ }
    }
}
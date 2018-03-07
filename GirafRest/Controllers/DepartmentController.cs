using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GirafRest.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using GirafRest.Models.DTOs;
using GirafRest.Models.Responses;
using GirafRest.Services;
using Org.BouncyCastle.Math.EC;

namespace GirafRest.Controllers
{
    /// <summary>
    /// The department controller serves the purpose of handling departments. It is capable of producing a
    /// list of all departments in the system as well as adding resources and users to departments.
    /// </summary>
    [Route("v1/[controller]")]
    public class DepartmentController : Controller
    {
        /// <summary>
        /// A reference to IGirafService, which defines common functionality for all controllers.
        /// </summary>
        private readonly IGirafService _giraf;

        /// <summary>
        /// Constructor for the department-controller. This is called by the asp.net runtime.
        /// </summary>
        /// <param name="giraf">A reference to the GirafService.</param>
        /// <param name="loggerFactory">A reference to an implementation of ILoggerFactory. Used to create a logger.</param>
        public DepartmentController(IGirafService giraf, ILoggerFactory loggerFactory)
        {
            _giraf = giraf;
            _giraf._logger = loggerFactory.CreateLogger("Department");
        }

        /// <summary>
        /// Get all departments registered in the database or search for a department name as a query string.
        /// </summary>
        /// <returns>A list of all departments.</returns>
        [HttpGet("")]
        public async Task<Response<List<DepartmentDTO>>> Get()
        {
            try {
                //Filter all departments away that does not satisfy the name query.
                var nameQuery = HttpContext.Request.Query["name"];
                var depart = NameQueryFilter(nameQuery);
                //Include relevant information and cast to list.
                var result = await depart
                    .Include(dep => dep.Members)
                    .Include(dep => dep.Resources)
                    .ThenInclude(dr => dr.Resource)
                    .ToListAsync();

                if (result.Count == 0)
                    return new ErrorResponse<List<DepartmentDTO>>(ErrorCode.NotFound);

                //Return the list.
                return new Response<List<DepartmentDTO>>(result.Select(d => new DepartmentDTO(d)).ToList());
            } catch (Exception e)
            {
                string errorMessage = $"Exception in Get: {e.Message}, {e.InnerException}";
                _giraf._logger.LogError(errorMessage);
                return new ErrorResponse<List<DepartmentDTO>>(ErrorCode.Error, errorMessage);
            }
        }

        /// <summary>
        /// Get the department with the specified id.
        /// </summary>
        /// <param name="id">The id of the department to search for.</param>
        /// <returns>The department with the given id or NotFound.</returns>
        [HttpGet("{id}")]
        public async Task<Response<DepartmentDTO>> Get(long id)
        {
            //.Include is used to get information on members aswell when getting the Department
            var department = _giraf._context.Departments
                .Where(dep => dep.Key == id);

            var depa = await department
                .Include(dep => dep.Members)
                .Include(dep => dep.Resources)
                .FirstOrDefaultAsync();

            if(depa == null)
                return new ErrorResponse<DepartmentDTO>(ErrorCode.NotFound);

            return new Response<DepartmentDTO>(new DepartmentDTO(depa)); 
        }

        /// <summary>
        /// Add a department to the database.
        /// </summary>
        /// <param name="dep">The department to add to the database.</param>
        /// <returns>The new departmentDTO with all database-generated information.</returns>
        [HttpPost("")]
        [Authorize]
        public async Task<Response<DepartmentDTO>> Post([FromBody]DepartmentDTO dep)
        {
            try
            {
                if (dep == null || dep.Name == null)
                    return new ErrorResponse<DepartmentDTO>(ErrorCode.MissingProperties,
                        "Deparment name has to be specified!");
                //Add the department to the database.
                Department result = new Department(dep);
                await _giraf._context.Departments.AddAsync(result);

                //Add all members specified by either id or username in the DTO
                foreach(var mem in dep.Members) {
                    var usr = await _giraf._context.Users
                        .Where(u => u.UserName == mem || u.Id == mem)
                        .FirstOrDefaultAsync();

                    if (usr == null)
                        return new ErrorResponse<DepartmentDTO>(ErrorCode.InvalidProperties, 
                            "The member list contained an invalid id: " + mem);

                    result.Members.Add(usr);
                    usr.Department = result;
                }
                
                //Add all the resources with the given ids
                foreach (var reso in dep.Resources) {
                    var res = await _giraf._context.Pictograms
                        .Where(p => p.Id == reso)
                        .FirstOrDefaultAsync();

                    if(res == null) 
                        return new ErrorResponse<DepartmentDTO>(ErrorCode.InvalidProperties, 
                            "The list of resources contained an invalid resource id: " + reso);
                    var dr = new DepartmentResource(result, res);
                    await _giraf._context.DepartmentResources.AddAsync(dr);
                }
                //Save the changes and return the entity
                await _giraf._context.SaveChangesAsync();
                return new Response<DepartmentDTO>(new DepartmentDTO(result));
            }
            catch (System.Exception e)
            {
                string errorDescription = $"Exception in Post: {e.Message}, {e.InnerException}";
                _giraf._logger.LogError(errorDescription);
                return new ErrorResponse<DepartmentDTO>(ErrorCode.Error, errorDescription);
            }
        }

        /// <summary>
        /// Add a user to the given department.
        /// </summary>
        /// <param name="ID">The Id of the department to add the user to.</param>
        /// <param name="usr">An existing GirafUser instance to be added to the department.</param>
        /// <returns>MissingProperties if the DTO is empty.
        /// DepartmentNotFound if department of specified ID isn't found.
        /// UserNameAlreadyTakenWithinDepartment if a user with usr's username already exists in the specified department.
        /// UserNotFound if no user exists with the ID of usr. 
        /// A DepartmentDTO representing the new state of the department, if there were no problems.</returns>
        [HttpPost("user/{departmentID}")]
        public async Task<Response<DepartmentDTO>> AddUser(long departmentID, [FromBody]GirafUserDTO usr)
        {
            //Fetch user and department and check that they exist
            if(usr?.Username == null)
                return new ErrorResponse<DepartmentDTO>(ErrorCode.MissingProperties);
            
            Department dep = await _giraf._context.Departments
                .Where(d => d.Key == departmentID)
                .Include(d => d.Members)
                .FirstOrDefaultAsync();
            
            if(dep == null)
                return new ErrorResponse<DepartmentDTO>(ErrorCode.DepartmentNotFound);

            //Check if the user is already in the department
            if(dep.Members.Any(u => u.UserName == usr.Username))
                return new ErrorResponse<DepartmentDTO>(ErrorCode.UserNameAlreadyTakenWithinDepartment);

            //Add the user and save these changes
            var user = await _giraf._context.Users.Where(u => u.Id == usr.Id).FirstOrDefaultAsync();

            if (user == null)
                return new ErrorResponse<DepartmentDTO>(ErrorCode.UserNotFound);
            
            user.DepartmentKey = dep.Key;
            dep.Members.Add(user);
            await _giraf._context.SaveChangesAsync();
            return new Response<DepartmentDTO>(new DepartmentDTO(dep));
        }

        /// <summary>
        /// Removes a user from a given department.
        /// </summary>
        /// <param name="departmentID">Id of the department from which the user should be removed</param>
        /// <param name="usr">A serialized instance of a <see cref="GirafUser"/> user.</param>
        /// <returns>
        /// MissingProperties if no user is given.
        /// UserNotFound if user does not appear within given department.
        /// DepartmentNotFound if there is no department with the given Id.
        /// DepartmentDTO in its updated state if no problems occured.
        /// </returns>
        [HttpDelete("user/{departmentID}")]
        public async Task<Response<DepartmentDTO>> RemoveUser(long departmentID, [FromBody]GirafUser usr)
        {
            //Check if a valid user was supplied and that the given department exists
            if(usr == null)
                return new ErrorResponse<DepartmentDTO>(ErrorCode.MissingProperties);

            var dep = await _giraf._context
                .Departments
                .Where(d => d.Key == departmentID)
                .Include(d => d.Members)
                .FirstOrDefaultAsync();
            
            if(dep == null)
                return new ErrorResponse<DepartmentDTO>(ErrorCode.DepartmentNotFound);

            //Check if the user actually is in the department
            if(!dep.Members.Any(u => u.UserName == usr.UserName))
                return new ErrorResponse<DepartmentDTO>(ErrorCode.UserNotFound, 
                    "User does not exist in the given department.");

            //Remove the user from the department
            dep.Members.Remove(dep.Members.First(u => u.UserName == usr.UserName));
            _giraf._context.SaveChanges();
            return new Response<DepartmentDTO>(new DepartmentDTO(dep));
        }

        /// <summary>
        /// Add a resource to the given department. After this call, the department owns the resource and it is available to all its members.
        /// </summary>
        /// <param name="departmentID">Id of the department to add the resource to.</param>
        /// <param name="resourceId">ResourceIdDTO containing relevant information about the resource.</param>
        /// <returns>
        /// DepartmentNotFound If department wasn't found.
        /// ResourceIDUnreadable If resourceDTO's id was null or not a number.
        /// ResourceNotFound If no resource exists with the given ID.
        /// NotAuthorized If user does not have ownership of resource.
        /// DepartmentAlreadyOwnsResource If requested resource is already owned by requested department.
        /// The DepartmentDTO represented the updated state of the department if there were no errors.
        /// </returns>
        [HttpPost("resource/{departmentID}")]
        [Authorize]
        public async Task<Response<DepartmentDTO>> AddResource(long departmentID, [FromBody] ResourceIdDTO resourceDTO) {
            if (resourceDTO?.Id == null)
                return new ErrorResponse<DepartmentDTO>(ErrorCode.MissingProperties, "Missing resource ID.");
            
            //Fetch the department and check that it exists.
            var department = await _giraf._context.Departments.Where(d => d.Key == departmentID).FirstOrDefaultAsync();
            var usr = await _giraf.LoadUserAsync(HttpContext.User);
            
            if(department == null) 
                return new ErrorResponse<DepartmentDTO>(ErrorCode.DepartmentNotFound);

            //Check if there is a resourceId specified in the body or as a query-paramater
            long resId = -1;
            var resourceIdValid = CheckResourceId(resourceDTO.Id, ref resId);
            
            if(!resourceIdValid) 
                return new ErrorResponse<DepartmentDTO>(ErrorCode.ResourceIDUnreadable);

            //Fetch the resource with the given id, check that it exists and that the user owns it.
            var resource = await _giraf._context.Pictograms.Where(f => f.Id == resId).FirstOrDefaultAsync();
            
            if(resource == null) 
                return new ErrorResponse<DepartmentDTO>(ErrorCode.ResourceNotFound);
            
            var resourceOwned = await _giraf.CheckPrivateOwnership(resource, usr);
            
            if(!resourceOwned) 
                return new ErrorResponse<DepartmentDTO>(ErrorCode.NotAuthorized);

            //Check if the department already owns the resource
            var alreadyOwned = await _giraf._context.DepartmentResources
                .Where(depres => depres.OtherKey == departmentID && depres.ResourceKey == resId)
                .AnyAsync();
            
            if(alreadyOwned) 
                return new ErrorResponse<DepartmentDTO>(ErrorCode.DepartmentAlreadyOwnsResource);

            //Remove resource from user
            var usrResource = await _giraf._context.UserResources
                .Where(ur => ur.ResourceKey == resource.Id && ur.OtherKey == usr.Id)
                .FirstOrDefaultAsync();
            usr.Resources.Remove(usrResource);
            await _giraf._context.SaveChangesAsync();

            //Change resource AccessLevel to Protected from Private
            resource.AccessLevel = AccessLevel.PROTECTED;

            //Create a relationship between the department and the resource.
            var dr = new DepartmentResource(department, resource);
            await _giraf._context.DepartmentResources.AddAsync(dr);
            await _giraf._context.SaveChangesAsync();
            
            //Return Ok and the department - the resource is now visible in deparment.Resources
            return new Response<DepartmentDTO>(new DepartmentDTO(department));
        }

        /// <summary>
        /// Removes a resource from the users department.
        /// </summary>
        /// <param name="resourceId">ResourceIdDTO containing relevant information about the resource.</param>
        /// <returns>
        /// DepartmentDTO of updated state if no problems occured.
        /// </returns>
        [HttpDelete("resource/")]
        [Authorize]
        public async Task<Response<DepartmentDTO>> RemoveResource([FromBody]ResourceIdDTO resourceDTO) {
            if (resourceDTO == null)
                return new ErrorResponse<DepartmentDTO>(ErrorCode.MissingProperties);
            
            //Fetch the department and check that it exists.
            var usr = await _giraf.LoadUserAsync(HttpContext.User);

            long resId = -1;
            var resourceIdValid = CheckResourceId(resourceDTO.Id, ref resId);
            
            if(!resourceIdValid) 
                return new ErrorResponse<DepartmentDTO>(ErrorCode.ResourceIDUnreadable);
            
            //Fetch the resource with the given id, check that it exists.
            var resource = await _giraf._context.Pictograms
                .Where(f => f.Id == resId)
                .FirstOrDefaultAsync();
            
            if(resource == null) 
                return new ErrorResponse<DepartmentDTO>(ErrorCode.ResourceNotFound);
            
            var resourceOwned = await _giraf.CheckProtectedOwnership(resource, usr);
            
            if(!resourceOwned) 
                return new ErrorResponse<DepartmentDTO>(ErrorCode.NotAuthorized);

            //Check if the department already owns the resource and remove if so.
            var drrelation = await _giraf._context.DepartmentResources
                .Where(dr => dr.ResourceKey == resource.Id && dr.OtherKey == usr.Department.Key)
                .FirstOrDefaultAsync();
            
            if(drrelation == null) 
                return new ErrorResponse<DepartmentDTO>(ErrorCode.ResourceNotOwnedByDepartment);
            
            usr.Department.Resources.Remove(drrelation);
            await _giraf._context.SaveChangesAsync();

            //Return Ok and the department - the resource is now visible in deparment.Resources
            return new Response<DepartmentDTO>(new DepartmentDTO(usr.Department));
        }

        #region Helpers
        /// <summary>
        /// Produces a list of all departments that has 'nameQuery' in their name.
        /// </summary>
        /// <param name="nameQuery">A string to search for in the name of the departments.</param>
        /// <returns>A list of all departments with 'nameQuery' in their name.</returns>
        private IQueryable<Department> NameQueryFilter(string nameQuery)
        {
            if(string.IsNullOrEmpty(nameQuery)) nameQuery = "";
            return _giraf._context.Departments.Where(d => d.Name.ToLower().Contains(nameQuery.ToLower()));
        }

        /// <summary>
        /// Checks if a valid resource-id has been specified along with the request.
        /// </summary>
        /// <param name="resourceId">The resource-id specified in the request's body.</param>
        /// <param name="resId">A ref parameter for storing the found resource-id.</param>
        /// <returns>True if a valid resource-id was found, false otherwise.</returns>
        private bool CheckResourceId(long? resourceId, ref long resId) 
        {
            if(resourceId == null) 
            {
                var resourceQuery = HttpContext.Request.Query["resourceId"];
                if(string.IsNullOrEmpty(resourceQuery)) return false;
                if(!long.TryParse(resourceQuery, out resId)) return false;
            }
            else resId = (long) resourceId;
            return true;
        }
        #endregion
    }
}
﻿using System.IO;
using System.Linq;
using GirafRest.Models.Responses;
using GirafRest.Services;
using Microsoft.AspNetCore.Mvc;

namespace GirafRest.Controllers
{
    [Route("v1/[controller]")]
    public class StatusController : Controller
    {
        private readonly IGirafService _giraf;
        public StatusController(IGirafService giraf)
        {
            _giraf = giraf;
        }

        /// <summary>
        /// End-point for checking if the API is running
        /// </summary>
        /// <returns>Success Reponse.</returns>
        [HttpGet("")]
        public Response Status()
        {
            return new Response();
        }

        /// <summary>
        /// End-point for checking connection to the database
        /// </summary>
        /// <returns>Success response if connection to database else ErrorResponse</returns>
        [HttpGet("database")]
        public Response DatabaseStatus()
        {
            try
            {
                _giraf._context.Users.FirstOrDefault();
                return new Response();
            }
            catch (System.Exception ex)
            {
                return new ErrorResponse(ErrorCode.Error);
            }
        }

        /// <summary>
        /// Endpoint for getting git version info i.e. branch and commithash 
        /// </summary>
        /// <returns>branch and commit hash for this API instance</returns>
        [HttpGet("version-info")]
        public Response<string> GetVersionInfo()
        {
            var gitpath = Directory.GetParent(Directory.GetCurrentDirectory()).FullName + "/.git/";
            var pathToHead = System.IO.File.ReadLines(gitpath + "HEAD").First().Split(" ").Last();

            var hash = System.IO.File.ReadLines(gitpath +  pathToHead).First();
            // this assumes that branches are not named with / however this should be enforced anyways
            var branch = pathToHead.Split("/").Last();
            return new Response<string>($"Branch: {branch} CommitHash: {hash}");
        }


    }
}

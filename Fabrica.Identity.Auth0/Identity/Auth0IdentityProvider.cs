﻿using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Fabrica.Exceptions;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Text;
using JetBrains.Annotations;

namespace Fabrica.Identity
{


    public class Auth0IdentityProvider: CorrelatedObject, IIdentityProvider
    {


        public Auth0IdentityProvider( ICorrelation correlation, IAccessTokenSource source, string domain ): base(correlation)
        {
            Source = source;
            Domain = domain;
        }


        private IAccessTokenSource Source { get; }
        private string Domain { get; }

        private RandomNumberGenerator Rng { get; } = new RNGCryptoServiceProvider();

        private ManagementApiClient Client { get; set; }

        public async Task<SyncUserResponse> SyncUser( SyncUserRequest request )
        {

            if (request == null) throw new ArgumentNullException(nameof(request));


            using var logger = EnterMethod();

            logger.LogObject(nameof(request), request);



            // *****************************************************************
            logger.Debug("Attempting to fetch token and build Auth0 Mgmt client");
            var token = await Source.GetToken();
            Client = new ManagementApiClient( token, Domain );



            // *****************************************************************
            logger.Debug("Attempting to find user");
            var user = await _findUser( request );



            // *****************************************************************
            SyncUserResponse result;
            if ( user == null )
                result = await _createUser( request );
            else
                result = await _updateUser( request );


            logger.LogObject(nameof(user), user);



            // *****************************************************************
            logger.LogObject(nameof(result), result);
            return result;


        }


        private async Task<User> _findUser( [NotNull] SyncUserRequest request )
        {
            
            if (request == null) throw new ArgumentNullException(nameof(request));


            using var logger = EnterMethod();



            // *****************************************************************            
            User user;
            try
            {

                logger.Debug("Attempting to find user");
                if( !string.IsNullOrWhiteSpace(request.IdentityUid) )
                    user = await Client.Users.GetAsync(request.IdentityUid);
                else if( string.IsNullOrWhiteSpace(request.IdentityUid) && !string.IsNullOrWhiteSpace(request.CurrentEmail) )
                {
                    var list = await Client.Users.GetUsersByEmailAsync(request.CurrentEmail);
                    user =  list.FirstOrDefault();
                }
                else
                    user = null;

                logger.LogObject(nameof(user), user);

            }
            catch( Exception cause )
            {
                logger.ErrorWithContext(cause, request, "Failed while trying to find Auth0 User");
                throw;
            }



            // *****************************************************************            
            return user;


        }

        private async Task<SyncUserResponse> _createUser( [NotNull] SyncUserRequest request )
        {

            if (request == null) throw new ArgumentNullException(nameof(request));

            using var logger = EnterMethod();


            logger.Debug("Attempting to create new User");
            try
            {

                PredicateException exp = null;

                if (string.IsNullOrWhiteSpace(request.NewEmail))
                {
                    exp ??= new PredicateException("Invalid Create User request");
                    exp.WithDetail(new EventDetail {Category = EventDetail.EventCategory.Violation, Explanation = "Email is required"} );
                }

                if (string.IsNullOrWhiteSpace(request.NewFirstName))
                {
                    exp ??= new PredicateException("Invalid Create User request");
                    exp.WithDetail(new EventDetail { Category = EventDetail.EventCategory.Violation, Explanation = "First Name is required" });
                }

                if (string.IsNullOrWhiteSpace(request.NewLastName))
                {
                    exp ??= new PredicateException("Invalid Create User request");
                    exp.WithDetail(new EventDetail { Category = EventDetail.EventCategory.Violation, Explanation = "Last Name is required" });
                }

                if (exp != null)
                    throw exp;



                // *****************************************************************
                logger.Debug("Attempting to create password");
                var buff = new byte[16];
                Rng.GetNonZeroBytes(buff);
                var password = Base62Converter.Encode(buff);



                // *****************************************************************
                logger.Debug("Attempting to Auth0 request");
                var ucr = new UserCreateRequest
                {
                    Connection    = "Username-Password-Authentication",
                    Email         = request.NewEmail,
                    EmailVerified = true,
                    Password      = password,
                    FirstName     = request.NewFirstName,
                    LastName      = request.NewLastName,
                    FullName      = $"{request.NewFirstName} {request.NewLastName}",
                };


                logger.Debug("Attempting to call Create");
                var user = await Client.Users.CreateAsync(ucr);


                // *****************************************************************
                logger.Debug("Attempting to build result");
                var result = new SyncUserResponse
                {
                    Created     = true,
                    IdentityUid = user.UserId,
                    Password    = password,

                };



                // *****************************************************************
                return result;



            }
            catch (Exception cause)
            {
                logger.ErrorWithContext(cause, request, "Failed to create new Auth0 User");
                throw;
            }



        }

        private async Task<SyncUserResponse> _updateUser( [NotNull] SyncUserRequest request )
        {

            if (request == null) throw new ArgumentNullException(nameof(request));

            using var logger = EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to update existing Auth0 User");
            try
            {


                logger.Debug("Attempting to Auth0 request");
                var uur = new UserUpdateRequest();

                if (!string.IsNullOrWhiteSpace(request.NewEmail))
                {
                    uur.Email = request.NewEmail;
                    uur.EmailVerified = true;
                }

                if( !string.IsNullOrWhiteSpace(request.NewFirstName) )
                    uur.FirstName = request.NewFirstName;

                if (!string.IsNullOrWhiteSpace(request.NewLastName))
                    uur.LastName = request.NewLastName;


                logger.Debug("Attempting to call Update");
                var user = await Client.Users.UpdateAsync( request.IdentityUid, uur );



                logger.Debug("Attempting to build result");
                var result = new SyncUserResponse
                {
                    Created     = false,
                    IdentityUid = user.UserId
                };


                // *****************************************************************
                return result;


            }
            catch (Exception cause)
            {
                logger.ErrorWithContext(cause, request, "Failed to update existing Auth0 User");
                throw;
            }


        }


    }

}

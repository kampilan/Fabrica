using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
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

        public async Task<SyncUserResponse> SyncUser( [NotNull] string identityUid, [NotNull] string email, [NotNull] string firstName, [NotNull] string lastName )
        {


            if (firstName == null) throw new ArgumentNullException(nameof(firstName));
            if (lastName == null) throw new ArgumentNullException(nameof(lastName));
            if (email == null) throw new ArgumentNullException(nameof(email));


            using var logger = EnterMethod();

            logger.Inspect( nameof(identityUid), identityUid );
            logger.Inspect(nameof(email), email);
            logger.Inspect( nameof(firstName), firstName );
            logger.Inspect( nameof(lastName), lastName );



            // *****************************************************************
            logger.Debug("Attempting to fetch token and build Auth0 Mgmt client");
            var token = await Source.GetToken();
            Client = new ManagementApiClient( token, Domain );



            // *****************************************************************
            logger.Debug("Attempting to find user");
            var user = await _findUser( identityUid, email );



            // *****************************************************************
            SyncUserResponse result;
            if ( user == null )
                result = await _createUser(email, firstName, lastName);
            else
                result = await _updateUser(identityUid, email, firstName, lastName);


            logger.LogObject(nameof(user), user);



            // *****************************************************************
            logger.LogObject(nameof(result), result);
            return result;


        }


        private async Task<User> _findUser( [CanBeNull] string identityUid, [NotNull] string email )
        {

            
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(email));

            using var logger = EnterMethod();



            // *****************************************************************            
            User user;
            try
            {

                logger.Debug("Attempting to find user");
                if (string.IsNullOrWhiteSpace(identityUid))
                {
                    var list = await Client.Users.GetUsersByEmailAsync(email);
                    user =  list.FirstOrDefault();
                }
                else
                    user = await Client.Users.GetAsync(identityUid);

                logger.LogObject(nameof(user), user);

            }
            catch (Exception cause)
            {
                var ctx = new { identityUid, email };
                logger.ErrorWithContext(cause, ctx, "Failed while trying to find Auth0 User");
                throw;
            }



            // *****************************************************************            
            return user;


        }

        private async Task<SyncUserResponse> _createUser(string email, string firstName, string lastName)
        {

            using var logger = EnterMethod();


            logger.Debug("Attempting to create new User");
            try
            {


                // *****************************************************************
                logger.Debug("Attempting to create password");
                var buff = new byte[16];
                Rng.GetNonZeroBytes(buff);
                var password = Base62Converter.Encode(buff);



                // *****************************************************************
                logger.Debug("Attempting to Auth0 request");
                var request = new UserCreateRequest
                {
                    Connection    = "Username-Password-Authentication",
                    Email         = email,
                    EmailVerified = true,
                    Password      = password,
                    FirstName     = firstName,
                    LastName      = lastName,
                    FullName      = $"{firstName} {lastName}",
                };


                logger.Debug("Attempting to call Create");
                var user = await Client.Users.CreateAsync(request);


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
                var ctx = new { email, firstName, lastName };
                logger.ErrorWithContext(cause, ctx, "Failed to create new Auth0 User");
                throw;
            }



        }

        private async Task<SyncUserResponse> _updateUser( string identityUid, string email, string firstName, string lastName )
        {

            using var logger = EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to update existing Auth0 User");
            try
            {


                logger.Debug("Attempting to Auth0 request");
                var request = new UserUpdateRequest
                {
                    FirstName     = firstName,
                    LastName      = lastName,
                    FullName      = $"{firstName} {lastName}",
                    Email         = email,
                    EmailVerified = true,
                };


                logger.Debug("Attempting to call Update");
                var user = await Client.Users.UpdateAsync(identityUid, request);



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
                var ctx = new { identityUid, firstName, lastName, email };
                logger.ErrorWithContext(cause, ctx, "Failed to update existing Auth0 User");
                throw;
            }


        }


    }

}

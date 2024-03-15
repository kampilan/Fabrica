using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fabrica.Http;
using Fabrica.Identity.Keycloak.Models;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Text;
using Fabrica.Watch;
using Humanizer;
using MediatR;

// ReSharper disable AccessToDisposedClosure

namespace Fabrica.Identity.Keycloak;

public class KeycloakIdentityProvider : CorrelatedObject, IIdentityProvider
{

    public const string HttpClientName = "Keycloak";

    private static JsonSerializerOptions DefaultOptions { get; } = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };


    public KeycloakIdentityProvider( ICorrelation correlation, IHttpClientFactory factory ) : base(correlation)
    {

        _factory = factory;

    }

    private readonly IHttpClientFactory _factory;

    private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();


    public async Task<bool> ExecutePasswordReset( string email, CancellationToken ct = new() )
    {

        using var logger = EnterMethod();


        logger.Debug("Attempting to fetch User by Email");

        var req = HttpRequestBuilder.Get(HttpClientName)
            .ForResource<User>()
            .AddParameter("email", email)
            .AddParameter("exact", true )
            .ToRequest();

        logger.Inspect("endpoint", req.Path);


        var (ok, results) = await _factory.Many<User>(req, ct);

        logger.Inspect(nameof(ok), ok);



        // *****************************************************************
        if( ok && results.FirstOrDefault() is { } user && !string.IsNullOrWhiteSpace(user.Id) )
        {


            // *****************************************************************
            logger.Debug("User found");

            var body = new List<string> {"UPDATE_PASSWORD"};

            var actReq = HttpRequestBuilder.Put(HttpClientName)
                .ForResource<User>()
                .WithIdentifier(user.Id)
                .WithSubResource("execute-actions-email")
                .WithBody(body)
                .ToRequest();
            


            // *****************************************************************
            logger.Debug("Attempting to send Reset Password Email request");
            var actRes = await _factory.Send(actReq, ct);

            logger.LogObject(nameof(actRes), actRes);



            // *****************************************************************
            return actRes.WasSuccessful;

        }



        // *****************************************************************
        if ( ok )
            logger.Debug("User not found");


        // *****************************************************************
        return false;

    }


    public async Task<SyncUserResponse> SyncUser( SyncUserRequest request, CancellationToken ct=new() )
    {

        if (request == null) throw new ArgumentNullException(nameof(request));


        using var logger = EnterMethod();

        logger.LogObject(nameof(request), request);

        
        var response = new SyncUserResponse();



        // *****************************************************************
        logger.Debug("Attempting to dig out User given request");

        User? user = null;
        if ( !string.IsNullOrWhiteSpace(request.IdentityUid) )
        {

            logger.Debug("Attempting to fetch User by given IdentityUid");

            var req = HttpRequestBuilder.Get(HttpClientName)
                .ForResource<User>()
                .WithIdentifier(request.IdentityUid)
                .ToRequest();

            var (ok, model) = await _factory.One<User>( req, ct );

            if (ok)
                user = model;
            else
            {
                response.Created = false;
                response.Exists = false;
                return response;
            }

        }
        else if (!string.IsNullOrWhiteSpace(request.CurrentUsername))
        {

            logger.Debug("Attempting to fetch User by CurrentUsername");

            var req = HttpRequestBuilder.Get(HttpClientName)
                .ForResource<User>()
                .AddParameter("username", request.CurrentUsername )
                .AddParameter("exact", "true")
                .ToRequest();

            var (ok, results) = await _factory.Many<User>(req, ct);

            if ( ok )
                user = results.FirstOrDefault();

        }
        else if( !string.IsNullOrWhiteSpace(request.NewUsername) )
        {

            logger.Debug("Attempting to fetch User by given NewUsername");

            var req = HttpRequestBuilder.Get(HttpClientName)
                .ForResource<User>()
                .AddParameter("username", request.NewUsername)
                .AddParameter("exact", "true")
                .ToRequest();

            var (ok, results) = await _factory.Many<User>(req, ct);

            if( ok && results.Any() )
            {
                response.Created = false;
                response.Exists = true;
                return response;
            }

        }



        // *****************************************************************
        logger.Debug("Checking null User and not Upsert");
        if ( user is null && !request.Upsert )
            return response;

        // *****************************************************************
        logger.Debug("Checking to null User and Upsert");
        if( user is null && request.Upsert && !string.IsNullOrWhiteSpace(request.NewUsername) )
            await Create();
        else if( user is not null)
            await Update();


        return response;


        async Task Create()
        {


            // *****************************************************************
            logger.Debug("Attempting to create new user");

            var actions = new List<string>();

            if (request.MustVerifyEmail)
                actions.Add("VERIFY_EMAIL");

            if (request.MustUpdateProfile)
                actions.Add("UPDATE_PROFILE");

            if (request.MustUpdatePassword)
                actions.Add("UPDATE_PASSWORD");

            if (request.MustConfigureMfa)
                actions.Add("CONFIGURE_TOTP");


            logger.LogObject(nameof(actions), actions);


            // *****************************************************************
            logger.Debug("Attempting to populate User");
            user = new User
            {
                UserName      = request.NewUsername,
                Email         = request.NewEmail,
                FirstName     = request.NewFirstName,
                LastName      = request.NewLastName,
                EmailVerified = !request.MustVerifyEmail,
                Enabled       = !request.NewEnabled.HasValue || request.NewEnabled.Value
            };

            if( actions.Count > 0 )
                user.RequiredActions = actions;

            if( request.Attributes.Count > 0 )
                user.Attributes = request.Attributes;

            if( request.Groups.Count > 0 )
                user.Groups = request.Groups;


            var password = "";
            if( !string.IsNullOrWhiteSpace(request.NewPassword) )
            {

                logger.Debug("Attempting to save custom password");

                password = request.NewPassword;
                user.Credentials = new List<Credentials> { new() { Type = "password", UserLabel = "Custom", Value = password, Temporary = false } };

            }
            else if ( request.GeneratePassword )
            {
                logger.Debug("Attempting to generate password");

                var buf = new byte[16];
                _rng.GetNonZeroBytes(buf);
                password = Base62Converter.Encode(buf);

                user.Credentials = new List<Credentials> { new(){ Type  = "password", UserLabel = "Generated", Value = password, Temporary = request.PasswordIsTemporary }};

            }
            else if( !string.IsNullOrWhiteSpace(request.HashedPassword) )
            {
                logger.Debug("Attempting to import password");

                var cred = new { algorithm = request.HashAlgorithm, hashIterations = request.HashIterations, additionalParameters = new { } };
                var credJson = JsonSerializer.Serialize(cred);

                var sec = new { value = request.HashedPassword, salt="", additionalParameters= new {} };
                var secJson = JsonSerializer.Serialize(sec);

                user.Credentials = new List<Credentials> { new() { Type = "password", UserLabel = "Imported", CredentialData = credJson, SecretData = secJson} };

            }


            logger.LogObject(nameof(user), user);



            // *****************************************************************
            logger.Debug("Attempting to Create User");
            var json = JsonSerializer.Serialize(user, DefaultOptions);
            var req = HttpRequestBuilder.Post(HttpClientName)
                .ForResource<User>()
                .WithJson(json)
                .ToRequest();

            var res = await _factory.Send( req, ct, false );

            logger.Inspect(nameof(res.WasSuccessful), res.WasSuccessful);

            if( !res.WasSuccessful )
                return;


            // *****************************************************************
            logger.Debug("Attempting to fetch newly created user");

            var reqGet = HttpRequestBuilder.Get(HttpClientName)
                .ForResource("users")
                .AddParameter("username", request.NewUsername)
                .ToRequest();

            var (okGet, results) = await _factory.Many<User>(reqGet, ct);

            if( okGet )
            {
                var newUser = results.FirstOrDefault();

                if (newUser is null)
                    return;

                logger.LogObject(nameof(newUser), newUser);

                response.Created = true;
                response.IdentityUid = newUser.Id ?? "";
                response.Password = password;

            }

        }


        async Task Update()
        {

            response.IdentityUid = user.Id ?? "";
            response.Exists = true;


            var perform = false;

            if (!string.IsNullOrWhiteSpace(request.NewFirstName))
            {
                logger.Debug("Updating FirstName to {0}", request.NewFirstName);
                user.FirstName = request.NewFirstName;
                perform = true;
            }

            if (!string.IsNullOrWhiteSpace(request.NewLastName))
            {
                logger.Debug("Updating LastName to {0}", request.NewLastName);
                user.LastName = request.NewLastName;
                perform = true;
            }

            if (!string.IsNullOrWhiteSpace(request.NewEmail))
            {
                logger.Debug("Updating Email to {0}", request.NewEmail);
                user.Email = request.NewEmail;
                user.EmailVerified = !request.MustVerifyEmail;
                perform = true;
            }

            if( request.NewEnabled.HasValue )
            {
                logger.Debug("Updating Enabled to {0}", request.NewEnabled);
                user.Enabled = request.NewEnabled.Value;
                perform = true;
            }


            if (!string.IsNullOrWhiteSpace(request.NewPassword))
            {
                user.Credentials = new List<Credentials> { new() { Type = "password", UserLabel = "Custom", Value = request.NewPassword, Temporary = false } };
                perform = true;
            }


            // *****************************************************************
            if (perform)
            {

                logger.Debug("Attempting to Update existing User");
                var json = JsonSerializer.Serialize( user, DefaultOptions );
                var req = HttpRequestBuilder.Put(HttpClientName)
                    .ForResource<User>()
                    .WithIdentifier(user.Id??"x")
                    .WithJson(json)
                    .ToRequest();

                logger.Inspect(nameof(req), req);


                var res = await _factory.Send(req, ct, false);

                logger.LogObject(nameof(res), res );

                if (!res.WasSuccessful)
                    return;

                response.Updated = res.WasSuccessful;

            }


        }


    }



}




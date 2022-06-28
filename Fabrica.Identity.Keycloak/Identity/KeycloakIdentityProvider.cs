// ReSharper disable AccessToDisposedClosure
// ReSharper disable UnusedMember.Global

using System.Collections.ObjectModel;
using System.Security.Cryptography;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Text;
using Flurl.Http.Configuration;
using Keycloak.Net;
using Keycloak.Net.Models.Users;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


namespace Fabrica.Identity;

public class KeycloakIdentityProvider: CorrelatedObject, IIdentityProvider
{


    public KeycloakIdentityProvider( ICorrelation correlation, string endpoint, string realm, string clientId, string clientSecret ) : base(correlation)
    {

        Endpoint     = endpoint;
        Realm        = realm;
        ClientId     = clientId;
        ClientSecret = clientSecret;
    }

    private string Endpoint { get; }
    private string Realm { get; }
    private string ClientId { get; }
    private string ClientSecret { get; }


    private RandomNumberGenerator Rng { get; } = RandomNumberGenerator.Create();


    public async Task<SyncUserResponse> SyncUser( SyncUserRequest request )
    {

        using var logger = EnterMethod();


        logger.Inspect(nameof(Endpoint), Endpoint);
        logger.Inspect(nameof(Realm), Realm);
        logger.Inspect(nameof(ClientId), ClientId);
        logger.Inspect(nameof(ClientSecret), ClientSecret);

        var response = new SyncUserResponse();


        // *****************************************************************
        logger.Debug("Attempting to create Keycloak API client");
        var client = new KeycloakClient(Endpoint, ClientSecret );
        client.SetSerializer(new TheSerializer());



        // *****************************************************************
        logger.Debug("Attempting to dig out existing user by Uid then Username then Email");
        User? user = null;
        try
        {

            if( !string.IsNullOrWhiteSpace(request.IdentityUid) )
                user = await client.GetUserAsync(Realm, request.IdentityUid);

            if( user is null && !string.IsNullOrWhiteSpace(request.CurrentUsername) )
            {
                logger.Debug("Attempting to fetch by Username");
                var list = await client.GetUsersAsync(Realm, username: request.CurrentUsername);
                user = list.FirstOrDefault();
            }

            if( user is null && !string.IsNullOrWhiteSpace(request.CurrentEmail) )
            {
                logger.Debug("Attempting to fetch by Email");
                var list = await client.GetUsersAsync(Realm, email: request.CurrentEmail);
                user = list.FirstOrDefault();
            }

            logger.LogObject(nameof(user), user);


        }
        catch (Exception cause)
        {
            var ctx = new {Endpoint, Realm, Request = request};
            logger.ErrorWithContext( cause, ctx, "Fetching existing user failed" );
            throw;
        }


        try
        {

            if (user is null)
                await Create();
            else
                await Update();

        }
        catch (Exception cause)
        {
            var ctx = new { Endpoint, Realm, Request = request, Create=user is null };
            logger.ErrorWithContext(cause, ctx, "Sync user failed");
            throw;
        }



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


            if (actions.Count > 0)
                user.RequiredActions = new ReadOnlyCollection<string>(actions);

            if (request.Attributes.Count > 0)
                user.Attributes = new Dictionary<string, IEnumerable<string>>(request.Attributes);

            if (request.Groups.Count > 0)
                user.Groups = new List<string>(request.Groups);


            logger.LogObject(nameof(user), user);



            // *****************************************************************
            logger.Debug("Attempting to Create User");
            var created = await client.CreateUserAsync(Realm, user);

            logger.Inspect(nameof(created), created);
            logger.LogObject(nameof(user), user);



            // *****************************************************************
            logger.Debug("Attempting to fetch newly created user");
            var list = await client.GetUsersAsync(Realm, username: user.UserName);
            var newUser = list.FirstOrDefault();

            logger.LogObject(nameof(newUser), newUser);

            if (newUser is not null)
            {

                response.IdentityUid = newUser.Id;


                if( request.GeneratePassword )
                {

                    logger.Debug("Attempting to generate password");

                    var buf = new byte[16];
                    Rng.GetNonZeroBytes(buf);
                    var password = Base62Converter.Encode(buf);

                    var res = await client.SetUserPasswordAsync( Realm, newUser.Id, password );

                    logger.LogObject(nameof(res), res);

                    response.Password = password;

                }

            }


            response.Created = created;


        }



        async Task Update()
        {


            // *****************************************************************
            logger.Debug("Attempting to Update existing User");

            var perform = false;

            if( !string.IsNullOrWhiteSpace(request.NewFirstName) )
            {
                user.FirstName = request.NewFirstName;
                perform = true;
            }

            if( !string.IsNullOrWhiteSpace(request.NewLastName) )
            {
                user.LastName = request.NewLastName;
                perform = true;
            }

            if( !string.IsNullOrWhiteSpace(request.NewEmail) )
            {
                user.Email = request.NewEmail;
                user.EmailVerified = true;
                perform = true;
            }


            // *****************************************************************
            if (perform)
            {
                logger.Debug("Attempting to Update existing User");
                await client.UpdateUserAsync(Realm, user.Id, user);
            }


            response.IdentityUid = user.Id;
            response.Created = false;
            response.Password = "";


        }




    }


}


public class TheSerializer : ISerializer
{


    private static JsonSerializerSettings Settings { get; } = new ()
    {
        ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy { ProcessDictionaryKeys = false } }
    };


    public string Serialize(object obj)
    {

        var json = JsonConvert.SerializeObject(obj, Settings);
        return json;

    }

    public T? Deserialize<T>(string s)
    {
        var obj = JsonConvert.DeserializeObject<T>(s, Settings);
        return obj;
    }

    public T? Deserialize<T>(Stream stream)
    {
        using var reader = new StreamReader(stream);

        var json = reader.ReadToEnd();
        var obj = JsonConvert.DeserializeObject<T>(json, Settings);

        return obj;

    }

}

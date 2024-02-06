﻿# udap-devdays-2024
Spin up a local UDAP playground.  Includes FHIR server, static certificates server UDAP Auth Server and UDAP IDP Server.


[udap-dotnet](https://github.com/udap-tools/udap-dotnet) tutorial.

UDAP is the acronym for [Unified Data Access Profiles](https://www.udap.org/).
The HL7 "[Security IG](http://hl7.org/fhir/us/udap-security/)" is a constraint on UDAP.  The actual implementation guide has a long name of "Security for Scalable Registration, Authentication, and Authorization".

- FHIR® is the registered trademark of HL7 and is used with the permission of HL7. Use of the FHIR trademark does not constitute endorsement of the contents of this repository by HL7.
- UDAP® and the UDAP gear logo, ecosystem gears, and green lock designs are trademarks of UDAP.org. UDAP Draft Specifications are referenced and displayed in parts of this source code to document specification implementation.

In 2023 the [udap-devdays-2023](https://github.com/JoeShook/udap-devdays-2023) presentation contained detailed instructions on how to setup Objectives 1 through 3.  That presentation was very detailed concerning setup.
This years presentation will focus on Objective 4, Tiered OAuth and put less into the setup of the FHIR server and UDAP Auth Server details.  [udap-devdays-2023](https://github.com/JoeShook/udap-devdays-2023) is a good 
reference.   

## Objectives

1. 🧩 Host UDAP Metadata on a FHIR Server and secure access f
2. 🧩 Host UDAP Authorization Server and perform Dynamic Client Registration (DCR [RFC 7591](https://datatracker.ietf.org/doc/html/rfc7591))
3. 🧩 Secure the FHIR Server with UDAP
4. 🧩 Enabled Tiered OAuth and perform Dynamic Client Registration (DCR [RFC 7591](https://datatracker.ietf.org/doc/html/rfc7591)) with UDAP Auth Server actingas the client.



## Prerequisites

Clone the udap-dotnet repository.

````cli
git clone https://github.com/udap-tools/udap-dotnet.git
````

Ensure you can compile and run UdapEd.Server ahead of time.  It requires .NET 8.0.

We will run the UdapEd.Server project locally to test Discovery, DCR, Token Access and finally request a resource.  
Then we will let the user include the base URL of a preferred OpenID Connect Identity Provider (IdP).


## Things to look out for

If the developer regenerates certificates with the udap.pki.devdays project during the Tutorial delete the udap.authserver.devdays.EntityFramework.db database.  And restart udap.authserver.devdays.


### On Windows

If the developer regenerates certificates with the udap.pki.devdays during the Tutorial they may then need to launch mmc.exe, the Certificates snap-in for the current user.  Go to Intermediate Certification Authorities and delete the DevDaysSubCA_1.  


## Project Summaries

### udap.pki.devdays

This is a simple cli that generates all PKI needed for this desktop lab.  It generates a root CA, a sub CA, and a leaf UDAP certificate.  For the lab
three PKIs are generated representing three communities listed by name and type below 
  -  ``udap://Community1`` as RSA
  -  ``udap://Community2`` as ECDSA
  -  ``udap://Community3`` as a Revoked RSA client certificate.


### udap.certificates.server.devdays
This is a static certificate server, serving the certificate material that would typically be available for download.  One of those items is the intermediate certificates.
The other are the certificate revocation lists.  This allows you to test from your local machine without the gymnastics of setting up a real certificate server.

### udap.fhirserver.devdays

The FHIR server has one patient resource loaded.  This FHIR server is a simple DemoFileSystemFhirServer implementation of Brian Postlethwaite’s [fhir-net-web-api](https://github.com/brianpos/fhir-net-web-api/tree/feature/r4b). 

A member of the following communities
  -  ``udap://Community1``
  -  ``udap://Community2``
  -  ``udap://Community3``

### udap.authserver.devdays

Data Holder's Authorization Server.  
It is a Duende Identity Server implementation with a SQLite data store. We will be adding Udap.* packages to the
Identity Server to enabled UDAP for DCR and Tiered OAuth.

### udap.idp.server.devdays

Identity Provider's Authorization Server.  This is a UDAP enabled OpenID Connect Identity Provider.
A member of the ``udap://Community1`` community.


## Configuration

### configure udap.fhirserver.devdays

Starting from a basic FHIR Server.

```txt
dotnet add package Udap.Metadata.Server
```
#### :boom: Include services

```csharp
builder.Services.AddUdapMetadataServer(builder.Configuration);
```


#### :boom: Add Authentication

```txt
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer -v 8.*
```

Register services required by authentication services. Specifically the Bearer schema.

````csharp
  builder.Services.AddAuthentication(
    OidcConstants.AuthenticationSchemes.AuthorizationHeaderBearer)

    .AddJwtBearer(OidcConstants.AuthenticationSchemes.AuthorizationHeaderBearer, 
      options =>
      {
        options.Authority = builder.Configuration["Jwt:Authority"];
        options.RequireHttpsMetadata = 
            bool.Parse(
                builder.Configuration["Jwt:RequireHttpsMetadata"] ?? "true"
                );        
      
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
      }
    );
````

Add the AuthorizationMiddleware with the UseAuthentication() and UseAuthorization() extensions. 
Add the the authorization policy to the endpoints with the RequireAuthorization() extension. 
The order of the middleware is demonstrated in the following code.



#### :boom: Add Udap.Metadata to pipeline

Place it just before the MapControllers() extension method.

```csharp
  app.UseUdapMetadataServer();
```

The pipeline configuration should look like the following when complete.

```csharp
  app.UsePathBase(new PathString("/fhir/r4"));
  app.UseRouting();

  app.UseAuthentication();
  app.UseAuthorization();
  
  app.UseHttpsRedirection();
  app.UseUdapMetadataServer();
  app.MapControllers().RequireAuthorization();
```

##### :boom: Add Certificates and Configuration

- FHIR Server to the Authorization Server backchannel URL.

```json
  "Jwt": {
      "Authority": "https://localhost:5102"
}
```

- 
- Add the following UdapMetadataOptions section to appsettings.json

````json
  "UdapMetadataOptions": {
    "Enabled": true,
    "UdapMetadataConfigs": [
      {
        "Community": "udap://Community1",
        "SignedMetadataConfig": {
          "AuthorizationEndPoint": "https://localhost:5002/connect/authorize",
          "TokenEndpoint": "https://localhost:5002/connect/token",
          "RegistrationEndpoint": "https://localhost:5002/connect/register"
        }
      },
      {
        "Community": "udap://Community2",
        "SignedMetadataConfig": {
          "RegistrationSigningAlgorithms": [ "ES384" ],
          "TokenSigningAlgorithms": [ "ES384" ],
          "Issuer": "https://localhost:7016/fhir/r4",
          "Subject": "https://localhost:7016/fhir/r4",
          "AuthorizationEndPoint": "https://localhost:5002/connect/authorize",
          "TokenEndpoint": "https://localhost:5002/connect/token",
          "RegistrationEndpoint": "https://localhost:5002/connect/register"
        }
      },
      {
        "Community": "udap://Community3",
        "SignedMetadataConfig": {
          "AuthorizationEndPoint": "https://localhost:5002/connect/authorize",
          "TokenEndpoint": "https://localhost:5002/connect/token",
          "RegistrationEndpoint": "https://localhost:5002/connect/register"
        }
      }
    ]
  }
````

- Add the following UdapFileCertStoreManifest section to appsettings.json.  The CertificateStore folder has already been added 
to the project.  While it is a unsecure folder of certificates, you are free to implement your own ICertificateStore to
load certificates from a secure location such as an HSM.  

````json
  "UdapFileCertStoreManifest": {
    "Communities": [
      {
        "Name": "udap://Community1",
        "IssuedCerts": [
          {
            "FilePath": "CertificateStore/Community1/issued/DevDaysFhirServerRSAClient.pfx",
            "Password": "udap-test"
          }
        ]
      },
      {
        "Name": "udap://Community2",
        "IssuedCerts": [
          {
            "FilePath": "CertificateStore/Community2/issued/DevDaysECDSAClient.pfx",
            "Password": "udap-test"
          }
        ]
      },
      {
        "Name": "udap://Community3",
        "IssuedCerts": [
          {
            "FilePath": "CertificateStore/Community3/issued/DevDaysRevokedClient.pfx",
            "Password": "udap-test"
          }
        ]
      }
    ]     
  }
````



#### :boom: Run udap.fhirserver.devdays Project

- [https://localhost:7017/fhir/r4?_format=json](https://localhost:7017/fhir/r4?_format=json)
- [https://localhost:7017/fhir/r4/Patient](https://localhost:7017/fhir/r4/Patient)  (Need a token)

Default UDAP metadata endpoint.

- [https://localhost:7017/fhir/r4/.well-known/udap](https://localhost:7017/fhir/r4/.well-known/udap)

Convenience links to find community specific UDAP metadata endpoints

- [https://localhost:7017/fhir/r4/.well-known/udap/communities](https://localhost:7017/fhir/r4/.well-known/udap/communities)
- [https://localhost:7017/fhir/r4/.well-known/udap/communities/ashtml](https://localhost:7017/fhir/r4/.well-known/udap/communities/ashtml)


### configure udap.authserver.devdays

#### Use ``IUdapServiceBuilder`` to configure the UDAP server via the ``AddUdapServer`` extension method. 

  - options supply UDAP server options.
  - storeOptionAction indicates database type and connection string.
  - baseUrl is important and must not be left out.
  - AddUdapResonseGenerators() is required to augment IdTokens from Tiered Oauth Identity Providers to propogate the hl7_identifier._
  - AddSmrtV2Expander() implements rules to expand scopes where the scope parameter part may represent an encoded set of scopes like wild cards. 


```csharp
  builder.Services.AddUdapServer(
    options =>
    {
        var udapServerOptions = builder.Configuration.GetOption<ServerSettings>("ServerSettings");
        options.DefaultSystemScopes = udapServerOptions.DefaultSystemScopes;
        options.DefaultUserScopes = udapServerOptions.DefaultUserScopes;
        options.ServerSupport = udapServerOptions.ServerSupport;
        options.ForceStateParamOnAuthorizationCode = udapServerOptions.
            ForceStateParamOnAuthorizationCode;
    },
    storeOptionAction: options =>
        options.UdapDbContext = b =>
            b.UseSqlite(connectionString,
                dbOpts =>
                    dbOpts.MigrationsAssembly(typeof(Program).Assembly.FullName)),
    baseUrl: "https://localhost:5102/connect/register"
    )
    .AddUdapResponseGenerators()
    .AddSmartV2Expander();
```

#### Use the MS AuthenticationBuilder to add a standard Tiered OauthHandler implementation

```csharp
  builder.Services.AddAuthentication()
    .AddTieredOAuth(options =>
    {
        options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
    });
```

#### pipeline configuration

The flowing layout will plug Udap into the pipeline.  It is important for ``UseUdapServer()`` to 
be placed before ``UseIdentityServer()``.

``SeedData`` is just a convenience utility top populate the database for this lab.

```csharp
  var app = builder.Build();

  //
  // Configure the HTTP request pipeline.
  //

  app.UseSerilogRequestLogging();
  if (app.Environment.IsDevelopment())
  {
      app.UseDeveloperExceptionPage();
  }

  await SeedData.InitializeDatabase(app, Log.Logger);

  app.UseStaticFiles();
  app.UseRouting();

  app.UseUdapServer();
  app.UseIdentityServer();

  app.UseAuthorization();
  app.MapRazorPages().RequireAuthorization();

  app.Run();
```



### configure udap.idp.server.devdays


#### Use ``IUdapServiceBuilder`` to configure the Idp UDAP server via the ``AddUdapServerAsIdentityProvider`` extension method. 

```csharp
  builder.Services.AddUdapServerAsIdentityProvider(
        options =>
        {
            var udapServerOptions = builder.Configuration.GetOption<ServerSettings>("ServerSettings");
            options.DefaultSystemScopes = udapServerOptions.DefaultSystemScopes;
            options.DefaultUserScopes = udapServerOptions.DefaultUserScopes;
            options.ServerSupport = udapServerOptions.ServerSupport;
            options.ForceStateParamOnAuthorizationCode = udapServerOptions.ForceStateParamOnAuthorizationCode;
            options.LogoRequired = udapServerOptions.LogoRequired;
            options.AlwaysIncludeUserClaimsInIdToken = udapServerOptions.AlwaysIncludeUserClaimsInIdToken;
        },
        storeOptionAction: options =>
            options.UdapDbContext = b =>
                b.UseSqlite(connectionString,
                    dbOpts =>
                      dbOpts.MigrationsAssembly(typeof(Program).Assembly.FullName))
            )
    .AddPrivateFileStore();
```

Then the following two lines will enabled the Idp UDAP Auth server to present signed UDAP metadata so other UDAP Auth Servers can auto register.

```csharp

  builder.Services.Configure<UdapFileCertStoreManifest>(builder.Configuration.GetSection(Constants.UDAP_FILE_STORE_MANIFEST));

  builder.Services.AddUdapMetadataServer(builder.Configuration);

```


#### pipeline configuration
And finally this pipeline configuration will look similar to the AuthServer with the addition of a MedatadataSErver endpoint.

```csharp
  var app = builder.Build();

  // Configure the HTTP request pipeline.

  app.UseHsts();

  app.UseSerilogRequestLogging();


  await SeedData.InitializeDatabase(app, "../../../../udap.pki.devdays/CertificateStore", Log.Logger);

  // uncomment if you want to add a UI
  app.UseStaticFiles();
  app.UseRouting();

  app.UseUdapMetadataServer();
  app.UseUdapIdPServer();
  app.UseIdentityServer();


  // uncomment if you want to add a UI
  app.UseAuthorization();
  app.MapRazorPages().RequireAuthorization();

  app.Run();
```


